<script lang="ts">
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Shop, Product } from '$types';

  let shopId = $derived($page.params.id!);
  let loading = $state(true);
  let saving = $state(false);
  let shop = $state<Shop | null>(null);
  let recentProducts = $state<Product[]>([]);
  let accessDenied = $state(false);

  let isOwner = $derived(
    authStore.isAdmin || (shop !== null && shop.ownerId === authStore.currentUser?.id)
  );
  let canEdit = $derived(authStore.isAdmin || isOwner);

  let name = $state('');
  let description = $state('');
  let logoUrl = $state('');
  let bannerUrl = $state('');

  $effect(() => {
    loadShop(shopId);
  });

  async function loadShop(id: string) {
    try {
      loading = true;
      const [shopRes, productsRes] = await Promise.all([
        catalogApi.getShopById(id),
        catalogApi.getProductsByShop(id, { page: 1, pageSize: 5 }),
      ]);
      shop = shopRes;
      recentProducts = productsRes.items;

      if (!authStore.isAdmin && shopRes.ownerId !== authStore.currentUser?.id) {
        accessDenied = true;
        return;
      }

      name = shopRes.name;
      description = shopRes.description;
      logoUrl = shopRes.logoUrl ?? '';
      bannerUrl = shopRes.bannerUrl ?? '';
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingShop'));
    } finally {
      loading = false;
    }
  }

  async function save() {
    if (!name.trim()) {
      notificationStore.warning(i18n.t('admin.enterShopName'));
      return;
    }
    try {
      saving = true;
      await catalogApi.updateShop(shopId, {
        name: name.trim(),
        description: description.trim(),
        logoUrl: logoUrl.trim() || undefined,
        bannerUrl: bannerUrl.trim() || undefined,
      });
      notificationStore.success(i18n.t('admin.shopUpdated'));
    } catch {
      notificationStore.error(i18n.t('admin.errorSaving'));
    } finally {
      saving = false;
    }
  }

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA');
  }

  function formatCurrency(value: number): string {
    return `₴${value.toLocaleString('uk-UA', { minimumFractionDigits: 2 })}`;
  }
</script>

<div class="shop-detail">
  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingShop')} />
  {:else if accessDenied}
    <div class="access-denied card">
      <div class="card-body" style="text-align: center; padding: var(--space-8);">
        <p style="font-size: 1.5rem; margin-bottom: var(--space-2);">🚫</p>
        <p class="text-muted" style="font-size: 1.125rem;">{i18n.t('admin.accessDenied')}</p>
        <p class="text-muted text-sm" style="margin-top: var(--space-2);">{i18n.t('admin.notYourShop')}</p>
        <a href="/admin/shops" class="btn btn-outline" style="margin-top: var(--space-4);">{i18n.t('common.back')}</a>
      </div>
    </div>
  {:else if shop}
    <div class="page-header">
      <div>
        <h1 class="page-title">{shop.name}</h1>
        {#if !authStore.isAdmin && isOwner}
          <span class="badge badge-success" style="margin-top: var(--space-1);">
            ✅ {i18n.t('admin.yourShop')}
          </span>
        {/if}
      </div>
      <div class="header-actions">
        <a href="/admin/shops" class="btn btn-outline">{i18n.t('common.back')}</a>
        {#if canEdit}
          <button class="btn btn-primary" onclick={save} disabled={saving}>
            {saving ? i18n.t('common.saving') : i18n.t('common.save')}
          </button>
        {/if}
      </div>
    </div>

    <!-- Cross-links bar -->
    <div class="cross-links">
      <a href="/admin/products?shopId={shop.id}" class="cross-link">
        <span class="cross-icon">📦</span> {i18n.t('admin.products')} ({shop.productCount})
      </a>
      <a href="/admin/orders?shopId={shop.id}" class="cross-link">
        <span class="cross-icon">🧾</span> {i18n.t('admin.orders')}
      </a>
      <a href="/shops/{shop.slug || shop.id}" class="cross-link" target="_blank" rel="noopener">
        <span class="cross-icon">🌐</span> {i18n.t('admin.viewPublicPage')}
      </a>
    </div>

    <!-- Stats -->
    <div class="stats-row">
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">{i18n.t('admin.products')}</span>
          <span class="stat-value">{shop.productCount}</span>
        </div>
      </div>
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">{i18n.t('admin.status')}</span>
          <span class="stat-value">
            <span class="badge {shop.isActive ? 'badge-success' : 'badge-error'}">
              {shop.isActive ? i18n.t('admin.active') : i18n.t('admin.inactive')}
            </span>
          </span>
        </div>
      </div>
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">{i18n.t('admin.created')}</span>
          <span class="stat-value text-sm">{formatDate(shop.createdAt)}</span>
        </div>
      </div>
    </div>

    <div class="detail-grid">
      <!-- Edit form -->
      {#if canEdit}
      <section class="card">
        <div class="card-header">
          <h2>{i18n.t('admin.shopInfo')}</h2>
        </div>
        <div class="card-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label" for="shopName">{i18n.t('admin.name')}</label>
              <input id="shopName" class="input" type="text" bind:value={name} />
            </div>

            <div class="form-group">
              <label class="form-label" for="shopSlug">Slug</label>
              <input id="shopSlug" class="input" type="text" value={shop.slug} disabled />
            </div>

            <div class="form-group full">
              <label class="form-label" for="shopDesc">{i18n.t('admin.description')}</label>
              <textarea id="shopDesc" class="input" rows="4" bind:value={description}></textarea>
            </div>

            <div class="form-group">
              <label class="form-label" for="logoUrl">{i18n.t('admin.logoUrl')}</label>
              <input id="logoUrl" class="input" type="url" bind:value={logoUrl} placeholder="https://..." />
            </div>

            <div class="form-group">
              <label class="form-label" for="bannerUrl">{i18n.t('admin.bannerUrl')}</label>
              <input id="bannerUrl" class="input" type="url" bind:value={bannerUrl} placeholder="https://..." />
            </div>
          </div>
        </div>
      </section>
      {:else}
      <section class="card">
        <div class="card-header">
          <h2>{i18n.t('admin.shopInfo')}</h2>
        </div>
        <div class="card-body">
          <p><strong>{i18n.t('admin.name')}:</strong> {shop.name}</p>
          <p><strong>Slug:</strong> {shop.slug}</p>
          <p><strong>{i18n.t('admin.description')}:</strong> {shop.description}</p>
        </div>
      </section>
      {/if}

      <!-- Owner info -->
      <section class="card">
        <div class="card-header">
          <h2>{i18n.t('admin.owner')}</h2>
        </div>
        <div class="card-body">
          <p><strong>ID:</strong> <span class="mono">{shop.ownerId}</span></p>
        </div>
      </section>
    </div>

    <!-- Recent products -->
    {#if recentProducts.length > 0}
      <section class="card mt-6">
        <div class="card-header">
          <h2>{i18n.t('admin.recentProducts')}</h2>
        </div>
        <div class="card-body" style="padding: 0;">
          <div class="table-wrapper">
            <table class="data-table">
              <thead>
                <tr>
                  <th>{i18n.t('admin.name')}</th>
                  <th>{i18n.t('admin.status')}</th>
                  <th>{i18n.t('admin.price')}</th>
                </tr>
              </thead>
              <tbody>
                {#each recentProducts as product}
                  <tr>
                    <td>
                      <a href="/admin/products/{product.id}" class="product-link">
                        {product.name}
                      </a>
                    </td>
                    <td>
                      <span class="badge {product.isActive ? 'badge-success' : 'badge-error'}">
                        {product.isActive ? i18n.t('admin.active') : i18n.t('admin.inactive')}
                      </span>
                    </td>
                    <td class="cell-mono">{formatCurrency(product.price)}</td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    {/if}
  {:else}
    <p class="text-muted">{i18n.t('admin.shopNotFound')}</p>
  {/if}
</div>

<style>
  .page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .page-title {
    font-size: 1.75rem;
  }

  .header-actions {
    display: flex;
    gap: var(--space-3);
  }

  .cross-links {
    display: flex;
    gap: var(--space-3);
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
  }

  .cross-link {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-2) var(--space-4);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    color: var(--color-text);
    text-decoration: none;
    font-size: 0.875rem;
    font-weight: 500;
    transition: all var(--transition-fast);
  }

  .cross-link:hover {
    border-color: var(--color-primary);
    background: var(--color-surface-hover);
    color: var(--color-primary);
  }

  .cross-icon {
    font-size: 1rem;
  }

  .stats-row {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-4);
    margin-bottom: var(--space-6);
  }

  .stat-card .card-body {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: var(--space-4);
  }

  .stat-label {
    font-size: 0.75rem;
    color: var(--color-text-light);
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: var(--space-1);
  }

  .stat-value {
    font-size: 1.25rem;
    font-weight: 700;
    font-family: var(--font-display);
  }

  .detail-grid {
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: var(--space-6);
    align-items: start;
  }

  .form-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-4);
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .form-group.full {
    grid-column: 1 / -1;
  }

  .form-label {
    font-size: 0.8125rem;
    font-weight: 600;
  }

  .mono {
    font-family: monospace;
    font-size: 0.8125rem;
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

  .product-link {
    color: var(--color-primary);
    text-decoration: none;
  }

  .product-link:hover {
    text-decoration: underline;
  }

  .mt-6 {
    margin-top: var(--space-6);
  }

  .text-sm {
    font-size: 0.875rem;
  }

  @media (max-width: 1024px) {
    .stats-row {
      grid-template-columns: repeat(2, 1fr);
    }
    .detail-grid {
      grid-template-columns: 1fr;
    }
  }

  @media (max-width: 640px) {
    .stats-row {
      grid-template-columns: 1fr;
    }
    .form-grid {
      grid-template-columns: 1fr;
    }
  }
</style>
