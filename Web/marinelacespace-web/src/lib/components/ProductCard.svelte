<script lang="ts">
  import ReviewStars from './ReviewStars.svelte';
  import PriceDisplay from './PriceDisplay.svelte';

  interface Product {
    id: string;
    title: string;
    shopName?: string;
    imageUrl?: string;
    imageAlt?: string;
    basePrice: number;
    oldPrice?: number;
    minPrice?: number;
    maxPrice?: number;
    discountPercentage?: number;
    rating?: number;
    reviewCount?: number;
    currency?: string;
  }

  let {
    product,
    compact = false
  }: {
    product: Product;
    compact?: boolean;
  } = $props();

  let hasDiscount = $derived(
    (product.oldPrice != null && product.oldPrice > product.basePrice) ||
    (product.discountPercentage != null && product.discountPercentage > 0)
  );

  let discountPercent = $derived(
    product.discountPercentage ??
    (product.oldPrice != null && product.oldPrice > product.basePrice
      ? Math.round(((product.oldPrice - product.basePrice) / product.oldPrice) * 100)
      : undefined)
  );

  let hasPriceRange = $derived(
    product.minPrice != null && product.maxPrice != null && product.minPrice !== product.maxPrice
  );
</script>

<a
  href="/products/{product.id}"
  class="product-card"
  class:compact
  aria-label="View {product.title}"
>
  <div class="image-wrapper">
    {#if product.imageUrl}
      <img
        src={product.imageUrl}
        alt={product.imageAlt ?? product.title}
        class="product-image"
        loading="lazy"
      />
    {:else}
      <div class="image-placeholder" aria-hidden="true">
        <svg viewBox="0 0 24 24" width="40" height="40" fill="none" stroke="currentColor" stroke-width="1" opacity="0.3">
          <rect x="3" y="3" width="18" height="18" rx="2" />
          <circle cx="8.5" cy="8.5" r="1.5" />
          <polyline points="21 15 16 10 5 21" />
        </svg>
      </div>
    {/if}
    {#if hasDiscount && discountPercent}
      <span class="discount-badge">-{discountPercent}%</span>
    {/if}
  </div>

  <div class="product-info">
    {#if product.shopName && !compact}
      <span class="shop-name">{product.shopName}</span>
    {/if}

    <h3 class="product-title" class:compact>{product.title}</h3>

    {#if product.rating != null && !compact}
      <div class="product-rating">
        <ReviewStars rating={product.rating} count={product.reviewCount} size="sm" />
      </div>
    {/if}

    <div class="product-price">
      {#if hasPriceRange}
        <span class="price-range">
          {product.currency ?? '₴'}{product.minPrice?.toFixed(2)} — {product.currency ?? '₴'}{product.maxPrice?.toFixed(2)}
        </span>
      {:else}
        <PriceDisplay
          price={product.basePrice}
          oldPrice={product.oldPrice}
          discountPercentage={product.discountPercentage}
          currency={product.currency}
        />
      {/if}
    </div>
  </div>
</a>

<style>
  .product-card {
    display: flex;
    flex-direction: column;
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    overflow: hidden;
    text-decoration: none;
    transition: box-shadow var(--transition-base), transform var(--transition-base);
  }

  .product-card:hover {
    box-shadow: var(--shadow-lg);
    transform: translateY(-2px);
  }

  .product-card:focus-visible {
    outline: 2px solid var(--color-primary);
    outline-offset: 2px;
  }

  .image-wrapper {
    position: relative;
    overflow: hidden;
    aspect-ratio: 3 / 4;
    background-color: var(--color-surface-hover);
  }

  .compact .image-wrapper {
    aspect-ratio: 1;
  }

  .product-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform var(--transition-slow);
  }

  .product-card:hover .product-image {
    transform: scale(1.05);
  }

  .image-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: var(--color-surface-hover);
  }

  .discount-badge {
    position: absolute;
    top: var(--space-3);
    left: var(--space-3);
    padding: var(--space-1) var(--space-2);
    font-size: 0.6875rem;
    font-weight: 700;
    color: #FFFFFF;
    background-color: var(--color-error);
    border-radius: var(--radius-sm);
    letter-spacing: 0.02em;
  }

  .product-info {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
    padding: var(--space-4);
  }

  .compact .product-info {
    padding: var(--space-3);
  }

  .shop-name {
    font-size: 0.75rem;
    color: var(--color-text-muted);
    text-transform: uppercase;
    letter-spacing: 0.04em;
    font-weight: 500;
  }

  .product-title {
    font-family: var(--font-body);
    font-size: 0.9375rem;
    font-weight: 500;
    color: var(--color-text);
    line-height: 1.4;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  .product-title.compact {
    font-size: 0.8125rem;
    -webkit-line-clamp: 1;
  }

  .product-rating {
    margin-top: var(--space-1);
  }

  .product-price {
    margin-top: var(--space-2);
  }

  .price-range {
    font-size: 0.9375rem;
    font-weight: 600;
    color: var(--color-text);
  }
</style>
