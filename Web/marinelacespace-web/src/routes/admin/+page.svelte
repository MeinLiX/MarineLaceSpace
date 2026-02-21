<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import * as orderApi from '$api/order';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import type { Order } from '$types';

  let loading = $state(true);
  let recentOrders = $state<Order[]>([]);
  let stats = $state({
    ordersToday: 0,
    monthlyRevenue: 0,
    activeProducts: 0,
    newReviews: 0,
  });

  $effect(() => {
    loadDashboard();
  });

  async function loadDashboard() {
    try {
      loading = true;
      const [ordersRes, productsRes] = await Promise.all([
        orderApi.getOrders({ page: 1, pageSize: 10 }),
        catalogApi.getProducts({ page: 1, pageSize: 1 }),
      ]);

      recentOrders = ordersRes.items;
      stats.activeProducts = productsRes.totalCount;

      const today = new Date().toDateString();
      stats.ordersToday = ordersRes.items.filter(
        (o) => new Date(o.createdAt).toDateString() === today
      ).length;

      const now = new Date();
      stats.monthlyRevenue = ordersRes.items
        .filter((o) => {
          const d = new Date(o.createdAt);
          return d.getMonth() === now.getMonth() && d.getFullYear() === now.getFullYear();
        })
        .reduce((sum, o) => sum + o.totalPrice, 0);
    } catch {
      /* keep default values */
    } finally {
      loading = false;
    }
  }

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA');
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

<div class="dashboard">
  <h1 class="page-title">Панель керування</h1>

  {#if loading}
    <LoadingSpinner message="Завантаження..." />
  {:else}
    <div class="stats-row">
      <div class="stat-card card">
        <span class="stat-icon">📋</span>
        <div class="stat-body">
          <span class="stat-label">Замовлення сьогодні</span>
          <span class="stat-value">{stats.ordersToday}</span>
          <span class="stat-change positive">↑ сьогодні</span>
        </div>
      </div>
      <div class="stat-card card">
        <span class="stat-icon">💰</span>
        <div class="stat-body">
          <span class="stat-label">Дохід за місяць</span>
          <span class="stat-value">{formatCurrency(stats.monthlyRevenue)}</span>
          <span class="stat-change positive">₴</span>
        </div>
      </div>
      <div class="stat-card card">
        <span class="stat-icon">📦</span>
        <div class="stat-body">
          <span class="stat-label">Активні товари</span>
          <span class="stat-value">{stats.activeProducts}</span>
          <span class="stat-change neutral">загалом</span>
        </div>
      </div>
      <div class="stat-card card">
        <span class="stat-icon">⭐</span>
        <div class="stat-body">
          <span class="stat-label">Нові відгуки</span>
          <span class="stat-value">{stats.newReviews}</span>
          <span class="stat-change neutral">за тиждень</span>
        </div>
      </div>
    </div>

    <div class="dashboard-grid">
      <section class="recent-orders card">
        <div class="card-header">
          <h2>Останні замовлення</h2>
        </div>
        <div class="card-body" style="padding: 0;">
          <div class="table-wrapper">
            <table class="data-table">
              <thead>
                <tr>
                  <th>№</th>
                  <th>Дата</th>
                  <th>Покупець</th>
                  <th>Сума</th>
                  <th>Статус</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {#each recentOrders as order}
                  <tr>
                    <td class="cell-mono">#{order.id.slice(0, 8)}</td>
                    <td>{formatDate(order.createdAt)}</td>
                    <td>{order.buyerEmail}</td>
                    <td class="cell-mono">{formatCurrency(order.totalPrice)}</td>
                    <td>
                      <span class="badge {statusBadge(order.status)}">
                        {statusLabel(order.status)}
                      </span>
                    </td>
                    <td>
                      <a href="/admin/orders/{order.id}" class="btn btn-sm btn-ghost">
                        Переглянути
                      </a>
                    </td>
                  </tr>
                {:else}
                  <tr>
                    <td colspan="6" class="empty-cell">Замовлень поки немає</td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <section class="quick-actions card">
        <div class="card-header">
          <h2>Швидкі дії</h2>
        </div>
        <div class="card-body">
          <div class="actions-list">
            <a href="/admin/products/new" class="action-card">
              <span class="action-icon">➕</span>
              <span class="action-label">Додати товар</span>
            </a>
            <a href="/admin/orders" class="action-card">
              <span class="action-icon">📋</span>
              <span class="action-label">Переглянути замовлення</span>
            </a>
            <a href="/admin/categories" class="action-card">
              <span class="action-icon">📂</span>
              <span class="action-label">Управління каталогом</span>
            </a>
          </div>
        </div>
      </section>
    </div>
  {/if}
</div>

<style>
  .page-title {
    font-size: 1.75rem;
    margin-bottom: var(--space-6);
  }

  .stats-row {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: var(--space-4);
    margin-bottom: var(--space-6);
  }

  .stat-card {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    padding: var(--space-5);
  }

  .stat-icon {
    font-size: 2rem;
    line-height: 1;
  }

  .stat-body {
    display: flex;
    flex-direction: column;
  }

  .stat-label {
    font-size: 0.75rem;
    color: var(--color-text-light);
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .stat-value {
    font-size: 1.5rem;
    font-weight: 700;
    font-family: var(--font-display);
    color: var(--color-text);
  }

  .stat-change {
    font-size: 0.6875rem;
    margin-top: 2px;
  }

  .stat-change.positive {
    color: var(--color-success);
  }

  .stat-change.neutral {
    color: var(--color-text-muted);
  }

  .dashboard-grid {
    display: grid;
    grid-template-columns: 1fr 300px;
    gap: var(--space-6);
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

  .empty-cell {
    text-align: center;
    padding: var(--space-8) !important;
    color: var(--color-text-muted);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .actions-list {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .action-card {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    border-radius: var(--radius-md);
    transition: background var(--transition-fast);
    text-decoration: none;
    color: var(--color-text);
  }

  .action-card:hover {
    background: var(--color-surface-hover);
  }

  .action-icon {
    font-size: 1.25rem;
  }

  .action-label {
    font-size: 0.875rem;
    font-weight: 500;
  }

  @media (max-width: 1024px) {
    .stats-row {
      grid-template-columns: repeat(2, 1fr);
    }
    .dashboard-grid {
      grid-template-columns: 1fr;
    }
  }

  @media (max-width: 640px) {
    .stats-row {
      grid-template-columns: 1fr;
    }
  }
</style>
