<script lang="ts">
  import * as orderApi from '$api/order';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Order, OrderStatus } from '$types';

  let loading = $state(true);
  let orders = $state<Order[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');
  let activeStatus = $state<string>('');

  const statusTabs: { label: string; value: string }[] = [
    { label: 'Всі', value: '' },
    { label: 'Нові', value: 'New' },
    { label: 'Оплачені', value: 'Paid' },
    { label: 'В обробці', value: 'Processing' },
    { label: 'Відправлені', value: 'Shipped' },
    { label: 'Доставлені', value: 'Delivered' },
    { label: 'Скасовані', value: 'Canceled' },
  ];

  $effect(() => {
    loadOrders(currentPage, activeStatus);
  });

  async function loadOrders(page: number, status: string) {
    try {
      loading = true;
      const result = await orderApi.getOrders({
        page,
        pageSize: 20,
        status: status || undefined,
      });
      orders = search
        ? result.items.filter(
            (o) =>
              o.id.toLowerCase().includes(search.toLowerCase()) ||
              o.buyerEmail.toLowerCase().includes(search.toLowerCase())
          )
        : result.items;
      totalPages = result.totalPages;
    } catch {
      notificationStore.error('Помилка завантаження замовлень');
    } finally {
      loading = false;
    }
  }

  function setStatusFilter(status: string) {
    activeStatus = status;
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
      New: 'Нове',
      PendingPayment: 'Очікує оплати',
      Paid: 'Оплачено',
      Processing: 'В обробці',
      Shipped: 'Відправлено',
      Delivered: 'Доставлено',
      Completed: 'Завершено',
      Canceled: 'Скасовано',
      Refunded: 'Повернення',
    };
    return map[status] ?? status;
  }
</script>

<div class="orders-page">
  <h1 class="page-title">Замовлення</h1>

  <div class="status-tabs">
    {#each statusTabs as tab}
      <button
        class="status-tab"
        class:active={activeStatus === tab.value}
        on:click={() => setStatusFilter(tab.value)}
      >
        {tab.label}
      </button>
    {/each}
  </div>

  <div class="toolbar">
    <input
      class="input input-sm search-input"
      type="search"
      placeholder="Пошук за номером або email..."
      bind:value={search}
      on:input={() => loadOrders(currentPage, activeStatus)}
    />
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження замовлень..." />
  {:else if orders.length === 0}
    <EmptyState
      title="Замовлень не знайдено"
      description="Немає замовлень з обраним фільтром"
      icon="📋"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>№</th>
            <th>Дата</th>
            <th>Покупець</th>
            <th>Email</th>
            <th>Товари</th>
            <th>Сума</th>
            <th>Статус</th>
            <th>Дії</th>
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
                  Деталі
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
