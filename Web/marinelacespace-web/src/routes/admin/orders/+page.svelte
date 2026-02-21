<script lang="ts">
  import * as orderApi from '$api/order';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Order, OrderStatus, Shop } from '$types';

  let loading = $state(true);
  let orders = $state<Order[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');
  let activeStatusId = $state<number | null>(null);
  let sellerShops = $state<Shop[]>([]);

  let statusTabs = $derived([
    { label: i18n.t('admin.all'), value: null as number | null },
    { label: i18n.t('admin.orderStatus.newPlural'), value: 1 },
    { label: i18n.t('admin.orderStatus.paidPlural'), value: 3 },
    { label: i18n.t('admin.orderStatus.processing'), value: 4 },
    { label: i18n.t('admin.orderStatus.shippedPlural'), value: 5 },
    { label: i18n.t('admin.orderStatus.deliveredPlural'), value: 6 },
    { label: i18n.t('admin.orderStatus.canceledPlural'), value: 8 },
  ]);

  let orderStats = $derived({
    total: orders.length,
    pending: orders.filter((o) => o.status === 'New' || o.status === 'PendingPayment').length,
    processing: orders.filter((o) => o.status === 'Processing' || o.status === 'Paid').length,
    shipped: orders.filter((o) => o.status === 'Shipped').length,
  });

  $effect(() => {
    loadOrders(currentPage, activeStatusId);
  });

  async function loadOrders(page: number, statusId: number | null) {
    try {
      loading = true;

      if (authStore.isAdmin) {
        const result = await orderApi.getAdminOrders({
          page,
          pageSize: 20,
          statusId: statusId ?? undefined,
          search: search || undefined,
        });
        orders = result.items;
        totalPages = result.totalPages;
      } else if (authStore.isSeller) {
        if (sellerShops.length === 0) {
          sellerShops = await catalogApi.getMyShops();
        }
        if (sellerShops.length === 0) {
          orders = [];
          totalPages = 1;
          return;
        }

        const shopResults = await Promise.all(
          sellerShops.map((shop) =>
            orderApi.getShopOrders(shop.id, {
              page,
              pageSize: 20,
              statusId: statusId ?? undefined,
            }),
          ),
        );

        const merged = shopResults.flatMap((r) => r.items);
        merged.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        orders = merged;
        totalPages = Math.max(...shopResults.map((r) => r.totalPages), 1);
      }
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingOrders'));
    } finally {
      loading = false;
    }
  }

  function setStatusFilter(statusId: number | null) {
    activeStatusId = statusId;
    currentPage = 1;
  }

  function handlePageChange(page: number) {
    currentPage = page;
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
      New: 'badge-info',
      PendingPayment: 'badge-info',
      Paid: 'badge-success',
      Processing: 'badge-warning',
      Shipped: 'badge-warning',
      Delivered: 'badge-secondary',
      Completed: 'badge-success',
      Canceled: 'badge-error',
      Refunded: 'badge-error',
    };
    return map[status] ?? 'badge-outline';
  }

  function statusLabel(status: string): string {
    const map: Record<string, string> = {
      New: i18n.t('admin.orderStatus.new'),
      PendingPayment: i18n.t('admin.orderStatus.pendingPayment'),
      Paid: i18n.t('admin.orderStatus.paid'),
      Processing: i18n.t('admin.orderStatus.processing'),
      Shipped: i18n.t('admin.orderStatus.shipped'),
      Delivered: i18n.t('admin.orderStatus.delivered'),
      Completed: i18n.t('admin.orderStatus.completed'),
      Canceled: i18n.t('admin.orderStatus.canceled'),
      Refunded: i18n.t('admin.orderStatus.refunded'),
    };
    return map[status] ?? status;
  }
</script>

<div class="orders-page">
  <div class="page-header">
    <h1 class="page-title">{i18n.t('admin.orders')}</h1>
    {#if !loading && orders.length > 0}
      <div class="order-stats">
        <div class="stat-chip">
          <span class="stat-count">{orderStats.total}</span>
          <span class="stat-label">{i18n.t('admin.all')}</span>
        </div>
        <div class="stat-chip stat-pending">
          <span class="stat-count">{orderStats.pending}</span>
          <span class="stat-label">Pending</span>
        </div>
        <div class="stat-chip stat-processing">
          <span class="stat-count">{orderStats.processing}</span>
          <span class="stat-label">{i18n.t('admin.orderStatus.processing')}</span>
        </div>
        <div class="stat-chip stat-shipped">
          <span class="stat-count">{orderStats.shipped}</span>
          <span class="stat-label">{i18n.t('admin.orderStatus.shippedPlural')}</span>
        </div>
      </div>
    {/if}
  </div>

  {#if authStore.isSeller && !authStore.isAdmin && sellerShops.length > 0}
    <div class="seller-context">
      <span class="seller-context-icon">🏪</span>
      <span>
        {sellerShops.length === 1
          ? `Orders for: ${sellerShops[0].name}`
          : `Orders across ${sellerShops.length} shops`}
      </span>
    </div>
  {/if}

  <div class="status-tabs">
    {#each statusTabs as tab}
      <button
        class="status-tab"
        class:active={activeStatusId === tab.value}
        onclick={() => setStatusFilter(tab.value)}
      >
        {tab.label}
      </button>
    {/each}
  </div>

  <div class="toolbar">
    <input
      class="input input-sm search-input"
      type="search"
      placeholder={i18n.t('admin.searchByNumberOrEmail')}
      bind:value={search}
      oninput={() => loadOrders(currentPage, activeStatusId)}
    />
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingOrders')} />
  {:else if orders.length === 0}
    <EmptyState
      title={i18n.t('admin.noOrdersFound')}
      description={i18n.t('admin.noOrdersWithFilter')}
      icon="📋"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>№</th>
            <th>{i18n.t('admin.date')}</th>
            <th>{i18n.t('admin.buyer')}</th>
            <th>Email</th>
            <th>{i18n.t('admin.items')}</th>
            <th>{i18n.t('admin.amount')}</th>
            <th>{i18n.t('admin.status')}</th>
            <th>{i18n.t('admin.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {#each orders as order}
            <tr class="order-row" onclick={() => { window.location.href = `/admin/orders/${order.id}`; }}>
              <td class="cell-mono">
                <a href="/admin/orders/{order.id}" class="order-link" onclick={(e: MouseEvent) => e.stopPropagation()}>
                  #{order.id.slice(0, 8)}
                </a>
              </td>
              <td class="cell-nowrap">{formatDate(order.createdAt)}</td>
              <td class="cell-buyer">{order.shippingAddress.fullName}</td>
              <td>
                <a href="mailto:{order.buyerEmail}" class="email-link" onclick={(e: MouseEvent) => e.stopPropagation()}>
                  {order.buyerEmail}
                </a>
              </td>
              <td class="text-center">
                <span class="items-count">{order.items.length}</span>
              </td>
              <td class="cell-mono cell-amount">{formatCurrency(order.totalPrice)}</td>
              <td>
                <span class="badge {statusBadge(order.status)}">
                  {statusLabel(order.status)}
                </span>
              </td>
              <td>
                <a href="/admin/orders/{order.id}" class="btn btn-sm btn-ghost detail-btn" onclick={(e: MouseEvent) => e.stopPropagation()}>
                  {i18n.t('admin.details')} →
                </a>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>

    <Pagination {currentPage} {totalPages} onPageChange={handlePageChange} />
  {/if}
</div>

<style>
  .page-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: var(--space-3);
    margin-bottom: var(--space-4);
  }

  .page-title {
    font-size: 1.75rem;
    margin: 0;
  }

  .order-stats {
    display: flex;
    gap: var(--space-2);
    flex-wrap: wrap;
  }

  .stat-chip {
    display: flex;
    align-items: center;
    gap: var(--space-1);
    padding: var(--space-1) var(--space-3);
    border-radius: var(--radius-full);
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    font-size: 0.75rem;
  }

  .stat-count {
    font-weight: 700;
    font-size: 0.8125rem;
  }

  .stat-label {
    color: var(--color-text-light);
  }

  .stat-pending {
    border-color: var(--color-info);
    background: color-mix(in srgb, var(--color-info) 8%, transparent);
  }

  .stat-processing {
    border-color: var(--color-warning);
    background: color-mix(in srgb, var(--color-warning) 8%, transparent);
  }

  .stat-shipped {
    border-color: var(--color-primary);
    background: color-mix(in srgb, var(--color-primary) 8%, transparent);
  }

  .seller-context {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-3) var(--space-4);
    background: color-mix(in srgb, var(--color-primary) 6%, transparent);
    border: 1px solid color-mix(in srgb, var(--color-primary) 20%, transparent);
    border-radius: var(--radius-md);
    font-size: 0.875rem;
    color: var(--color-text);
    margin-bottom: var(--space-4);
  }

  .seller-context-icon {
    font-size: 1.125rem;
  }

  .status-tabs {
    display: flex;
    gap: var(--space-1);
    border-bottom: 2px solid var(--color-border);
    margin-bottom: var(--space-4);
    overflow-x: auto;
  }

  .status-tab {
    padding: var(--space-3) var(--space-4);
    font-size: 0.8125rem;
    font-weight: 500;
    border: none;
    background: none;
    cursor: pointer;
    color: var(--color-text-light);
    border-bottom: 2px solid transparent;
    margin-bottom: -2px;
    transition: all var(--transition-fast);
    white-space: nowrap;
  }

  .status-tab:hover {
    color: var(--color-text);
    background: var(--color-surface-hover);
  }

  .status-tab.active {
    color: var(--color-primary);
    border-bottom-color: var(--color-primary);
  }

  .toolbar {
    margin-bottom: var(--space-4);
  }

  .search-input {
    max-width: 400px;
  }

  .table-wrapper {
    overflow-x: auto;
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    margin-bottom: var(--space-4);
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

  .order-row {
    cursor: pointer;
    transition: background var(--transition-fast);
  }

  .order-row:hover {
    background: var(--color-surface-hover);
  }

  .order-link {
    color: var(--color-primary);
    text-decoration: none;
    font-weight: 600;
  }

  .order-link:hover {
    text-decoration: underline;
  }

  .email-link {
    color: var(--color-text-light);
    text-decoration: none;
    font-size: 0.8125rem;
  }

  .email-link:hover {
    color: var(--color-primary);
    text-decoration: underline;
  }

  .cell-mono {
    font-family: monospace;
    font-size: 0.8125rem;
  }

  .cell-nowrap {
    white-space: nowrap;
  }

  .cell-buyer {
    font-weight: 500;
  }

  .cell-amount {
    font-weight: 600;
  }

  .items-count {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 1.5rem;
    height: 1.5rem;
    padding: 0 var(--space-1);
    border-radius: var(--radius-full);
    background: var(--color-border-light);
    font-size: 0.75rem;
    font-weight: 600;
  }

  .text-center {
    text-align: center;
  }

  .detail-btn {
    font-weight: 500;
    white-space: nowrap;
  }
</style>
