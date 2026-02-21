<script lang="ts">
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import ReviewStars from '$components/ReviewStars.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Shop, Product } from '$types';

  let shopId = $derived($page.params.id);
  let loading = $state(true);
  let saving = $state(false);
  let shop = $state<Shop | null>(null);
  let recentProducts = $state<Product[]>([]);

  // Form state
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

      // Populate form
      name = shopRes.name;
      description = shopRes.description;
      logoUrl = shopRes.logoUrl ?? '';
      bannerUrl = shopRes.bannerUrl ?? '';
    } catch {
      notificationStore.error('Помилка завантаження магазину');
    } finally {
      loading = false;
    }
  }

  async function save() {
    if (!name.trim()) {
      notificationStore.warning('Введіть назву магазину');
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
      notificationStore.success('Магазин оновлено');
    } catch {
      notificationStore.error('Помилка збереження');
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
    <LoadingSpinner message="Завантаження магазину..." />
  {:else if shop}
    <div class="page-header">
      <h1 class="page-title">{shop.name}</h1>
      <div class="header-actions">
        <a href="/admin/shops" class="btn btn-outline">Назад</a>
        <button class="btn btn-primary" on:click={save} disabled={saving}>
          {saving ? 'Збереження...' : 'Зберегти'}
        </button>
      </div>
    </div>

    <!-- Stats -->
    <div class="stats-row">
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">Товари</span>
          <span class="stat-value">{shop.productCount}</span>
        </div>
      </div>
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">Рейтинг</span>
          <span class="stat-value">
            <ReviewStars rating={shop.averageRating} count={shop.reviewCount} size="sm" />
          </span>
        </div>
      </div>
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">Відгуки</span>
          <span class="stat-value">{shop.reviewCount}</span>
        </div>
      </div>
      <div class="stat-card card">
        <div class="card-body">
          <span class="stat-label">Створено</span>
          <span class="stat-value text-sm">{formatDate(shop.createdAt)}</span>
        </div>
      </div>
    </div>

    <div class="detail-grid">
      <!-- Edit form -->
      <section class="card">
        <div class="card-header">
          <h2>Інформація про магазин</h2>
        </div>
        <div class="card-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label" for="shopName">Назва</label>
              <input id="shopName" class="input" type="text" bind:value={name} />
            </div>

            <div class="form-group">
              <label class="form-label">Slug</label>
              <input class="input" type="text" value={shop.slug} disabled />
            </div>

            <div class="form-group full">
              <label class="form-label" for="shopDesc">Опис</label>
              <textarea id="shopDesc" class="input" rows="4" bind:value={description}></textarea>
            </div>

            <div class="form-group">
              <label class="form-label" for="logoUrl">URL логотипу</label>
              <input id="logoUrl" class="input" type="url" bind:value={logoUrl} placeholder="https://..." />
            </div>

            <div class="form-group">
              <label class="form-label" for="bannerUrl">URL банеру</label>
              <input id="bannerUrl" class="input" type="url" bind:value={bannerUrl} placeholder="https://..." />
            </div>
          </div>
        </div>
      </section>

      <!-- Owner info -->
      <section class="card">
        <div class="card-header">
          <h2>Власник</h2>
        </div>
        <div class="card-body">
          <p><strong>ID:</strong> <span class="mono">{shop.ownerId}</span></p>
          <p><strong>Ім'я:</strong> {shop.ownerName}</p>
        </div>
      </section>
    </div>

    <!-- Recent products -->
    {#if recentProducts.length > 0}
      <section class="card mt-6">
        <div class="card-header">
          <h2>Останні товари</h2>
        </div>
        <div class="card-body" style="padding: 0;">
          <div class="table-wrapper">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Назва</th>
                  <th>Статус</th>
                  <th>Ціна</th>
                  <th>Рейтинг</th>
                </tr>
              </thead>
              <tbody>
                {#each recentProducts as product}
                  <tr>
                    <td>
                      <a href="/admin/products/{product.id}" class="product-link">
                        {product.title}
                      </a>
                    </td>
                    <td>
                      <span class="badge {product.status === 'Active' ? 'badge-success' : 'badge-outline'}">
                        {product.status}
                      </span>
                    </td>
                    <td class="cell-mono">{formatCurrency(product.minPrice)}</td>
                    <td>
                      <ReviewStars rating={product.averageRating} size="sm" />
                    </td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    {/if}
  {:else}
    <p class="text-muted">Магазин не знайдено.</p>
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

  .stats-row {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
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
