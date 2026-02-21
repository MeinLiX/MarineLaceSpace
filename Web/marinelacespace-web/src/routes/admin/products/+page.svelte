<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Product, ProductStatus } from '$types';

  let loading = $state(true);
  let products = $state<Product[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');
  let statusFilter = $state('');
  let selectedIds = $state<Set<string>>(new Set());
  let deleteTarget = $state<Product | null>(null);
  let showDeleteModal = $state(false);
  let showBulkDeleteModal = $state(false);

  $effect(() => {
    loadProducts(currentPage, search, statusFilter);
  });

  async function loadProducts(page: number, q: string, status: string) {
    try {
      loading = true;
      const result = await catalogApi.getProducts({
        page,
        pageSize: 20,
        search: q || undefined,
      });
      products = status
        ? result.items.filter((p) => p.status === status)
        : result.items;
      totalPages = result.totalPages;
    } catch {
      notificationStore.error('Помилка завантаження товарів');
    } finally {
      loading = false;
    }
  }

  function handleSearch() {
    currentPage = 1;
  }

  function handlePageChange(page: number) {
    currentPage = page;
  }

  function toggleSelectAll() {
    if (selectedIds.size === products.length) {
      selectedIds = new Set();
    } else {
      selectedIds = new Set(products.map((p) => p.id));
    }
  }

  function toggleSelect(id: string) {
    const next = new Set(selectedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedIds = next;
  }

  function confirmDelete(product: Product) {
    deleteTarget = product;
    showDeleteModal = true;
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteProduct(deleteTarget.id);
      notificationStore.success('Товар видалено');
      showDeleteModal = false;
      deleteTarget = null;
      loadProducts(currentPage, search, statusFilter);
    } catch {
      notificationStore.error('Помилка видалення товару');
    }
  }

  async function bulkDelete() {
    try {
      await Promise.all([...selectedIds].map((id) => catalogApi.deleteProduct(id)));
      notificationStore.success(`Видалено ${selectedIds.size} товарів`);
      selectedIds = new Set();
      showBulkDeleteModal = false;
      loadProducts(currentPage, search, statusFilter);
    } catch {
      notificationStore.error('Помилка видалення');
    }
  }

  function productStatusBadge(status: ProductStatus): string {
    const map: Record<ProductStatus, string> = {
      Draft: 'badge-outline',
      Active: 'badge-success',
      Inactive: 'badge-warning',
      SoldOut: 'badge-error',
    };
    return map[status];
  }

  function productStatusLabel(status: ProductStatus): string {
    const map: Record<ProductStatus, string> = {
      Draft: 'Чернетка',
      Active: 'Активний',
      Inactive: 'Неактивний',
      SoldOut: 'Розпродано',
    };
    return map[status];
  }

  function formatCurrency(value: number): string {
    return `₴${value.toLocaleString('uk-UA', { minimumFractionDigits: 2 })}`;
  }
</script>

<div class="products-page">
  <div class="page-header">
    <h1 class="page-title">Товари</h1>
    <a href="/admin/products/new" class="btn btn-primary">Створити товар</a>
  </div>

  <div class="toolbar">
    <div class="toolbar-left">
      <input
        class="input input-sm"
        type="search"
        placeholder="Пошук товарів..."
        bind:value={search}
        on:input={handleSearch}
      />
      <select class="input input-sm filter-select" bind:value={statusFilter}>
        <option value="">Всі статуси</option>
        <option value="Active">Активні</option>
        <option value="Draft">Чернетки</option>
        <option value="Inactive">Неактивні</option>
        <option value="SoldOut">Розпродано</option>
      </select>
    </div>
    {#if selectedIds.size > 0}
      <div class="toolbar-right">
        <span class="text-sm text-muted">Обрано: {selectedIds.size}</span>
        <button
          class="btn btn-sm btn-danger"
          on:click={() => (showBulkDeleteModal = true)}
        >
          Видалити обрані
        </button>
      </div>
    {/if}
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження товарів..." />
  {:else if products.length === 0}
    <EmptyState
      title="Товарів не знайдено"
      description="Створіть перший товар для вашого магазину"
      icon="📦"
      actionLabel="Створити товар"
      actionHref="/admin/products/new"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th class="col-check">
              <input
                type="checkbox"
                checked={selectedIds.size === products.length && products.length > 0}
                on:change={toggleSelectAll}
              />
            </th>
            <th>Фото</th>
            <th>Назва</th>
            <th>Магазин</th>
            <th>Категорія</th>
            <th>Ціна</th>
            <th>Статус</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          {#each products as product}
            <tr>
              <td class="col-check">
                <input
                  type="checkbox"
                  checked={selectedIds.has(product.id)}
                  on:change={() => toggleSelect(product.id)}
                />
              </td>
              <td>
                {#if product.mainImageUrl}
                  <img src={product.mainImageUrl} alt={product.title} class="thumb" />
                {:else}
                  <div class="thumb thumb-placeholder">📷</div>
                {/if}
              </td>
              <td class="cell-title">
                <a href="/admin/products/{product.id}" class="product-link">
                  {product.title}
                </a>
              </td>
              <td>{product.shopName}</td>
              <td>{product.categoryName}</td>
              <td class="cell-mono">
                {#if product.minPrice === product.maxPrice}
                  {formatCurrency(product.minPrice)}
                {:else}
                  {formatCurrency(product.minPrice)} – {formatCurrency(product.maxPrice)}
                {/if}
              </td>
              <td>
                <span class="badge {productStatusBadge(product.status)}">
                  {productStatusLabel(product.status)}
                </span>
              </td>
              <td class="cell-actions">
                <a href="/admin/products/{product.id}" class="btn btn-sm btn-ghost">
                  Редагувати
                </a>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  on:click={() => confirmDelete(product)}
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

<Modal open={showDeleteModal} title="Видалити товар?" onclose={() => (showDeleteModal = false)}>
  <p>Ви впевнені, що хочете видалити товар <strong>{deleteTarget?.title}</strong>?</p>
  <p class="text-sm text-muted mt-2">Цю дію неможливо скасувати.</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showDeleteModal = false)}>Скасувати</button>
    <button class="btn btn-danger" on:click={executeDelete}>Видалити</button>
  </div>
</Modal>

<Modal open={showBulkDeleteModal} title="Видалити обрані товари?" onclose={() => (showBulkDeleteModal = false)}>
  <p>Ви впевнені, що хочете видалити {selectedIds.size} товарів?</p>
  <p class="text-sm text-muted mt-2">Цю дію неможливо скасувати.</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showBulkDeleteModal = false)}>Скасувати</button>
    <button class="btn btn-danger" on:click={bulkDelete}>Видалити</button>
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
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-4);
    margin-bottom: var(--space-4);
    flex-wrap: wrap;
  }

  .toolbar-left {
    display: flex;
    gap: var(--space-3);
    flex: 1;
    max-width: 600px;
  }

  .toolbar-right {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .filter-select {
    width: auto;
    min-width: 160px;
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

  .col-check {
    width: 40px;
  }

  .thumb {
    width: 40px;
    height: 40px;
    border-radius: var(--radius-sm);
    object-fit: cover;
  }

  .thumb-placeholder {
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--color-border-light);
    font-size: 1rem;
  }

  .cell-title {
    font-weight: 500;
    max-width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .product-link {
    color: var(--color-primary);
    text-decoration: none;
  }

  .product-link:hover {
    text-decoration: underline;
  }

  .cell-mono {
    font-family: monospace;
    font-size: 0.8125rem;
  }

  .cell-actions {
    white-space: nowrap;
  }

  .btn-danger {
    background: var(--color-error);
    color: #fff;
    border: none;
  }

  .btn-danger:hover {
    opacity: 0.9;
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
