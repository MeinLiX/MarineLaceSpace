<script lang="ts">
  import { page } from '$app/stores';
  import { i18n } from '$i18n/index.svelte';
  import type { Order, OrderStatus } from '$types';
  import * as orderApi from '$lib/api/order';
  import { notificationStore } from '$lib/stores/notification.svelte';
  import LoadingSpinner from '$lib/components/LoadingSpinner.svelte';

  let order = $state<Order | null>(null);
  let isLoading = $state(true);
  let isCanceling = $state(false);

  let orderId = $derived($page.params.id ?? '');

  let statusLabels: Record<OrderStatus, string> = $derived({
    New: i18n.t('account.statusNew'),
    PendingPayment: i18n.t('account.statusPendingPayment'),
    Paid: i18n.t('account.statusPaid'),
    Processing: i18n.t('account.statusProcessing'),
    Shipped: i18n.t('account.statusShipped'),
    Delivered: i18n.t('account.statusDelivered'),
    Completed: i18n.t('account.statusCompleted'),
    Canceled: i18n.t('account.statusCancelled'),
    Refunded: i18n.t('account.statusRefunded'),
  });

  const statusColors: Record<OrderStatus, string> = {
    New: 'var(--color-info)',
    PendingPayment: 'var(--color-warning)',
    Paid: 'var(--color-success)',
    Processing: 'var(--color-secondary)',
    Shipped: 'var(--color-primary)',
    Delivered: 'var(--color-success)',
    Completed: 'var(--color-success)',
    Canceled: 'var(--color-error)',
    Refunded: 'var(--color-text-muted)',
  };

  let timelineSteps: { status: OrderStatus; label: string }[] = $derived([
    { status: 'New', label: i18n.t('account.statusNew') },
    { status: 'Paid', label: i18n.t('account.statusPaid') },
    { status: 'Processing', label: i18n.t('account.statusProcessing') },
    { status: 'Shipped', label: i18n.t('account.statusShipped') },
    { status: 'Delivered', label: i18n.t('account.statusDelivered') },
  ]);

  const statusOrder: OrderStatus[] = [
    'New', 'PendingPayment', 'Paid', 'Processing', 'Shipped', 'Delivered', 'Completed',
  ];

  let currentStepIndex = $derived.by(() => {
    if (!order) return -1;
    if (order.status === 'Canceled' || order.status === 'Refunded') return -1;
    return statusOrder.indexOf(order.status);
  });

  function isStepCompleted(step: { status: OrderStatus }): boolean {
    const stepIdx = statusOrder.indexOf(step.status);
    return currentStepIndex >= stepIdx && currentStepIndex >= 0;
  }

  function isStepActive(step: { status: OrderStatus }): boolean {
    const stepIdx = statusOrder.indexOf(step.status);
    return currentStepIndex === stepIdx;
  }

  let canCancel = $derived.by(() => {
    if (!order) return false;
    return ['New', 'PendingPayment'].includes(order.status);
  });

  $effect(() => {
    loadOrder();
  });

  async function loadOrder() {
    isLoading = true;
    try {
      order = await orderApi.getOrderById(orderId);
    } catch {
      order = null;
      notificationStore.error(i18n.t('account.loadOrderError'));
    } finally {
      isLoading = false;
    }
  }

  async function handleCancel() {
    if (!order || !canCancel) return;
    isCanceling = true;
    try {
      order = await orderApi.cancelOrder(order.id);
      notificationStore.success(i18n.t('account.orderCanceled'));
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : i18n.t('account.cancelOrderError');
      notificationStore.error(message);
    } finally {
      isCanceling = false;
    }
  }

  function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('uk-UA', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  function formatPrice(price: number): string {
    return price.toLocaleString('uk-UA', { style: 'currency', currency: 'UAH' });
  }
</script>

<svelte:head>
  <title>{i18n.t('account.orderNumber', { id: orderId?.slice(0, 8).toUpperCase() ?? '' })} — MarineLaceSpace</title>
</svelte:head>

<div class="order-detail-page">
  {#if isLoading}
    <LoadingSpinner message={i18n.t('account.loadingOrder')} />
  {:else if !order}
    <div class="error-state">
      <p>{i18n.t('account.orderNotFound')}</p>
      <a href="/account/orders" class="btn btn-outline">{i18n.t('account.backToOrders')}</a>
    </div>
  {:else}
    <!-- Status timeline -->
    {#if order.status !== 'Canceled' && order.status !== 'Refunded'}
      <div class="timeline" role="progressbar" aria-label={i18n.t('account.orderStatus')}>
        {#each timelineSteps as step, i}
          <div
            class="timeline-step"
            class:completed={isStepCompleted(step)}
            class:active={isStepActive(step)}
          >
            <div class="timeline-dot">
              {#if isStepCompleted(step) && !isStepActive(step)}
                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="3" aria-hidden="true">
                  <polyline points="20 6 9 17 4 12"/>
                </svg>
              {:else}
                <span class="step-number">{i + 1}</span>
              {/if}
            </div>
            {#if i < timelineSteps.length - 1}
              <div class="timeline-line" class:line-completed={isStepCompleted(timelineSteps[i + 1])}></div>
            {/if}
            <span class="timeline-label">{step.label}</span>
          </div>
        {/each}
      </div>
    {:else}
      <div class="status-banner" style:background-color="{statusColors[order.status]}10" style:border-color="{statusColors[order.status]}30">
        <span class="status-banner-text" style:color={statusColors[order.status]}>
          {statusLabels[order.status]}
        </span>
      </div>
    {/if}

    <!-- Order info -->
    <div class="order-info-grid">
      <div class="info-card card">
        <h3 class="info-card-title">{i18n.t('account.orderInfo')}</h3>
        <div class="info-rows">
          <div class="info-row">
            <span class="info-label">{i18n.t('account.orderDate')}</span>
            <span class="info-value">{formatDate(order.createdAt)}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{i18n.t('account.orderStatus')}</span>
            <span
              class="status-badge"
              style:background-color="{statusColors[order.status]}15"
              style:color={statusColors[order.status]}
            >
              {statusLabels[order.status]}
            </span>
          </div>
          {#if order.trackingNumber}
            <div class="info-row">
              <span class="info-label">{i18n.t('account.trackingNumber')}</span>
              <span class="info-value tracking-number">{order.trackingNumber}</span>
            </div>
          {/if}
        </div>
      </div>

      <div class="info-card card">
        <h3 class="info-card-title">{i18n.t('account.shippingAddress')}</h3>
        <div class="address-block">
          <p>{order.shippingAddress.fullName}</p>
          <p>{order.shippingAddress.addressLine1}</p>
          {#if order.shippingAddress.addressLine2}
            <p>{order.shippingAddress.addressLine2}</p>
          {/if}
          <p>{order.shippingAddress.city}, {order.shippingAddress.state} {order.shippingAddress.postalCode}</p>
          <p>{order.shippingAddress.country}</p>
          <p class="address-phone">{order.shippingAddress.phone}</p>
        </div>
      </div>
    </div>

    <!-- Order items -->
    <div class="items-section card">
      <h3 class="section-title">{i18n.t('account.orderItems')}</h3>
      <div class="items-table" role="table" aria-label={i18n.t('account.orderItems')}>
        <div class="table-header" role="row">
          <span class="col-product" role="columnheader">{i18n.t('account.product')}</span>
          <span class="col-qty" role="columnheader">{i18n.t('account.quantity')}</span>
          <span class="col-price" role="columnheader">{i18n.t('account.price')}</span>
        </div>
        {#each order.items as item}
          <div class="table-row" role="row">
            <div class="col-product" role="cell">
              <div class="item-image-wrapper">
                {#if item.imageUrl}
                  <img src={item.imageUrl} alt={item.productName} class="item-img" loading="lazy" />
                {:else}
                  <div class="item-img-placeholder" aria-hidden="true">
                    <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.5">
                      <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                      <circle cx="8.5" cy="8.5" r="1.5"/>
                      <polyline points="21 15 16 10 5 21"/>
                    </svg>
                  </div>
                {/if}
              </div>
              <div class="item-details">
                <span class="item-name">{item.productName}</span>
                <div class="item-variants">
                  {#if item.sizeName}
                    <span>{i18n.t('product.size')}: {item.sizeName}</span>
                  {/if}
                  {#if item.colorName}
                    <span>{i18n.t('product.color')}: {item.colorName}</span>
                  {/if}
                  {#if item.materialName}
                    <span>{i18n.t('product.material')}: {item.materialName}</span>
                  {/if}
                </div>
                {#if item.personalization}
                  <p class="item-personalization">{i18n.t('product.personalization')}: {item.personalization}</p>
                {/if}
              </div>
            </div>
            <span class="col-qty" role="cell">{item.quantity}</span>
            <span class="col-price" role="cell">{formatPrice(item.unitPrice * item.quantity)}</span>
          </div>
        {/each}
      </div>
    </div>

    <!-- Order summary -->
    <div class="summary-section card">
      <div class="summary-rows">
        <div class="summary-row">
          <span>{i18n.t('account.subtotal')}</span>
          <span>{formatPrice(order.totalPrice)}</span>
        </div>
        <div class="summary-row">
          <span>{i18n.t('account.shipping')}</span>
          <span class="text-muted">{i18n.t('account.free')}</span>
        </div>
        <div class="summary-row summary-total">
          <span>{i18n.t('account.orderTotal')}</span>
          <span>{formatPrice(order.totalPrice)}</span>
        </div>
      </div>
    </div>

    <!-- Actions -->
    {#if canCancel}
      <div class="order-actions">
        <button
          class="btn btn-outline cancel-btn"
          onclick={handleCancel}
          disabled={isCanceling}
        >
          {#if isCanceling}
            <span class="spinner" aria-hidden="true"></span>
            {i18n.t('account.canceling')}
          {:else}
            {i18n.t('account.cancelOrder')}
          {/if}
        </button>
      </div>
    {/if}
  {/if}
</div>

<style>
  .order-detail-page {
    padding-bottom: var(--space-8);
  }

  .error-state {
    text-align: center;
    padding: var(--space-16) 0;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-4);
    color: var(--color-text-light);
  }

  /* Timeline */
  .timeline {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    margin-bottom: var(--space-8);
    padding: var(--space-6) var(--space-4);
    background: var(--color-surface);
    border-radius: var(--radius-lg);
    border: 1px solid var(--color-border);
    overflow-x: auto;
  }

  .timeline-step {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-2);
    position: relative;
    flex: 1;
    min-width: 80px;
  }

  .timeline-dot {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    border: 2px solid var(--color-border);
    background: var(--color-surface);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1;
    transition: all var(--transition-fast);
  }

  .timeline-step.completed .timeline-dot {
    border-color: var(--color-success);
    background: var(--color-success);
    color: #fff;
  }

  .timeline-step.active .timeline-dot {
    border-color: var(--color-primary);
    background: var(--color-primary);
    color: #fff;
    box-shadow: 0 0 0 4px rgba(139, 94, 107, 0.15);
  }

  .step-number {
    font-size: 0.75rem;
    font-weight: 600;
    color: var(--color-text-muted);
  }

  .timeline-step.completed .step-number,
  .timeline-step.active .step-number {
    color: #fff;
  }

  .timeline-line {
    position: absolute;
    top: 16px;
    left: calc(50% + 18px);
    right: calc(-50% + 18px);
    height: 2px;
    background: var(--color-border);
    z-index: 0;
  }

  .timeline-line.line-completed {
    background: var(--color-success);
  }

  .timeline-label {
    font-size: 0.75rem;
    color: var(--color-text-muted);
    text-align: center;
    white-space: nowrap;
  }

  .timeline-step.completed .timeline-label {
    color: var(--color-success);
    font-weight: 500;
  }

  .timeline-step.active .timeline-label {
    color: var(--color-primary);
    font-weight: 600;
  }

  .status-banner {
    padding: var(--space-4) var(--space-6);
    border-radius: var(--radius-md);
    border: 1px solid;
    margin-bottom: var(--space-8);
    text-align: center;
  }

  .status-banner-text {
    font-weight: 600;
    font-size: 0.9375rem;
  }

  /* Info grid */
  .order-info-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-4);
    margin-bottom: var(--space-6);
  }

  .info-card {
    padding: var(--space-5) var(--space-6);
  }

  .info-card-title {
    font-family: var(--font-display);
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-4);
  }

  .info-rows {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: var(--space-3);
  }

  .info-label {
    font-size: 0.875rem;
    color: var(--color-text-light);
  }

  .info-value {
    font-size: 0.875rem;
    color: var(--color-text);
    font-weight: 500;
  }

  .tracking-number {
    font-family: monospace;
    letter-spacing: 0.05em;
  }

  .status-badge {
    display: inline-flex;
    padding: 2px 10px;
    font-size: 0.75rem;
    font-weight: 600;
    border-radius: var(--radius-full);
  }

  .address-block {
    font-size: 0.875rem;
    color: var(--color-text);
    line-height: 1.6;
  }

  .address-phone {
    margin-top: var(--space-2);
    color: var(--color-text-light);
  }

  /* Items table */
  .items-section {
    padding: var(--space-6);
    margin-bottom: var(--space-6);
  }

  .section-title {
    font-family: var(--font-display);
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-4);
  }

  .table-header {
    display: grid;
    grid-template-columns: 1fr auto auto;
    gap: var(--space-4);
    padding: var(--space-3) 0;
    border-bottom: 1px solid var(--color-border);
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text-muted);
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .table-row {
    display: grid;
    grid-template-columns: 1fr auto auto;
    gap: var(--space-4);
    padding: var(--space-4) 0;
    border-bottom: 1px solid var(--color-border-light);
    align-items: center;
  }

  .table-row:last-child {
    border-bottom: none;
  }

  .col-product {
    display: flex;
    gap: var(--space-3);
    align-items: flex-start;
    min-width: 0;
  }

  .col-qty {
    text-align: center;
    min-width: 50px;
  }

  .col-price {
    text-align: right;
    min-width: 80px;
    font-weight: 500;
  }

  .item-image-wrapper {
    width: 64px;
    height: 64px;
    border-radius: var(--radius-md);
    overflow: hidden;
    flex-shrink: 0;
    background: var(--color-surface-hover);
  }

  .item-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .item-img-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--color-text-muted);
  }

  .item-details {
    min-width: 0;
  }

  .item-name {
    font-size: 0.9375rem;
    font-weight: 500;
    color: var(--color-text);
    display: block;
    margin-bottom: var(--space-1);
  }

  .item-variants {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  .item-variants span:not(:last-child)::after {
    content: '·';
    margin-left: var(--space-2);
  }

  .item-personalization {
    font-size: 0.8125rem;
    color: var(--color-text-light);
    font-style: italic;
    margin-top: var(--space-1);
  }

  /* Summary */
  .summary-section {
    padding: var(--space-5) var(--space-6);
    margin-bottom: var(--space-6);
    max-width: 360px;
    margin-left: auto;
  }

  .summary-rows {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .summary-row {
    display: flex;
    justify-content: space-between;
    font-size: 0.9375rem;
    color: var(--color-text);
  }

  .summary-total {
    padding-top: var(--space-3);
    border-top: 1px solid var(--color-border);
    font-weight: 600;
    font-size: 1.0625rem;
  }

  /* Actions */
  .order-actions {
    display: flex;
    justify-content: flex-end;
  }

  .cancel-btn {
    color: var(--color-error);
    border-color: var(--color-error);
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
  }

  .cancel-btn:hover {
    background: rgba(196, 85, 90, 0.06);
  }

  .spinner {
    display: inline-block;
    width: 16px;
    height: 16px;
    border: 2px solid rgba(196, 85, 90, 0.3);
    border-top-color: var(--color-error);
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }

  @media (max-width: 768px) {
    .order-info-grid {
      grid-template-columns: 1fr;
    }

    .summary-section {
      max-width: none;
    }

    .timeline {
      padding: var(--space-4) var(--space-2);
    }

    .table-header {
      display: none;
    }

    .table-row {
      grid-template-columns: 1fr;
      gap: var(--space-2);
    }

    .col-qty,
    .col-price {
      text-align: left;
    }

    .items-section {
      padding: var(--space-4);
    }

    .col-product {
      flex-direction: column;
    }

    .item-image-wrapper {
      width: 48px;
      height: 48px;
    }
  }
</style>
