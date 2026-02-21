<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
  import type { Order, OrderStatus } from '$types';
  import * as orderApi from '$lib/api/order';
  import EmptyState from '$lib/components/EmptyState.svelte';
  import Pagination from '$lib/components/Pagination.svelte';
  import LoadingSpinner from '$lib/components/LoadingSpinner.svelte';

  let orders = $state<Order[]>([]);
  let isLoading = $state(true);
  let currentPage = $state(1);
  let totalPages = $state(1);
  let activeFilter = $state<string>('all');
  const pageSize = 10;

  interface FilterTab {
    key: string;
    label: string;
    status?: string;
  }

  let filterTabs: FilterTab[] = $derived([
    { key: 'all', label: i18n.t('account.filterAll') },
    { key: 'new', label: i18n.t('account.filterNew'), status: 'New' },
    { key: 'processing', label: i18n.t('account.statusProcessing'), status: 'Processing' },
    { key: 'shipped', label: i18n.t('account.statusShipped'), status: 'Shipped' },
    { key: 'delivered', label: i18n.t('account.statusDelivered'), status: 'Delivered' },
    { key: 'canceled', label: i18n.t('account.statusCancelled'), status: 'Canceled' },
  ]);

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

  async function loadOrders() {
    isLoading = true;
    try {
      const tab = filterTabs.find((t) => t.key === activeFilter);
      const params: { page: number; pageSize: number; status?: string } = {
        page: currentPage,
        pageSize,
      };
      if (tab?.status) {
        params.status = tab.status;
      }
      const response = await orderApi.getOrders(params);
      orders = response.items;
      totalPages = response.totalPages;
    } catch {
      orders = [];
    } finally {
      isLoading = false;
    }
  }

  $effect(() => {
    loadOrders();
  });

  function handleFilterChange(key: string) {
    activeFilter = key;
    currentPage = 1;
  }

  function handlePageChange(page: number) {
    currentPage = page;
  }

  function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('uk-UA', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    });
  }

  function formatPrice(price: number): string {
    return price.toLocaleString('uk-UA', { style: 'currency', currency: 'UAH' });
  }
</script>

<svelte:head>
  <title>{i18n.t('account.orderHistory')} — MarineLaceSpace</title>
</svelte:head>

<div class="orders-page">
  <h1 class="page-heading">{i18n.t('account.orderHistory')}</h1>

  <div class="filter-tabs" role="tablist" aria-label={i18n.t('account.filterOrders')}>
    {#each filterTabs as tab}
      <button
        class="filter-tab"
        class:active={activeFilter === tab.key}
        role="tab"
        aria-selected={activeFilter === tab.key}
        onclick={() => handleFilterChange(tab.key)}
      >
        {tab.label}
      </button>
    {/each}
  </div>

  {#if isLoading}
    <LoadingSpinner message={i18n.t('account.loadingOrders')} />
  {:else if orders.length === 0}
    <EmptyState
      title={i18n.t('account.noOrders')}
      description={i18n.t('account.noOrdersDescription')}
      icon="📦"
      actionLabel={i18n.t('account.goToCatalog')}
      actionHref="/catalog"
    />
  {:else}
    <div class="orders-list">
      {#each orders as order (order.id)}
        <article class="order-card card">
          <div class="order-header">
            <div class="order-meta">
              <span class="order-number">{i18n.t('account.orderNumber', { id: order.id.slice(0, 8).toUpperCase() })}</span>
              <span class="order-date">{formatDate(order.createdAt)}</span>
            </div>
            <span
              class="status-badge"
              style:background-color="{statusColors[order.status]}15"
              style:color={statusColors[order.status]}
            >
              {statusLabels[order.status]}
            </span>
          </div>

          <div class="order-items-preview">
            {#each order.items.slice(0, 3) as item}
              <div class="item-thumb">
                {#if item.imageUrl}
                  <img src={item.imageUrl} alt={item.productName} class="item-image" loading="lazy" />
                {:else}
                  <div class="item-placeholder" aria-hidden="true">
                    <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.5">
                      <rect x="3" y="3" width="18" height="18" rx="2" ry="2"/>
                      <circle cx="8.5" cy="8.5" r="1.5"/>
                      <polyline points="21 15 16 10 5 21"/>
                    </svg>
                  </div>
                {/if}
              </div>
            {/each}
            {#if order.items.length > 3}
              <div class="item-more">+{order.items.length - 3}</div>
            {/if}
          </div>

          <div class="order-footer">
            <span class="order-total">{formatPrice(order.totalPrice)}</span>
            <a href="/account/orders/{order.id}" class="btn btn-sm btn-outline">
              {i18n.t('account.viewOrder')}
            </a>
          </div>
        </article>
      {/each}
    </div>

    {#if totalPages > 1}
      <div class="pagination-wrapper">
        <Pagination {currentPage} {totalPages} onPageChange={handlePageChange} />
      </div>
    {/if}
  {/if}
</div>

<style>
  .page-heading {
    font-family: var(--font-display);
    font-size: 1.5rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-6);
  }

  .filter-tabs {
    display: flex;
    gap: var(--space-2);
    overflow-x: auto;
    padding-bottom: var(--space-4);
    margin-bottom: var(--space-6);
    border-bottom: 1px solid var(--color-border);
    -webkit-overflow-scrolling: touch;
  }

  .filter-tab {
    padding: var(--space-2) var(--space-4);
    font-size: 0.875rem;
    font-family: inherit;
    color: var(--color-text-light);
    background: none;
    border: 1px solid transparent;
    border-radius: var(--radius-full);
    cursor: pointer;
    transition: all var(--transition-fast);
    white-space: nowrap;
  }

  .filter-tab:hover {
    color: var(--color-text);
    background: var(--color-surface-hover);
  }

  .filter-tab.active {
    color: var(--color-primary);
    border-color: var(--color-primary);
    background: rgba(139, 94, 107, 0.06);
    font-weight: 500;
  }

  .orders-list {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .order-card {
    padding: var(--space-5) var(--space-6);
  }

  .order-card:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
  }

  .order-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: var(--space-4);
    margin-bottom: var(--space-4);
  }

  .order-meta {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .order-number {
    font-size: 0.9375rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .order-date {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  .status-badge {
    display: inline-flex;
    align-items: center;
    padding: 4px 12px;
    font-size: 0.75rem;
    font-weight: 600;
    border-radius: var(--radius-full);
    white-space: nowrap;
  }

  .order-items-preview {
    display: flex;
    gap: var(--space-2);
    margin-bottom: var(--space-4);
    overflow-x: auto;
  }

  .item-thumb {
    width: 56px;
    height: 56px;
    border-radius: var(--radius-md);
    overflow: hidden;
    flex-shrink: 0;
    background: var(--color-surface-hover);
  }

  .item-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .item-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--color-text-muted);
  }

  .item-more {
    width: 56px;
    height: 56px;
    border-radius: var(--radius-md);
    background: var(--color-surface-hover);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text-light);
    flex-shrink: 0;
  }

  .order-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding-top: var(--space-4);
    border-top: 1px solid var(--color-border-light);
  }

  .order-total {
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .pagination-wrapper {
    margin-top: var(--space-8);
  }

  @media (max-width: 640px) {
    .order-card {
      padding: var(--space-4);
    }

    .order-header {
      flex-direction: column;
      gap: var(--space-2);
    }
  }
</style>
