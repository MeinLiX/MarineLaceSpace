<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Pagination from '$components/Pagination.svelte';
  import type { Shop } from '$types';
  import { getShops } from '$api/catalog';

  // ─── State ─────────────────────────────────────────────────────────────────

  let isLoading = $state(true);
  let shops = $state<Shop[]>([]);
  let searchQuery = $state('');
  let currentPage = $state(1);
  let totalPages = $state(1);

  // ─── Data Loading ──────────────────────────────────────────────────────────

  async function loadShops() {
    isLoading = true;
    try {
      const result = await getShops({
        page: currentPage,
        pageSize: 24,
        search: searchQuery.trim() || undefined,
      });
      shops = result.items;
      totalPages = result.totalPages;
    } catch (err) {
      console.error('Failed to load shops', err);
      shops = [];
    } finally {
      isLoading = false;
    }
  }

  $effect(() => {
    loadShops();
  });

  function handleSearch() {
    currentPage = 1;
  }

  function handlePageChange(page: number) {
    currentPage = page;
  }
</script>

<svelte:head>
  <title>{i18n.t('shops.title')} — MarineLaceSpace</title>
</svelte:head>

<div class="shops-page">
  <div class="container">
    <div class="shops-header">
      <h1>{i18n.t('shops.title')}</h1>
      <div class="search-wrapper">
        <input
          class="input"
          type="search"
          placeholder={i18n.t('shops.search')}
          bind:value={searchQuery}
          aria-label={i18n.t('shops.search')}
          oninput={handleSearch}
        />
      </div>
    </div>

    {#if isLoading}
      <LoadingSpinner size="lg" message={i18n.t('shops.loadingShops')} />
    {:else if shops.length === 0}
      <EmptyState
        title={i18n.t('shops.noShops')}
        description={searchQuery ? i18n.t('shops.noShopsSearchDescription') : i18n.t('shops.noShopsDescription')}
        icon="🏪"
      />
    {:else}
      <div class="shops-grid">
        {#each shops as shop (shop.id)}
          <a href="/shops/{shop.slug}" class="shop-card card">
            <div class="shop-logo-wrapper">
              {#if shop.logoUrl}
                <img src={shop.logoUrl} alt={i18n.t('shops.shopLogo', { name: shop.name })} class="shop-logo" />
              {:else}
                <div class="shop-logo-placeholder" aria-hidden="true">
                  <span>🏪</span>
                </div>
              {/if}
            </div>
            <div class="card-body">
              <h2 class="shop-name">{shop.name}</h2>
              {#if shop.description}
                <p class="shop-description">{shop.description.length > 120 ? shop.description.slice(0, 120) + '…' : shop.description}</p>
              {/if}
              <div class="shop-stats">
                <span class="stat">
                  <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                    <path d="M20.59 13.41l-7.17 7.17a2 2 0 01-2.83 0L2 12V2h10l8.59 8.59a2 2 0 010 2.82z"/>
                    <line x1="7" y1="7" x2="7.01" y2="7"/>
                  </svg>
                  {i18n.t('shops.products', { count: shop.productCount })}
                </span>
              </div>
            </div>
          </a>
        {/each}
      </div>

      <Pagination {currentPage} {totalPages} onPageChange={handlePageChange} />
    {/if}
  </div>
</div>

<style>
  .shops-page {
    padding-block: var(--space-4) var(--space-16);
  }

  .shops-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: var(--space-4);
    margin-bottom: var(--space-8);
  }

  .shops-header h1 {
    font-family: var(--font-display);
    font-size: 2rem;
    margin: 0;
  }

  .search-wrapper {
    min-width: 240px;
    max-width: 360px;
    flex: 1;
  }

  .shops-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-6);
  }

  .shop-card {
    display: flex;
    flex-direction: column;
    text-decoration: none;
    color: inherit;
    transition: box-shadow var(--transition-base), transform var(--transition-base);
  }

  .shop-card:hover {
    box-shadow: var(--shadow-lg);
    transform: translateY(-2px);
  }

  .shop-logo-wrapper {
    aspect-ratio: 16 / 9;
    background-color: var(--color-surface-hover);
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
  }

  .shop-logo {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .shop-logo-placeholder {
    font-size: 3rem;
    opacity: 0.4;
  }

  .shop-name {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    margin-bottom: var(--space-2);
    color: var(--color-text);
  }

  .shop-description {
    font-size: 0.875rem;
    color: var(--color-text-light);
    line-height: 1.5;
    margin-bottom: var(--space-3);
  }

  .shop-stats {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    flex-wrap: wrap;
  }

  .stat {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  @media (max-width: 1023px) {
    .shops-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }

  @media (max-width: 639px) {
    .shops-grid {
      grid-template-columns: 1fr;
    }

    .shops-header {
      flex-direction: column;
      align-items: flex-start;
    }

    .search-wrapper {
      max-width: 100%;
      width: 100%;
    }

    .shops-header h1 {
      font-size: 1.5rem;
    }
  }
</style>
