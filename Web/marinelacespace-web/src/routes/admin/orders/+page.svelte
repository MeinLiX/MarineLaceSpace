<script lang="ts">
  import * as orderApi from '$api/order';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Order, OrderStatus } from '$types';

  let loading = $state(true);
  let orders = $state<Order[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');
  let activeStatusId = $state<number | null>(null);

  let statusTabs = $derived([
    { label: i18n.t('admin.all'), value: null as number | null },
    { label: i18n.t('admin.orderStatus.newPlural'), value: 1 },
    { label: i18n.t('admin.orderStatus.paidPlural'), value: 3 },
    { label: i18n.t('admin.orderStatus.processing'), value: 4 },
    { label: i18n.t('admin.orderStatus.shippedPlural'), value: 5 },
    { label: i18n.t('admin.orderStatus.deliveredPlural'), value: 6 },
    { label: i18n.t('admin.orderStatus.canceledPlural'), value: 8 },
  ]);

  $effect(() => {
    loadOrders(currentPage, activeStatusId);
  });

  async function loadOrders(page: number, statusId: number | null) {
    try {
      loading = true;
      const result = await orderApi.getAdminOrders({
        page,
        pageSize: 20,
        statusId: statusId ?? undefined,
        search: search || undefined,
      });
      orders = result.items;
      totalPages = result.totalPages;
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
  <h1 class="page-title">{i18n.t('admin.orders')}</h1>

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
            <tr>
              <td class="cell-mono">#{order.id.slice(0, 8)}</td>
              <td class="cell-nowrap">{formatDate(order.createdAt)}</td>
              <td>{order.shippingAddress.fullName}</td>
              <td>{order.buyerEmail}</td>
              <td class="text-center">{order.items.length}</td>
              <td class="cell-mono">{formatCurrency(order.totalPrice)}</td>
              <td>
                <span class="badge {statusBadge(order.status)}">
                  {statusLabel(order.status)}
                </span>
              </td>
              <td>
                <a href="/admin/orders/{order.id}" class="btn btn-sm btn-ghost">
                  {i18n.t('admin.details')}
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
  .page-title {
    font-size: 1.75rem;
    margin-bottom: var(--space-4);
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

  .data-table tbody tr:hover {
    background: var(--color-surface-hover);
  }

  .cell-mono {
    font-family: monospace;
    font-size: 0.8125rem;
  }

  .cell-nowrap {
    white-space: nowrap;
  }

  .text-center {
    text-align: center;
  }
</style>
