<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import CategoryTree from '$components/CategoryTree.svelte';
  import ProductCard from '$components/ProductCard.svelte';
  import Pagination from '$components/Pagination.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import { getProducts, getCategoryTree, getSizes, getColors, getMaterials, type ProductFilters } from '$api/catalog';
  import type { Product, Category, PaginatedResponse, Size, Color, Material } from '$types';

  // ─── State ─────────────────────────────────────────────────────────────────

  let isLoading = $state(true);
  let products = $state<Product[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let categories = $state<Category[]>([]);
  let allSizes = $state<Size[]>([]);
  let allColors = $state<Color[]>([]);
  let allMaterials = $state<Material[]>([]);
  let mobileFiltersOpen = $state(false);

  // ─── Filters ───────────────────────────────────────────────────────────────

  let selectedCategory = $state<string | undefined>(undefined);
  let minPrice = $state<string>('');
  let maxPrice = $state<string>('');
  let selectedSizes = $state<Set<string>>(new Set());
  let selectedColors = $state<Set<string>>(new Set());
  let selectedMaterials = $state<Set<string>>(new Set());
  let sortBy = $state('popular');

  let sortOptions = $derived([
    { value: 'popular', label: i18n.t('catalog.sortPopular') },
    { value: 'price_asc', label: i18n.t('catalog.sortPriceAsc') },
    { value: 'price_desc', label: i18n.t('catalog.sortPriceDesc') },
    { value: 'newest', label: i18n.t('catalog.sortNewest') },
    { value: 'rating', label: i18n.t('catalog.sortRating') },
  ]);

  // ─── URL Sync ──────────────────────────────────────────────────────────────

  function readUrlParams() {
    const params = new URL(window.location.href).searchParams;
    selectedCategory = params.get('category') ?? undefined;
    minPrice = params.get('minPrice') ?? '';
    maxPrice = params.get('maxPrice') ?? '';
    sortBy = params.get('sort') ?? 'popular';
    currentPage = parseInt(params.get('page') ?? '1', 10);
  }

  function buildUrl(): string {
    const params = new URLSearchParams();
    if (selectedCategory) params.set('category', selectedCategory);
    if (minPrice) params.set('minPrice', minPrice);
    if (maxPrice) params.set('maxPrice', maxPrice);
    if (sortBy !== 'popular') params.set('sort', sortBy);
    if (currentPage > 1) params.set('page', String(currentPage));
    const qs = params.toString();
    return `/catalog${qs ? '?' + qs : ''}`;
  }

  function getSortParams(): Pick<ProductFilters, 'sortBy' | 'sortOrder'> {
    switch (sortBy) {
      case 'price_asc': return { sortBy: 'price', sortOrder: 'asc' };
      case 'price_desc': return { sortBy: 'price', sortOrder: 'desc' };
      case 'newest': return { sortBy: 'createdAt', sortOrder: 'desc' };
      case 'rating': return { sortBy: 'rating', sortOrder: 'desc' };
      default: return { sortBy: 'popular', sortOrder: 'desc' };
    }
  }

  // ─── Data Loading ──────────────────────────────────────────────────────────

  async function loadProducts() {
    isLoading = true;
    try {
      const sortParams = getSortParams();
      const filters: ProductFilters = {
        page: currentPage,
        pageSize: 12,
        categoryId: selectedCategory,
        minPrice: minPrice ? Number(minPrice) : undefined,
        maxPrice: maxPrice ? Number(maxPrice) : undefined,
        ...sortParams,
      };

      const result: PaginatedResponse<Product> = await getProducts(filters);
      products = result.items;
      totalPages = result.totalPages;
      currentPage = result.page;
    } catch (err) {
      console.error('Failed to load products', err);
      products = [];
    } finally {
      isLoading = false;
    }
  }

  async function loadFiltersData() {
    try {
      const [cats, sizes, colors, materials] = await Promise.all([
        getCategoryTree(),
        getSizes(),
        getColors(),
        getMaterials(),
      ]);
      categories = cats;
      allSizes = sizes;
      allColors = colors;
      allMaterials = materials;
    } catch (err) {
      console.error('Failed to load filter data', err);
    }
  }

  // ─── Actions ───────────────────────────────────────────────────────────────

  function applyFilters() {
    currentPage = 1;
    mobileFiltersOpen = false;
    goto(buildUrl(), { replaceState: true, noScroll: true });
    loadProducts();
  }

  function clearFilters() {
    selectedCategory = undefined;
    minPrice = '';
    maxPrice = '';
    selectedSizes = new Set();
    selectedColors = new Set();
    selectedMaterials = new Set();
    sortBy = 'popular';
    applyFilters();
  }

  function handlePageChange(p: number) {
    currentPage = p;
    goto(buildUrl(), { replaceState: true });
    loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function handleSortChange(e: Event) {
    sortBy = (e.target as HTMLSelectElement).value;
    applyFilters();
  }

  function toggleSize(id: string) {
    const next = new Set(selectedSizes);
    if (next.has(id)) next.delete(id); else next.add(id);
    selectedSizes = next;
  }

  function toggleColor(id: string) {
    const next = new Set(selectedColors);
    if (next.has(id)) next.delete(id); else next.add(id);
    selectedColors = next;
  }

  function toggleMaterial(id: string) {
    const next = new Set(selectedMaterials);
    if (next.has(id)) next.delete(id); else next.add(id);
    selectedMaterials = next;
  }

  // ─── Lifecycle ─────────────────────────────────────────────────────────────

  $effect(() => {
    readUrlParams();
    loadFiltersData();
    loadProducts();
  });

  function mapProductToCard(p: Product) {
    return {
      id: p.id,
      title: p.name,
      shopName: p.shopName,
      imageUrl: p.mainImageUrl ?? undefined,
      basePrice: p.price,
    };
  }
</script>

<svelte:head>
  <title>{i18n.t('catalog.title')} — MarineLaceSpace</title>
</svelte:head>

<div class="catalog-page">
  <div class="container">
    <div class="catalog-header">
      <h1>{i18n.t('catalog.title')}</h1>
      <div class="catalog-controls">
        <button
          class="btn btn-outline filter-toggle"
          onclick={() => (mobileFiltersOpen = !mobileFiltersOpen)}
          aria-expanded={mobileFiltersOpen}
          aria-controls="filters-sidebar"
        >
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <line x1="4" y1="6" x2="20" y2="6" /><line x1="4" y1="12" x2="16" y2="12" /><line x1="4" y1="18" x2="12" y2="18" />
          </svg>
          {i18n.t('catalog.filters')}
        </button>
        <div class="sort-wrapper">
          <label for="sort-select" class="sr-only">{i18n.t('catalog.sort')}</label>
          <select id="sort-select" class="input input-sm sort-select" value={sortBy} onchange={handleSortChange}>
            {#each sortOptions as opt (opt.value)}
              <option value={opt.value}>{opt.label}</option>
            {/each}
          </select>
        </div>
      </div>
    </div>

    <div class="catalog-layout">
      <!-- Sidebar -->
      <aside
        id="filters-sidebar"
        class="sidebar"
        class:open={mobileFiltersOpen}
        aria-label={i18n.t('catalog.filters')}
      >
        <div class="sidebar-inner">
          <div class="sidebar-header">
            <h2 class="sidebar-title">{i18n.t('catalog.filters')}</h2>
            <button class="btn btn-ghost btn-sm sidebar-close" onclick={() => (mobileFiltersOpen = false)} aria-label={i18n.t('common.close')}>
              ✕
            </button>
          </div>

          <!-- Category Tree -->
          <section class="filter-section">
            <h3 class="filter-heading">{i18n.t('catalog.categories')}</h3>
            <CategoryTree categories={categories} selectedId={selectedCategory} />
          </section>

          <!-- Price Range -->
          <section class="filter-section">
            <h3 class="filter-heading">{i18n.t('catalog.priceRange')}</h3>
            <div class="price-range-inputs">
              <input
                type="number"
                class="input input-sm"
                placeholder={i18n.t('catalog.minPrice')}
                bind:value={minPrice}
                min="0"
                aria-label={i18n.t('catalog.minPrice')}
              />
              <span class="price-divider">—</span>
              <input
                type="number"
                class="input input-sm"
                placeholder={i18n.t('catalog.maxPrice')}
                bind:value={maxPrice}
                min="0"
                aria-label={i18n.t('catalog.maxPrice')}
              />
            </div>
          </section>

          <!-- Sizes -->
          {#if allSizes.length > 0}
            <section class="filter-section">
              <h3 class="filter-heading">{i18n.t('product.size')}</h3>
              <div class="checkbox-group">
                {#each allSizes as size (size.id)}
                  <label class="checkbox-label">
                    <input
                      type="checkbox"
                      checked={selectedSizes.has(size.id)}
                      onchange={() => toggleSize(size.id)}
                    />
                    <span>{size.name}</span>
                  </label>
                {/each}
              </div>
            </section>
          {/if}

          <!-- Colors -->
          {#if allColors.length > 0}
            <section class="filter-section">
              <h3 class="filter-heading">{i18n.t('product.color')}</h3>
              <div class="color-swatches">
                {#each allColors as color (color.id)}
                  <button
                    class="color-swatch"
                    class:selected={selectedColors.has(color.id)}
                    style="--swatch-color: {color.hexCode}"
                    onclick={() => toggleColor(color.id)}
                    title={color.name}
                    aria-label={color.name}
                    aria-pressed={selectedColors.has(color.id)}
                  ></button>
                {/each}
              </div>
            </section>
          {/if}

          <!-- Materials -->
          {#if allMaterials.length > 0}
            <section class="filter-section">
              <h3 class="filter-heading">{i18n.t('product.materials')}</h3>
              <div class="checkbox-group">
                {#each allMaterials as material (material.id)}
                  <label class="checkbox-label">
                    <input
                      type="checkbox"
                      checked={selectedMaterials.has(material.id)}
                      onchange={() => toggleMaterial(material.id)}
                    />
                    <span>{material.name}</span>
                  </label>
                {/each}
              </div>
            </section>
          {/if}

          <div class="filter-actions">
            <button class="btn btn-primary w-full" onclick={applyFilters}>
              {i18n.t('catalog.apply')}
            </button>
            <button class="btn btn-ghost w-full" onclick={clearFilters}>
              {i18n.t('catalog.reset')}
            </button>
          </div>
        </div>
      </aside>

      <!-- Backdrop for mobile -->
      {#if mobileFiltersOpen}
        <button
          class="sidebar-backdrop"
          onclick={() => (mobileFiltersOpen = false)}
          aria-label={i18n.t('common.close')}
        ></button>
      {/if}

      <!-- Product Grid -->
      <section class="catalog-content" aria-label={i18n.t('catalog.products')}>
        {#if isLoading}
          <LoadingSpinner size="lg" message={i18n.t('common.loading')} />
        {:else if products.length === 0}
          <EmptyState
            title={i18n.t('emptyState.noProducts')}
            description={i18n.t('emptyState.noProductsDescription')}
            icon="🔍"
            actionLabel={i18n.t('catalog.reset')}
          />
        {:else}
          <div class="product-grid">
            {#each products as product (product.id)}
              <ProductCard product={mapProductToCard(product)} />
            {/each}
          </div>

          <div class="pagination-wrapper">
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={handlePageChange}
            />
          </div>
        {/if}
      </section>
    </div>
  </div>
</div>

<style>
  .catalog-page {
    padding-block: var(--space-4) var(--space-16);
  }

  .catalog-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: var(--space-4);
    margin-bottom: var(--space-6);
  }

  .catalog-header h1 {
    font-family: var(--font-display);
    font-size: 2rem;
    margin: 0;
  }

  .catalog-controls {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .sort-select {
    min-width: 180px;
  }

  .catalog-layout {
    display: grid;
    grid-template-columns: 260px 1fr;
    gap: var(--space-8);
    position: relative;
  }

  /* ─── Sidebar ───────────────────────────────────────────────────────────── */

  .sidebar {
    position: sticky;
    top: var(--space-4);
    align-self: start;
    max-height: calc(100vh - var(--space-8));
    overflow-y: auto;
  }

  .sidebar-inner {
    display: flex;
    flex-direction: column;
    gap: var(--space-6);
  }

  .sidebar-header {
    display: none;
    align-items: center;
    justify-content: space-between;
  }

  .sidebar-title {
    font-size: 1.25rem;
    margin: 0;
  }

  .filter-toggle {
    display: none;
  }

  .filter-section {
    border-bottom: 1px solid var(--color-border-light);
    padding-bottom: var(--space-5);
  }

  .filter-heading {
    font-family: var(--font-body);
    font-size: 0.8125rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-text-light);
    margin-bottom: var(--space-3);
  }

  .price-range-inputs {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }

  .price-range-inputs input {
    max-width: 100px;
  }

  .price-divider {
    color: var(--color-text-muted);
  }

  .checkbox-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .checkbox-label {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    font-size: 0.875rem;
    cursor: pointer;
    color: var(--color-text);
  }

  .checkbox-label input[type='checkbox'] {
    accent-color: var(--color-primary);
    width: 16px;
    height: 16px;
    cursor: pointer;
  }

  .color-swatches {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }

  .color-swatch {
    width: 28px;
    height: 28px;
    border-radius: var(--radius-full);
    background-color: var(--swatch-color);
    border: 2px solid var(--color-border);
    cursor: pointer;
    transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    padding: 0;
  }

  .color-swatch:hover {
    border-color: var(--color-primary-light);
  }

  .color-swatch.selected {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 2px var(--color-primary-light);
  }

  .filter-actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .sidebar-backdrop {
    display: none;
  }

  /* ─── Product Grid ──────────────────────────────────────────────────────── */

  .catalog-content {
    min-height: 400px;
  }

  .product-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-6);
  }

  .pagination-wrapper {
    margin-top: var(--space-8);
    display: flex;
    justify-content: center;
  }

  /* ─── Responsive ────────────────────────────────────────────────────────── */

  @media (max-width: 1023px) {
    .catalog-layout {
      grid-template-columns: 1fr;
    }

    .filter-toggle {
      display: inline-flex;
    }

    .sidebar {
      position: fixed;
      top: 0;
      left: 0;
      bottom: 0;
      width: 300px;
      max-height: 100vh;
      background-color: var(--color-surface);
      z-index: var(--z-modal);
      transform: translateX(-100%);
      transition: transform var(--transition-base);
      padding: var(--space-6);
      box-shadow: var(--shadow-xl);
    }

    .sidebar.open {
      transform: translateX(0);
    }

    .sidebar-header {
      display: flex;
    }

    .sidebar-backdrop {
      display: block;
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.4);
      z-index: calc(var(--z-modal) - 1);
      border: none;
      cursor: pointer;
    }

    .product-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }

  @media (max-width: 639px) {
    .product-grid {
      grid-template-columns: 1fr;
    }

    .catalog-header h1 {
      font-size: 1.5rem;
    }

    .sort-select {
      min-width: 140px;
    }
  }

  /* ─── Utility overrides for sidebar width ───────────────────────────────── */
  .w-full {
    width: 100%;
  }
</style>
