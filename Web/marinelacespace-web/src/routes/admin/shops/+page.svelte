<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import ReviewStars from '$components/ReviewStars.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Shop } from '$types';

  let loading = $state(true);
  let shops = $state<Shop[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');

  let showDeleteModal = $state(false);
  let deleteTarget = $state<Shop | null>(null);

  $effect(() => {
    loadShops(currentPage, search);
  });

  async function loadShops(page: number, q: string) {
    try {
      loading = true;
      const result = await catalogApi.getShops({
        page,
        pageSize: 20,
        search: q || undefined,
      });
      shops = result.items;
      totalPages = result.totalPages;
    } catch {
      notificationStore.error('Помилка завантаження магазинів');
    } finally {
      loading = false;
    }
  }

  function handlePageChange(page: number) {
    currentPage = page;
  }

  function confirmDelete(shop: Shop) {
    deleteTarget = shop;
    showDeleteModal = true;
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteShop(deleteTarget.id);
      notificationStore.success('Магазин видалено');
      showDeleteModal = false;
      deleteTarget = null;
      loadShops(currentPage, search);
    } catch {
      notificationStore.error('Помилка видалення магазину');
    }
  }

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA');
  }
</script>

<div class="shops-page">
  <div class="page-header">
    <h1 class="page-title">Магазини</h1>
  </div>

  <div class="toolbar">
    <input
      class="input input-sm search-input"
      type="search"
      placeholder="Пошук магазинів..."
      bind:value={search}
      on:input={() => { currentPage = 1; }}
    />
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження магазинів..." />
  {:else if shops.length === 0}
    <EmptyState title="Магазинів не знайдено" icon="🏪" />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Лого</th>
            <th>Назва</th>
            <th>Власник</th>
            <th>Товари</th>
            <th>Рейтинг</th>
            <th>Дата створення</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          {#each shops as shop}
            <tr>
              <td>
                {#if shop.logoUrl}
                  <img src={shop.logoUrl} alt={shop.name} class="shop-logo" />
                {:else}
                  <div class="shop-logo placeholder">🏪</div>
                {/if}
              </td>
              <td class="cell-title">
                <a href="/admin/shops/{shop.id}" class="shop-link">{shop.name}</a>
              </td>
              <td>{shop.ownerName}</td>
              <td class="text-center">{shop.productCount}</td>
              <td>
                <ReviewStars rating={shop.averageRating} count={shop.reviewCount} size="sm" />
              </td>
              <td>{formatDate(shop.createdAt)}</td>
              <td class="cell-actions">
                <a href="/admin/shops/{shop.id}" class="btn btn-sm btn-ghost">Деталі</a>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  on:click={() => confirmDelete(shop)}
                >
                  Видалити
                </button>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>

    <Pagination {currentPage} {totalPages} onPageChange={handlePageChange} />
  {/if}
</div>

<Modal open={showDeleteModal} title="Видалити магазин?" onclose={() => (showDeleteModal = false)}>
  <p>Ви впевнені, що хочете видалити магазин <strong>{deleteTarget?.name}</strong>?</p>
  <p class="text-sm text-muted mt-2">Усі товари цього магазину також будуть видалені.</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showDeleteModal = false)}>Скасувати</button>
    <button class="btn btn-danger" on:click={executeDelete}>Видалити</button>
  </div>
</Modal>

<style>
  .page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
  }

  .page-title {
    font-size: 1.75rem;
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

  .shop-logo {
    width: 36px;
    height: 36px;
    border-radius: var(--radius-sm);
    object-fit: cover;
  }

  .shop-logo.placeholder {
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--color-border-light);
    font-size: 1rem;
  }

  .cell-title {
    font-weight: 500;
  }

  .shop-link {
    color: var(--color-primary);
    text-decoration: none;
  }

  .shop-link:hover {
    text-decoration: underline;
  }

  .cell-actions {
    white-space: nowrap;
  }

  .text-center {
    text-align: center;
  }

  .btn-danger {
    background: var(--color-error);
    color: #fff;
    border: none;
  }

  .btn-danger-text {
    color: var(--color-error);
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }
</style>
