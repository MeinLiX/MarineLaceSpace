<script lang="ts">
  import Breadcrumb from '$components/Breadcrumb.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import ReviewStars from '$components/ReviewStars.svelte';
  import type { Shop } from '$types';
  import { api } from '$api/client';

  // ─── State ─────────────────────────────────────────────────────────────────

  let isLoading = $state(true);
  let shops = $state<Shop[]>([]);
  let searchQuery = $state('');

  // ─── Derived ───────────────────────────────────────────────────────────────

  let filteredShops = $derived(
    searchQuery.trim()
      ? shops.filter((s) => s.name.toLowerCase().includes(searchQuery.trim().toLowerCase()))
      : shops
  );

  let breadcrumbItems = $derived([
    { label: 'Головна', href: '/' },
    { label: 'Магазини' },
  ]);

  // ─── Data Loading ──────────────────────────────────────────────────────────

  async function loadShops() {
    isLoading = true;
    try {
      shops = await api.get<Shop[]>('/shops');
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
</script>

<svelte:head>
  <title>Наші магазини — MarineLaceSpace</title>
</svelte:head>

<div class="shops-page">
  <div class="container">
    <Breadcrumb items={breadcrumbItems} />

    <div class="shops-header">
      <h1>Наші магазини</h1>
      <div class="search-wrapper">
        <input
          class="input"
          type="search"
          placeholder="Пошук магазину…"
          bind:value={searchQuery}
          aria-label="Пошук магазину"
        />
      </div>
    </div>

    {#if isLoading}
      <LoadingSpinner size="lg" message="Завантаження магазинів…" />
    {:else if filteredShops.length === 0}
      <EmptyState
        title="Магазинів не знайдено"
        description={searchQuery ? 'Спробуйте змінити пошуковий запит' : 'Наразі немає зареєстрованих магазинів'}
        icon="🏪"
      />
    {:else}
      <div class="shops-grid">
        {#each filteredShops as shop (shop.id)}
          <a href="/shops/{shop.slug}" class="shop-card card">
            <div class="shop-logo-wrapper">
              {#if shop.logoUrl}
                <img src={shop.logoUrl} alt="{shop.name} логотип" class="shop-logo" />
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
                  {shop.productCount} товарів
                </span>
                {#if shop.averageRating > 0}
                  <span class="stat">
                    <ReviewStars rating={shop.averageRating} count={shop.reviewCount} size="sm" />
                  </span>
                {/if}
              </div>
            </div>
          </a>
        {/each}
      </div>
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
