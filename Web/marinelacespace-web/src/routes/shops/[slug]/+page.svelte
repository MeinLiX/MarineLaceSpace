<script lang="ts">
  import { page } from '$app/stores';
  import { i18n } from '$i18n/index.svelte';
  import ProductCard from '$components/ProductCard.svelte';
  import ReviewStars from '$components/ReviewStars.svelte';
  import Pagination from '$components/Pagination.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import { getShopBySlug, getProductsByShop, getProductReviews } from '$api/catalog';
  import type { Shop, Product, ProductReview, PaginatedResponse } from '$types';

  // ─── State ─────────────────────────────────────────────────────────────────

  let isLoading = $state(true);
  let shop = $state<Shop | null>(null);
  let products = $state<Product[]>([]);
  let productPage = $state(1);
  let productTotalPages = $state(1);
  let shopReviews = $state<ProductReview[]>([]);
  let reviewPage = $state(1);
  let reviewTotalPages = $state(1);
  let isProductsLoading = $state(false);
  let isReviewsLoading = $state(false);

  let activeTab = $state<'products' | 'reviews'>('products');

  // ─── Data Loading ──────────────────────────────────────────────────────────

  async function loadShop(slug: string) {
    isLoading = true;
    try {
      shop = await getShopBySlug(slug);
      await loadProducts();
    } catch (err) {
      console.error('Failed to load shop', err);
      shop = null;
    } finally {
      isLoading = false;
    }
  }

  async function loadProducts() {
    if (!shop) return;
    isProductsLoading = true;
    try {
      const result: PaginatedResponse<Product> = await getProductsByShop(shop.id, {
        page: productPage,
        pageSize: 12,
      });
      products = result.items;
      productTotalPages = result.totalPages;
    } catch {
      products = [];
    } finally {
      isProductsLoading = false;
    }
  }

  async function loadReviews() {
    if (!shop) return;
    isReviewsLoading = true;
    try {
      const result: PaginatedResponse<ProductReview> = await getProductReviews(shop.id, {
        page: reviewPage,
        pageSize: 10,
      });
      shopReviews = result.items;
      reviewTotalPages = result.totalPages;
    } catch {
      shopReviews = [];
    } finally {
      isReviewsLoading = false;
    }
  }

  function handleProductPageChange(p: number) {
    productPage = p;
    loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function handleReviewPageChange(p: number) {
    reviewPage = p;
    loadReviews();
  }

  function switchTab(tab: 'products' | 'reviews') {
    activeTab = tab;
    if (tab === 'reviews' && shopReviews.length === 0) {
      loadReviews();
    }
  }

  function mapProductToCard(p: Product) {
    return {
      id: p.id,
      title: p.name,
      shopName: p.shopName,
      imageUrl: p.mainImageUrl ?? undefined,
      basePrice: p.price,
    };
  }

  function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('uk-UA', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  // ─── Lifecycle ─────────────────────────────────────────────────────────────

  $effect(() => {
    const slug = $page.params.slug;
    if (slug) loadShop(slug);
  });
</script>

<svelte:head>
  <title>{shop?.name ?? i18n.t('shops.shop')} — MarineLaceSpace</title>
</svelte:head>

{#if isLoading}
  <div class="container shop-loading">
    <LoadingSpinner size="lg" message={i18n.t('shops.loadingShop')} />
  </div>
{:else if shop}
  <div class="shop-page">
    <div class="container">
      <!-- ─── Shop Header / Banner ─────────────────────────────────────────── -->
      <header class="shop-header">
        {#if shop.bannerUrl}
          <div class="shop-banner">
            <img src={shop.bannerUrl} alt={i18n.t('shops.shopBanner', { name: shop.name })} />
          </div>
        {/if}
        <div class="shop-identity">
          <div class="shop-avatar">
            {#if shop.logoUrl}
              <img src={shop.logoUrl} alt={i18n.t('shops.shopLogo', { name: shop.name })} class="avatar-image" />
            {:else}
              <div class="avatar-placeholder" aria-hidden="true">🏪</div>
            {/if}
          </div>
          <div class="shop-details">
            <h1 class="shop-name">{shop.name}</h1>
            {#if shop.description}
              <p class="shop-description">{shop.description}</p>
            {/if}
            <div class="shop-stats">
              <span class="shop-stat">
                {i18n.t('shops.products', { count: shop.productCount })}
              </span>
            </div>
          </div>
        </div>
      </header>

      <!-- ─── Tabs ─────────────────────────────────────────────────────────── -->
      <div class="tabs" role="tablist" aria-label={i18n.t('shops.shopSections')}>
        <button
          class="tab"
          class:active={activeTab === 'products'}
          role="tab"
          aria-selected={activeTab === 'products'}
          onclick={() => switchTab('products')}
        >
          {i18n.t('shops.allProducts')}
        </button>
        <button
          class="tab"
          class:active={activeTab === 'reviews'}
          role="tab"
          aria-selected={activeTab === 'reviews'}
          onclick={() => switchTab('reviews')}
        >
          {i18n.t('shops.reviews')}
        </button>
      </div>

      <!-- ─── Products Tab ─────────────────────────────────────────────────── -->
      {#if activeTab === 'products'}
        <section aria-label={i18n.t('shops.shopProducts')}>
          {#if isProductsLoading}
            <LoadingSpinner message={i18n.t('shops.loadingProducts')} />
          {:else if products.length === 0}
            <EmptyState
              title={i18n.t('shops.noProducts')}
              description={i18n.t('shops.noProductsDescription')}
              icon="📦"
            />
          {:else}
            <div class="products-grid">
              {#each products as product (product.id)}
                <ProductCard product={mapProductToCard(product)} />
              {/each}
            </div>

            {#if productTotalPages > 1}
              <div class="pagination-wrapper">
                <Pagination
                  currentPage={productPage}
                  totalPages={productTotalPages}
                  onPageChange={handleProductPageChange}
                />
              </div>
            {/if}
          {/if}
        </section>
      {/if}

      <!-- ─── Reviews Tab ──────────────────────────────────────────────────── -->
      {#if activeTab === 'reviews'}
        <section aria-label={i18n.t('shops.shopReviews')}>
          {#if isReviewsLoading}
            <LoadingSpinner message={i18n.t('shops.loadingReviews')} />
          {:else if shopReviews.length === 0}
            <EmptyState
              title={i18n.t('shops.noReviews')}
              description={i18n.t('shops.noReviewsDescription')}
              icon="💬"
            />
          {:else}
            <ul class="review-list">
              {#each shopReviews as review (review.id)}
                <li class="review-item card">
                  <div class="card-body">
                    <div class="review-header">
                      <ReviewStars rating={review.rating} size="sm" />
                      {#if review.isVerifiedPurchase}
                        <span class="badge badge-success">{i18n.t('shops.verifiedPurchase')}</span>
                      {/if}
                    </div>
                    {#if review.title}
                      <h4 class="review-title">{review.title}</h4>
                    {/if}
                    <p class="review-text">{review.text}</p>
                    <div class="review-footer">
                      <span class="text-muted text-sm">
                        {review.guestName ?? i18n.t('shops.customer')} • {formatDate(review.createdAt)}
                      </span>
                    </div>
                  </div>
                </li>
              {/each}
            </ul>

            {#if reviewTotalPages > 1}
              <div class="pagination-wrapper">
                <Pagination
                  currentPage={reviewPage}
                  totalPages={reviewTotalPages}
                  onPageChange={handleReviewPageChange}
                />
              </div>
            {/if}
          {/if}
        </section>
      {/if}
    </div>
  </div>
{:else}
  <div class="container shop-loading">
    <EmptyState
      title={i18n.t('shops.shopNotFound')}
      description={i18n.t('shops.shopNotFoundDescription')}
      icon="🔍"
      actionLabel={i18n.t('shops.goToShops')}
      actionHref="/shops"
    />
  </div>
{/if}

<style>
  .shop-loading {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
  }

  .shop-page {
    padding-block: var(--space-4) var(--space-16);
  }

  /* ─── Shop Header ─────────────────────────────────────────────────────── */

  .shop-header {
    margin-bottom: var(--space-8);
  }

  .shop-banner {
    width: 100%;
    height: 200px;
    border-radius: var(--radius-lg);
    overflow: hidden;
    margin-bottom: var(--space-6);
    background-color: var(--color-surface-hover);
  }

  .shop-banner img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .shop-identity {
    display: flex;
    gap: var(--space-6);
    align-items: flex-start;
  }

  .shop-avatar {
    width: 80px;
    height: 80px;
    border-radius: var(--radius-lg);
    overflow: hidden;
    flex-shrink: 0;
    background-color: var(--color-surface-hover);
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px solid var(--color-border-light);
  }

  .avatar-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .avatar-placeholder {
    font-size: 2rem;
    opacity: 0.5;
  }

  .shop-details {
    flex: 1;
  }

  .shop-name {
    font-family: var(--font-display);
    font-size: 2rem;
    font-weight: 700;
    margin-bottom: var(--space-2);
  }

  .shop-description {
    font-size: 0.9375rem;
    color: var(--color-text-light);
    line-height: 1.6;
    margin-bottom: var(--space-3);
    max-width: 600px;
  }

  .shop-stats {
    display: flex;
    align-items: center;
    gap: var(--space-6);
    flex-wrap: wrap;
  }

  .shop-stat {
    font-size: 0.875rem;
    color: var(--color-text-light);
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
  }

  /* ─── Tabs ────────────────────────────────────────────────────────────── */

  .tabs {
    display: flex;
    gap: 0;
    border-bottom: 2px solid var(--color-border-light);
    margin-bottom: var(--space-8);
  }

  .tab {
    padding: var(--space-3) var(--space-6);
    font-size: 0.9375rem;
    font-weight: 500;
    color: var(--color-text-light);
    background: none;
    border: none;
    border-bottom: 2px solid transparent;
    margin-bottom: -2px;
    cursor: pointer;
    transition: color var(--transition-fast), border-color var(--transition-fast);
  }

  .tab:hover {
    color: var(--color-primary);
  }

  .tab.active {
    color: var(--color-primary);
    border-bottom-color: var(--color-primary);
    font-weight: 600;
  }

  /* ─── Products Grid ───────────────────────────────────────────────────── */

  .products-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-6);
  }

  .pagination-wrapper {
    margin-top: var(--space-8);
    display: flex;
    justify-content: center;
  }

  /* ─── Reviews ─────────────────────────────────────────────────────────── */

  .review-list {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    list-style: none;
    padding: 0;
  }

  .review-item {
    transition: none;
  }

  .review-item:hover {
    box-shadow: none;
    transform: none;
  }

  .review-header {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-bottom: var(--space-2);
  }

  .review-title {
    font-size: 1rem;
    font-weight: 600;
    margin-bottom: var(--space-1);
  }

  .review-text {
    font-size: 0.9375rem;
    line-height: 1.6;
    color: var(--color-text);
    margin-bottom: var(--space-2);
  }

  .review-footer {
    display: flex;
    gap: var(--space-2);
  }

  /* ─── Responsive ────────────────────────────────────────────────────────── */

  @media (max-width: 1023px) {
    .products-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }

  @media (max-width: 639px) {
    .products-grid {
      grid-template-columns: 1fr;
    }

    .shop-identity {
      flex-direction: column;
      align-items: center;
      text-align: center;
    }

    .shop-description {
      max-width: 100%;
    }

    .shop-stats {
      justify-content: center;
    }

    .shop-name {
      font-size: 1.5rem;
    }

    .shop-banner {
      height: 140px;
    }

    .tab {
      flex: 1;
      text-align: center;
      padding-inline: var(--space-3);
    }
  }
</style>
