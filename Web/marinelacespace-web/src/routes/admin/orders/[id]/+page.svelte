<script lang="ts">
  import { page } from '$app/stores';
  import * as orderApi from '$api/order';
  import * as paymentApi from '$api/payment';
  import Breadcrumb from '$components/Breadcrumb.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Order, OrderStatus, PaymentRecord } from '$types';

  let orderId = $derived($page.params.id);
  let loading = $state(true);
  let order = $state<Order | null>(null);
  let payments = $state<PaymentRecord[]>([]);

  // Status change
  let newStatus = $state('');
  let changingStatus = $state(false);

  // Tracking
  let trackingInput = $state('');
  let showTrackingModal = $state(false);
  let addingTracking = $state(false);

  // Cancel
  let showCancelModal = $state(false);
  let canceling = $state(false);

  const breadcrumbItems = $derived([
    { label: 'Адмін', href: '/admin' },
    { label: 'Замовлення', href: '/admin/orders' },
    { label: order ? `#${order.id.slice(0, 8)}` : '...' },
  ]);

  const statusOptions: { value: OrderStatus; label: string }[] = [
    { value: 'New', label: 'Нове' },
    { value: 'PendingPayment', label: 'Очікує оплати' },
    { value: 'Paid', label: 'Оплачено' },
    { value: 'Processing', label: 'В обробці' },
    { value: 'Shipped', label: 'Відправлено' },
    { value: 'Delivered', label: 'Доставлено' },
    { value: 'Completed', label: 'Завершено' },
    { value: 'Canceled', label: 'Скасовано' },
    { value: 'Refunded', label: 'Повернення' },
  ];

  $effect(() => {
    loadOrder(orderId);
  });

  async function loadOrder(id: string) {
    try {
      loading = true;
      const [orderRes, paymentsRes] = await Promise.all([
        orderApi.getOrderById(id),
        paymentApi.getPaymentsByOrder(id).catch(() => [] as PaymentRecord[]),
      ]);
      order = orderRes;
      payments = paymentsRes;
      newStatus = orderRes.status;
    } catch {
      notificationStore.error('Помилка завантаження замовлення');
    } finally {
      loading = false;
    }
  }

  async function changeStatus() {
    if (!order || !newStatus || newStatus === order.status) return;
    try {
      changingStatus = true;
      await orderApi.updateOrderStatus(order.id, { status: newStatus });
      order = { ...order, status: newStatus as OrderStatus };
      notificationStore.success('Статус змінено');
    } catch {
      notificationStore.error('Помилка зміни статусу');
    } finally {
      changingStatus = false;
    }
  }

  async function addTracking() {
    if (!order || !trackingInput.trim()) return;
    try {
      addingTracking = true;
      await orderApi.addTrackingNumber(order.id, { trackingNumber: trackingInput.trim() });
      order = { ...order, trackingNumber: trackingInput.trim() };
      showTrackingModal = false;
      trackingInput = '';
      notificationStore.success('Трекінг-номер додано');
    } catch {
      notificationStore.error('Помилка додавання трекінг-номера');
    } finally {
      addingTracking = false;
    }
  }

  async function cancelOrder() {
    if (!order) return;
    try {
      canceling = true;
      await orderApi.cancelOrder(order.id);
      order = { ...order, status: 'Canceled' };
      showCancelModal = false;
      notificationStore.success('Замовлення скасовано');
    } catch {
      notificationStore.error('Помилка скасування');
    } finally {
      canceling = false;
    }
  }

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  function formatCurrency(value: number): string {
    return `₴${value.toLocaleString('uk-UA', { minimumFractionDigits: 2 })}`;
  }

  function statusBadge(status: string): string {
    const map: Record<string, string> = {
      New: 'badge-info', PendingPayment: 'badge-info',
      Paid: 'badge-success', Processing: 'badge-warning',
      Shipped: 'badge-warning', Delivered: 'badge-secondary',
      Completed: 'badge-success', Canceled: 'badge-error', Refunded: 'badge-error',
    };
    return map[status] ?? 'badge-outline';
  }

  function statusLabel(status: string): string {
    const map: Record<string, string> = {
      New: 'Нове', PendingPayment: 'Очікує оплати',
      Paid: 'Оплачено', Processing: 'В обробці',
      Shipped: 'Відправлено', Delivered: 'Доставлено',
      Completed: 'Завершено', Canceled: 'Скасовано', Refunded: 'Повернення',
    };
    return map[status] ?? status;
  }

  function paymentStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Pending: 'Очікує', Succeeded: 'Успішно',
      Failed: 'Невдало', Refunded: 'Повернено',
    };
    return map[status] ?? status;
  }
</script>

<div class="order-detail">
  <Breadcrumb items={breadcrumbItems} />

  {#if loading}
    <LoadingSpinner message="Завантаження замовлення..." />
  {:else if order}
    <div class="detail-header">
      <div class="header-left">
        <h1 class="page-title">Замовлення #{order.id.slice(0, 8)}</h1>
        <span class="badge {statusBadge(order.status)}">
          {statusLabel(order.status)}
        </span>
      </div>
      <div class="header-actions">
        {#if order.status !== 'Canceled' && order.status !== 'Refunded'}
          <button class="btn btn-outline btn-sm" on:click={() => (showTrackingModal = true)}>
            Додати трекінг
          </button>
          <button
            class="btn btn-sm"
            style="background: var(--color-error); color: #fff;"
            on:click={() => (showCancelModal = true)}
          >
            Скасувати
          </button>
        {/if}
      </div>
    </div>

    <div class="detail-grid">
      <div class="detail-main">
        <!-- Status change -->
        <section class="detail-section card">
          <div class="card-body">
            <h3 class="section-title">Змінити статус</h3>
            <div class="status-change-row">
              <select class="input input-sm" bind:value={newStatus} style="max-width: 240px;">
                {#each statusOptions as opt}
                  <option value={opt.value}>{opt.label}</option>
                {/each}
              </select>
              <button
                class="btn btn-primary btn-sm"
                on:click={changeStatus}
                disabled={changingStatus || newStatus === order.status}
              >
                Змінити статус
              </button>
            </div>
          </div>
        </section>

        <!-- Customer info -->
        <div class="info-cards-row">
          <section class="detail-section card">
            <div class="card-body">
              <h3 class="section-title">Покупець</h3>
              <p><strong>Ім'я:</strong> {order.shippingAddress.fullName}</p>
              <p><strong>Email:</strong> {order.buyerEmail}</p>
              <p><strong>Телефон:</strong> {order.shippingAddress.phone}</p>
            </div>
          </section>

          <section class="detail-section card">
            <div class="card-body">
              <h3 class="section-title">Адреса доставки</h3>
              <p>{order.shippingAddress.addressLine1}</p>
              {#if order.shippingAddress.addressLine2}
                <p>{order.shippingAddress.addressLine2}</p>
              {/if}
              <p>
                {order.shippingAddress.city}, {order.shippingAddress.state}
                {order.shippingAddress.postalCode}
              </p>
              <p>{order.shippingAddress.country}</p>
            </div>
          </section>
        </div>

        <!-- Tracking -->
        <section class="detail-section card">
          <div class="card-body">
            <h3 class="section-title">Трекінг</h3>
            {#if order.trackingNumber}
              <p class="tracking-number">{order.trackingNumber}</p>
            {:else}
              <p class="text-muted">Трекінг-номер ще не додано</p>
            {/if}
          </div>
        </section>

        <!-- Order items -->
        <section class="detail-section card">
          <div class="card-header">
            <h3>Товари замовлення</h3>
          </div>
          <div class="card-body" style="padding: 0;">
            <div class="table-wrapper">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Фото</th>
                    <th>Назва</th>
                    <th>Варіант</th>
                    <th>Персоналізація</th>
                    <th>Кількість</th>
                    <th>Ціна</th>
                  </tr>
                </thead>
                <tbody>
                  {#each order.items as item}
                    <tr>
                      <td>
                        {#if item.imageUrl}
                          <img src={item.imageUrl} alt={item.productName} class="item-thumb" />
                        {:else}
                          <div class="item-thumb placeholder">📷</div>
                        {/if}
                      </td>
                      <td class="cell-title">{item.productName}</td>
                      <td class="text-sm">
                        {[item.sizeName, item.colorName, item.materialName]
                          .filter(Boolean)
                          .join(' / ') || '—'}
                      </td>
                      <td class="text-sm">{item.personalization ?? '—'}</td>
                      <td>{item.quantity}</td>
                      <td class="cell-mono">{formatCurrency(item.unitPrice * item.quantity)}</td>
                    </tr>
                  {/each}
                </tbody>
                <tfoot>
                  <tr>
                    <td colspan="5" class="total-label">Загалом:</td>
                    <td class="cell-mono total-value">{formatCurrency(order.totalPrice)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </section>

        <!-- Payment info -->
        {#if payments.length > 0}
          <section class="detail-section card">
            <div class="card-header">
              <h3>Платежі</h3>
            </div>
            <div class="card-body" style="padding: 0;">
              <div class="table-wrapper">
                <table class="data-table">
                  <thead>
                    <tr>
                      <th>Провайдер</th>
                      <th>Сума</th>
                      <th>Статус</th>
                      <th>Дата</th>
                    </tr>
                  </thead>
                  <tbody>
                    {#each payments as payment}
                      <tr>
                        <td>{payment.provider}</td>
                        <td class="cell-mono">
                          {payment.currency === 'UAH' ? '₴' : payment.currency}{payment.amount.toFixed(2)}
                        </td>
                        <td>
                          <span class="badge {payment.status === 'Succeeded' ? 'badge-success' : payment.status === 'Failed' ? 'badge-error' : 'badge-outline'}">
                            {paymentStatusLabel(payment.status)}
                          </span>
                        </td>
                        <td>{formatDate(payment.createdAt)}</td>
                      </tr>
                    {/each}
                  </tbody>
                </table>
              </div>
            </div>
          </section>
        {/if}
      </div>

      <!-- Timeline sidebar -->
      <aside class="detail-sidebar">
        <section class="card">
          <div class="card-body">
            <h3 class="section-title">Хронологія</h3>
            <div class="timeline">
              <div class="timeline-item">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Створено</span>
                  <span class="timeline-date">{formatDate(order.createdAt)}</span>
                </div>
              </div>
              <div class="timeline-item">
                <div class="timeline-dot active"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Поточний статус</span>
                  <span class="badge {statusBadge(order.status)} mt-1">
                    {statusLabel(order.status)}
                  </span>
                </div>
              </div>
              <div class="timeline-item">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">Оновлено</span>
                  <span class="timeline-date">{formatDate(order.updatedAt)}</span>
                </div>
              </div>
            </div>
          </div>
        </section>
      </aside>
    </div>
  {:else}
    <p class="text-muted">Замовлення не знайдено.</p>
  {/if}
</div>

<Modal
  open={showTrackingModal}
  title="Додати трекінг-номер"
  onclose={() => (showTrackingModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="tracking">Номер відстеження</label>
    <input
      id="tracking"
      class="input"
      type="text"
      bind:value={trackingInput}
      placeholder="UA1234567890"
    />
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showTrackingModal = false)}>Скасувати</button>
    <button class="btn btn-primary" on:click={addTracking} disabled={addingTracking}>
      {addingTracking ? 'Збереження...' : 'Додати'}
    </button>
  </div>
</Modal>

<Modal
  open={showCancelModal}
  title="Скасувати замовлення?"
  onclose={() => (showCancelModal = false)}
>
  <p>Ви впевнені, що хочете скасувати це замовлення?</p>
  <p class="text-sm text-muted mt-2">Цю дію неможливо скасувати.</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showCancelModal = false)}>Ні</button>
    <button class="btn" style="background: var(--color-error); color: #fff;" on:click={cancelOrder} disabled={canceling}>
      {canceling ? 'Скасування...' : 'Скасувати замовлення'}
    </button>
  </div>
</Modal>

<style>
  .page-title {
    font-size: 1.5rem;
  }

  .detail-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .header-left {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .header-actions {
    display: flex;
    gap: var(--space-2);
  }

  .detail-grid {
    display: grid;
    grid-template-columns: 1fr 280px;
    gap: var(--space-6);
    align-items: start;
  }

  .detail-main {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .detail-section {
    overflow: hidden;
  }

  .section-title {
    font-size: 0.9375rem;
    font-weight: 600;
    margin-bottom: var(--space-3);
    font-family: var(--font-body);
  }

  .info-cards-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-4);
  }

  .status-change-row {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .tracking-number {
    font-family: monospace;
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-primary);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .data-table {
    width: 100%;
    font-size: 0.875rem;
  }

  .data-table th {
    text-align: left;
    padding: var(--space-3) var(--space-4);
    font-weight: 600;
    color: var(--color-text-light);
    border-bottom: 2px solid var(--color-border);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    white-space: nowrap;
  }

  .data-table td {
    padding: var(--space-3) var(--space-4);
    border-bottom: 1px solid var(--color-border-light);
    vertical-align: middle;
  }

  .data-table tbody tr:hover {
    background: var(--color-surface-hover);
  }

  .data-table tfoot td {
    border-top: 2px solid var(--color-border);
    border-bottom: none;
  }

  .item-thumb {
    width: 40px;
    height: 40px;
    border-radius: var(--radius-sm);
    object-fit: cover;
  }

  .item-thumb.placeholder {
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--color-border-light);
    font-size: 1rem;
  }

  .cell-title {
    font-weight: 500;
  }

  .cell-mono {
    font-family: monospace;
    font-size: 0.8125rem;
  }

  .total-label {
    text-align: right;
    font-weight: 700;
  }

  .total-value {
    font-weight: 700;
    font-size: 1rem;
  }

  .text-sm {
    font-size: 0.8125rem;
  }

  /* Timeline */
  .timeline {
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
    position: relative;
    padding-left: var(--space-5);
  }

  .timeline::before {
    content: '';
    position: absolute;
    left: 6px;
    top: 8px;
    bottom: 8px;
    width: 2px;
    background: var(--color-border);
  }

  .timeline-item {
    display: flex;
    gap: var(--space-3);
    position: relative;
  }

  .timeline-dot {
    width: 14px;
    height: 14px;
    border-radius: var(--radius-full);
    background: var(--color-border);
    border: 2px solid var(--color-surface);
    position: absolute;
    left: calc(-1 * var(--space-5));
    top: 2px;
    z-index: 1;
  }

  .timeline-dot.active {
    background: var(--color-primary);
  }

  .timeline-content {
    display: flex;
    flex-direction: column;
  }

  .timeline-label {
    font-size: 0.8125rem;
    font-weight: 500;
  }

  .timeline-date {
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .form-label {
    font-size: 0.8125rem;
    font-weight: 600;
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }

  @media (max-width: 1024px) {
    .detail-grid {
      grid-template-columns: 1fr;
    }
    .info-cards-row {
      grid-template-columns: 1fr;
    }
  }
</style>
