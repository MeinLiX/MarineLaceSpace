<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Shop } from '$types';

  let loading = $state(true);
  let shops = $state<Shop[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');

  let showDeleteModal = $state(false);
  let deleteTarget = $state<Shop | null>(null);

  let showCreateModal = $state(false);
  let newShopName = $state('');
  let newShopDescription = $state('');
  let isCreating = $state(false);

  $effect(() => {
    loadShops(currentPage, search);
  });

  async function loadShops(page: number, q: string) {
    try {
      loading = true;

      if (authStore.isAdmin) {
        const result = await catalogApi.getShops({
          page,
          pageSize: 20,
          search: q || undefined,
        });
        shops = result.items;
        totalPages = result.totalPages;
      } else {
        const myShops = await catalogApi.getMyShops();
        const filtered = q
          ? myShops.filter((s) => s.name.toLowerCase().includes(q.toLowerCase()))
          : myShops;
        shops = filtered;
        totalPages = 1;
      }
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingShops'));
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
      notificationStore.success(i18n.t('admin.shopDeleted'));
      showDeleteModal = false;
      deleteTarget = null;
      loadShops(currentPage, search);
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingShop'));
    }
  }

  async function handleCreateShop() {
    if (!newShopName.trim()) return;
    isCreating = true;
    try {
      await catalogApi.createShop({
        name: newShopName.trim(),
        description: newShopDescription.trim()
      });
      notificationStore.success(i18n.t('admin.shopCreated'));
      showCreateModal = false;
      newShopName = '';
      newShopDescription = '';
      loadShops(currentPage, search);
    } catch {
      notificationStore.error(i18n.t('admin.errorSaving'));
    } finally {
      isCreating = false;
    }
  }

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA');
  }
</script>

<div class="shops-page">
  <div class="page-header">
    <h1 class="page-title">{i18n.t('admin.shops')}</h1>
    <button class="btn btn-primary" onclick={() => (showCreateModal = true)}>{i18n.t('admin.createShop')}</button>
  </div>

  <div class="toolbar">
    <input
      class="input input-sm search-input"
      type="search"
      placeholder={i18n.t('admin.searchShops')}
      bind:value={search}
      oninput={() => { currentPage = 1; }}
    />
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingShops')} />
  {:else if shops.length === 0}
    <EmptyState title={i18n.t('admin.noShopsFound')} icon="🏪" />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>{i18n.t('admin.logo')}</th>
            <th>{i18n.t('admin.name')}</th>
            <th>{i18n.t('admin.status')}</th>
            <th>{i18n.t('admin.products')}</th>
            <th>{i18n.t('admin.createdDate')}</th>
            <th>{i18n.t('admin.quickLinks')}</th>
            <th>{i18n.t('admin.actions')}</th>
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
                {#if shop.slug}
                  <span class="slug-hint">/{shop.slug}</span>
                {/if}
              </td>
              <td>
                <span class="status-badge" class:active={shop.isActive} class:inactive={!shop.isActive}>
                  <span class="status-dot"></span>
                  {shop.isActive ? i18n.t('admin.active') : i18n.t('admin.inactive')}
                </span>
              </td>
              <td class="text-center">
                <a href="/admin/products?shopId={shop.id}" class="count-link">{shop.productCount}</a>
              </td>
              <td>{formatDate(shop.createdAt)}</td>
              <td class="cell-links">
                <a href="/admin/products?shopId={shop.id}" class="quick-link" title="Products">📦</a>
                <a href="/admin/orders?shopId={shop.id}" class="quick-link" title="Orders">🧾</a>
                <a href="/shops/{shop.slug || shop.id}" class="quick-link" title="Public page">🌐</a>
              </td>
              <td class="cell-actions">
                <a href="/admin/shops/{shop.id}" class="btn btn-sm btn-ghost">{i18n.t('admin.details')}</a>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  onclick={() => confirmDelete(shop)}
                >
                  {i18n.t('common.delete')}
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

<Modal open={showDeleteModal} title={i18n.t('admin.deleteShopQuestion')} onclose={() => (showDeleteModal = false)}>
  <p>{i18n.t('admin.confirmDeleteShop', { name: deleteTarget?.name ?? '' })}</p>
  <p class="text-sm text-muted mt-2">{i18n.t('admin.shopProductsAlsoDeleted')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-danger" onclick={executeDelete}>{i18n.t('common.delete')}</button>
  </div>
</Modal>

<Modal open={showCreateModal} title={i18n.t('admin.newShop')} onclose={() => (showCreateModal = false)}>
  <form onsubmit={(e) => { e.preventDefault(); handleCreateShop(); }}>
    <div class="form-group">
      <label class="form-label" for="newShopName">{i18n.t('admin.name')}</label>
      <input
        id="newShopName"
        class="input"
        type="text"
        bind:value={newShopName}
        placeholder={i18n.t('admin.shopNamePlaceholder')}
        required
      />
    </div>
    <div class="form-group">
      <label class="form-label" for="newShopDesc">{i18n.t('admin.description')}</label>
      <textarea
        id="newShopDesc"
        class="input"
        bind:value={newShopDescription}
        placeholder={i18n.t('admin.shopDescriptionPlaceholder')}
        rows="3"
      ></textarea>
    </div>
    <div class="modal-actions">
      <button type="button" class="btn btn-outline" onclick={() => (showCreateModal = false)}>{i18n.t('common.cancel')}</button>
      <button type="submit" class="btn btn-primary" disabled={isCreating || !newShopName.trim()}>
        {isCreating ? i18n.t('common.saving') : i18n.t('admin.createShop')}
      </button>
    </div>
  </form>
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

  .data-table tbody tr {
    transition: background var(--transition-fast);
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

  .slug-hint {
    display: block;
    font-size: 0.7rem;
    color: var(--color-text-light);
    font-weight: 400;
  }

  .status-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    font-size: 0.8rem;
    font-weight: 500;
    padding: 2px 10px 2px 8px;
    border-radius: 999px;
  }

  .status-badge.active {
    color: #15803d;
    background: #dcfce7;
  }

  .status-badge.inactive {
    color: #6b7280;
    background: #f3f4f6;
  }

  .status-dot {
    width: 7px;
    height: 7px;
    border-radius: 50%;
    background: currentColor;
  }

  .count-link {
    color: var(--color-primary);
    font-weight: 600;
    text-decoration: none;
  }

  .count-link:hover {
    text-decoration: underline;
  }

  .cell-links {
    white-space: nowrap;
    display: flex;
    gap: var(--space-2);
  }

  .quick-link {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 30px;
    height: 30px;
    border-radius: var(--radius-sm);
    text-decoration: none;
    font-size: 1rem;
    transition: background var(--transition-fast);
  }

  .quick-link:hover {
    background: var(--color-surface-hover);
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
