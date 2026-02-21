<script lang="ts">
  import { page } from '$app/stores';
  import * as orderApi from '$api/order';
  import * as paymentApi from '$api/payment';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Order, OrderStatus, PaymentRecord } from '$types';

  let orderId = $derived($page.params.id!);
  let loading = $state(true);
  let order = $state<Order | null>(null);
  let payments = $state<PaymentRecord[]>([]);
  let loadError = $state<string | null>(null);

  let newStatusId = $state(0);
  let changingStatus = $state(false);

  let trackingInput = $state('');
  let showTrackingModal = $state(false);
  let addingTracking = $state(false);

  let showCancelModal = $state(false);
  let canceling = $state(false);

  const statusIdMap: Record<string, number> = {
    New: 1, PendingPayment: 2, Paid: 3, Processing: 4,
    Shipped: 5, Delivered: 6, Completed: 7, Canceled: 8, Refunded: 9,
  };

  const statusNameMap: Record<number, string> = {
    1: 'New', 2: 'PendingPayment', 3: 'Paid', 4: 'Processing',
    5: 'Shipped', 6: 'Delivered', 7: 'Completed', 8: 'Canceled', 9: 'Refunded',
  };

  const allStatusOptions = $derived([
    { value: 1, label: i18n.t('admin.orderStatusNew') },
    { value: 2, label: i18n.t('admin.orderStatusPendingPayment') },
    { value: 3, label: i18n.t('admin.orderStatusPaid') },
    { value: 4, label: i18n.t('admin.orderStatusProcessing') },
    { value: 5, label: i18n.t('admin.orderStatusShipped') },
    { value: 6, label: i18n.t('admin.orderStatusDelivered') },
    { value: 7, label: i18n.t('admin.orderStatusCompleted') },
    { value: 8, label: i18n.t('admin.orderStatusCanceled') },
    { value: 9, label: i18n.t('admin.orderStatusRefunded') },
  ]);

  const sellerAllowedStatuses = new Set([4, 5, 6]); // Processing, Shipped, Delivered

  let statusOptions = $derived(
    authStore.isAdmin
      ? allStatusOptions
      : allStatusOptions.filter((opt) => sellerAllowedStatuses.has(opt.value))
  );

  $effect(() => {
    loadOrder(orderId);
  });

  async function loadOrder(id: string) {
    try {
      loading = true;
      loadError = null;
      const [orderRes, paymentsRes] = await Promise.all([
        orderApi.getOrderById(id),
        paymentApi.getPaymentsByOrder(id).catch(() => [] as PaymentRecord[]),
      ]);
      order = orderRes;
      payments = paymentsRes;
      newStatusId = statusIdMap[orderRes.status] ?? 1;
    } catch (err: unknown) {
      const status = (err as { status?: number })?.status
        ?? (err as { response?: { status?: number } })?.response?.status;
      if (status === 403) {
        loadError = i18n.t('admin.accessDenied');
      } else if (status === 404) {
        loadError = i18n.t('admin.orderNotFound');
      } else {
        loadError = i18n.t('admin.errorLoadingOrder');
      }
      notificationStore.error(loadError);
    } finally {
      loading = false;
    }
  }

  async function changeStatus() {
    if (!order || !newStatusId || statusNameMap[newStatusId] === order.status) return;
    try {
      changingStatus = true;
      await orderApi.updateOrderStatus(order.id, { statusId: newStatusId });
      order = { ...order, status: (statusNameMap[newStatusId] ?? order.status) as OrderStatus };
      notificationStore.success(i18n.t('admin.statusChanged'));
    } catch {
      notificationStore.error(i18n.t('admin.errorChangingStatus'));
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
      notificationStore.success(i18n.t('admin.trackingAdded'));
    } catch {
      notificationStore.error(i18n.t('admin.errorAddingTracking'));
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
      notificationStore.success(i18n.t('admin.orderCanceled'));
    } catch {
      notificationStore.error(i18n.t('admin.errorCanceling'));
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
      New: i18n.t('admin.orderStatusNew'), PendingPayment: i18n.t('admin.orderStatusPendingPayment'),
      Paid: i18n.t('admin.orderStatusPaid'), Processing: i18n.t('admin.orderStatusProcessing'),
      Shipped: i18n.t('admin.orderStatusShipped'), Delivered: i18n.t('admin.orderStatusDelivered'),
      Completed: i18n.t('admin.orderStatusCompleted'), Canceled: i18n.t('admin.orderStatusCanceled'), Refunded: i18n.t('admin.orderStatusRefunded'),
    };
    return map[status] ?? status;
  }

  function paymentStatusLabel(status: string): string {
    const map: Record<string, string> = {
      Pending: i18n.t('admin.paymentPending'), Succeeded: i18n.t('admin.paymentSucceeded'),
      Failed: i18n.t('admin.paymentFailed'), Refunded: i18n.t('admin.paymentRefunded'),
    };
    return map[status] ?? status;
  }

  const statusFlowSteps = ['New', 'Paid', 'Processing', 'Shipped', 'Delivered', 'Completed'] as const;

  let statusFlow = $derived.by(() => {
    const o = order;
    if (!o) return [];
    const currentIdx = statusFlowSteps.indexOf(o.status as typeof statusFlowSteps[number]);
    const isCanceled = o.status === 'Canceled' || o.status === 'Refunded';

    return statusFlowSteps.map((step, idx) => ({
      label: statusLabel(step),
      completed: !isCanceled && currentIdx >= 0 && idx < currentIdx,
      current: !isCanceled && step === o.status,
      canceled: isCanceled && idx === 0,
    }));
  });
</script>

<div class="order-detail">
  <a href="/admin/orders" class="back-link">← {i18n.t('admin.orders')}</a>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingOrder')} />
  {:else if order}
    <div class="detail-header">
      <div class="header-left">
        <h1 class="page-title">{i18n.t('admin.orderHash', { id: order.id.slice(0, 8) })}</h1>
        <span class="badge {statusBadge(order.status)}">
          {statusLabel(order.status)}
        </span>
      </div>
      <div class="header-actions">
        {#if order.status !== 'Canceled' && order.status !== 'Refunded'}
          <button class="btn btn-outline btn-sm" onclick={() => (showTrackingModal = true)}>
            📦 {i18n.t('admin.addTracking')}
          </button>
          <button
            class="btn btn-sm"
            style="background: var(--color-error); color: #fff;"
            onclick={() => (showCancelModal = true)}
          >
            {i18n.t('common.cancel')}
          </button>
        {/if}
      </div>
    </div>

    <!-- Status progression -->
    <section class="status-progression card">
      <div class="card-body">
        <div class="progression-bar">
          {#each statusFlow as step}
            <div
              class="progression-step"
              class:completed={step.completed}
              class:current={step.current}
              class:canceled={step.canceled}
            >
              <div class="step-dot"></div>
              <span class="step-label">{step.label}</span>
            </div>
          {/each}
        </div>
      </div>
    </section>

    <div class="detail-grid">
      <div class="detail-main">
        <!-- Status change -->
        <section class="detail-section card">
          <div class="card-body">
            <h3 class="section-title">{i18n.t('admin.changeStatus')}</h3>
            <div class="status-change-row">
              <select class="input input-sm" bind:value={newStatusId} style="max-width: 240px;">
                {#each statusOptions as opt}
                  <option value={opt.value}>{opt.label}</option>
                {/each}
              </select>
              <button
                class="btn btn-primary btn-sm"
                onclick={changeStatus}
                disabled={changingStatus || statusNameMap[newStatusId] === order.status}
              >
                {i18n.t('admin.changeStatus')}
              </button>
            </div>
          </div>
        </section>

        <!-- Customer info -->
        <div class="info-cards-row">
          <section class="detail-section card">
            <div class="card-body">
              <h3 class="section-title">👤 {i18n.t('admin.customer')}</h3>
              <p><strong>{i18n.t('admin.ownerName')}:</strong> {order.shippingAddress.fullName}</p>
              <p>
                <strong>Email:</strong>
                <a href="mailto:{order.buyerEmail}" class="email-link">{order.buyerEmail}</a>
              </p>
              <p><strong>{i18n.t('admin.phone')}:</strong> {order.shippingAddress.phone}</p>
            </div>
          </section>

          <section class="detail-section card">
            <div class="card-body">
              <h3 class="section-title">📍 {i18n.t('admin.shippingAddress')}</h3>
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
            <h3 class="section-title">📦 {i18n.t('admin.tracking')}</h3>
            {#if order.trackingNumber}
              <p class="tracking-number">{order.trackingNumber}</p>
            {:else}
              <p class="text-muted">{i18n.t('admin.noTrackingYet')}</p>
            {/if}
          </div>
        </section>

        <!-- Order items -->
        <section class="detail-section card">
          <div class="card-header">
            <h3>🛒 {i18n.t('admin.orderItems')}</h3>
          </div>
          <div class="card-body" style="padding: 0;">
            <div class="table-wrapper">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>{i18n.t('admin.photo')}</th>
                    <th>{i18n.t('admin.name')}</th>
                    <th>{i18n.t('admin.variant')}</th>
                    <th>{i18n.t('admin.personalization')}</th>
                    <th>{i18n.t('admin.quantity')}</th>
                    <th>{i18n.t('admin.price')}</th>
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
                      <td class="cell-title">
                        <a href="/admin/products/{item.productId}" class="product-link">
                          {item.productName}
                        </a>
                      </td>
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
                    <td colspan="5" class="total-label">{i18n.t('admin.total')}:</td>
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
              <h3>{i18n.t('admin.payments')}</h3>
            </div>
            <div class="card-body" style="padding: 0;">
              <div class="table-wrapper">
                <table class="data-table">
                  <thead>
                    <tr>
                      <th>{i18n.t('admin.provider')}</th>
                      <th>{i18n.t('admin.amount')}</th>
                      <th>{i18n.t('admin.status')}</th>
                      <th>{i18n.t('admin.date')}</th>
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
            <h3 class="section-title">{i18n.t('admin.timeline')}</h3>
            <div class="timeline">
              <div class="timeline-item">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">{i18n.t('admin.created')}</span>
                  <span class="timeline-date">{formatDate(order.createdAt)}</span>
                </div>
              </div>
              <div class="timeline-item">
                <div class="timeline-dot active"></div>
                <div class="timeline-content">
                  <span class="timeline-label">{i18n.t('admin.currentStatus')}</span>
                  <span class="badge {statusBadge(order.status)} mt-1">
                    {statusLabel(order.status)}
                  </span>
                </div>
              </div>
              <div class="timeline-item">
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <span class="timeline-label">{i18n.t('admin.updated')}</span>
                  <span class="timeline-date">{formatDate(order.updatedAt)}</span>
                </div>
              </div>
            </div>
          </div>
        </section>
      </aside>
    </div>
  {:else}
    <div class="error-message card">
      <div class="card-body" style="text-align: center; padding: var(--space-8);">
        <p class="text-muted" style="font-size: 1.125rem;">{loadError ?? i18n.t('admin.orderNotFound')}</p>
        <a href="/admin/orders" class="btn btn-outline" style="margin-top: var(--space-4);">{i18n.t('common.back')}</a>
      </div>
    </div>
  {/if}
</div>

<Modal
  open={showTrackingModal}
  title={i18n.t('admin.addTrackingNumber')}
  onclose={() => (showTrackingModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="tracking">{i18n.t('admin.trackingNumberLabel')}</label>
    <input
      id="tracking"
      class="input"
      type="text"
      bind:value={trackingInput}
      placeholder="UA1234567890"
    />
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showTrackingModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-primary" onclick={addTracking} disabled={addingTracking}>
      {addingTracking ? i18n.t('common.saving') : i18n.t('common.add')}
    </button>
  </div>
</Modal>

<Modal
  open={showCancelModal}
  title={i18n.t('admin.cancelOrderQuestion')}
  onclose={() => (showCancelModal = false)}
>
  <p>{i18n.t('admin.confirmCancelOrder')}</p>
  <p class="text-sm text-muted mt-2">{i18n.t('admin.actionIrreversible')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showCancelModal = false)}>{i18n.t('common.no')}</button>
    <button class="btn" style="background: var(--color-error); color: #fff;" onclick={cancelOrder} disabled={canceling}>
      {canceling ? i18n.t('admin.canceling') : i18n.t('admin.cancelOrder')}
    </button>
  </div>
</Modal>

<style>
  .back-link {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    color: var(--color-text-light);
    text-decoration: none;
    font-size: 0.875rem;
    margin-bottom: var(--space-4);
    transition: color var(--transition-fast);
  }

  .back-link:hover {
    color: var(--color-primary);
  }

  .page-title {
    font-size: 1.5rem;
  }

  .detail-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-4);
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

  /* Status progression */
  .status-progression {
    margin-bottom: var(--space-6);
  }

  .progression-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-2);
    position: relative;
  }

  .progression-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-1);
    flex: 1;
    position: relative;
  }

  .progression-step:not(:last-child)::after {
    content: '';
    position: absolute;
    top: 10px;
    left: calc(50% + 12px);
    right: calc(-50% + 12px);
    height: 2px;
    background: var(--color-border);
  }

  .progression-step.completed:not(:last-child)::after {
    background: var(--color-success);
  }

  .step-dot {
    width: 20px;
    height: 20px;
    border-radius: var(--radius-full);
    background: var(--color-border);
    border: 3px solid var(--color-surface);
    box-shadow: 0 0 0 2px var(--color-border);
    z-index: 1;
    transition: all var(--transition-fast);
  }

  .progression-step.completed .step-dot {
    background: var(--color-success);
    box-shadow: 0 0 0 2px var(--color-success);
  }

  .progression-step.current .step-dot {
    background: var(--color-primary);
    box-shadow: 0 0 0 2px var(--color-primary), 0 0 0 5px color-mix(in srgb, var(--color-primary) 20%, transparent);
  }

  .progression-step.canceled .step-dot {
    background: var(--color-error);
    box-shadow: 0 0 0 2px var(--color-error);
  }

  .step-label {
    font-size: 0.6875rem;
    font-weight: 500;
    color: var(--color-text-muted);
    text-align: center;
    white-space: nowrap;
  }

  .progression-step.completed .step-label {
    color: var(--color-success);
  }

  .progression-step.current .step-label {
    color: var(--color-primary);
    font-weight: 700;
  }

  .email-link {
    color: var(--color-primary);
    text-decoration: none;
  }

  .email-link:hover {
    text-decoration: underline;
  }

  .product-link {
    color: var(--color-primary);
    text-decoration: none;
    font-weight: 500;
  }

  .product-link:hover {
    text-decoration: underline;
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
    .progression-bar {
      flex-wrap: wrap;
    }
  }
</style>
