<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let {
    price,
    oldPrice = undefined,
    discountPercentage = undefined,
    currency = '₴'
  }: {
    price: number;
    oldPrice?: number;
    discountPercentage?: number;
    currency?: string;
  } = $props();

  let computedOldPrice = $derived(
    oldPrice ??
    (discountPercentage != null && discountPercentage > 0
      ? price / (1 - discountPercentage / 100)
      : undefined)
  );

  let hasDiscount = $derived(computedOldPrice != null && computedOldPrice > price);

  let discountBadgeText = $derived(
    discountPercentage ??
    (hasDiscount && computedOldPrice != null
      ? Math.round(((computedOldPrice - price) / computedOldPrice) * 100)
      : undefined)
  );

  function formatPrice(value: number): string {
    return `${currency}${value.toFixed(2)}`;
  }
</script>

<span class="price-display" aria-label={i18n.t('product.priceLabel', { price: formatPrice(price), oldPrice: hasDiscount && computedOldPrice ? formatPrice(computedOldPrice) : '' })}>
  <span class="current-price" class:discounted={hasDiscount}>
    {formatPrice(price)}
  </span>
  {#if hasDiscount && computedOldPrice}
    <span class="old-price">{formatPrice(computedOldPrice)}</span>
  {/if}
  {#if hasDiscount && discountBadgeText}
    <span class="discount-tag">-{discountBadgeText}%</span>
  {/if}
</span>

<style>
  .price-display {
    display: inline-flex;
    align-items: baseline;
    gap: var(--space-2);
    flex-wrap: wrap;
  }

  .current-price {
    font-size: 1rem;
    font-weight: 700;
    color: var(--color-text);
  }

  .current-price.discounted {
    color: var(--color-error);
  }

  .old-price {
    font-size: 0.8125rem;
    font-weight: 400;
    color: var(--color-text-muted);
    text-decoration: line-through;
  }

  .discount-tag {
    display: inline-flex;
    align-items: center;
    padding: 1px var(--space-2);
    font-size: 0.6875rem;
    font-weight: 700;
    color: #FFFFFF;
    background-color: var(--color-error);
    border-radius: var(--radius-sm);
    line-height: 1.4;
    letter-spacing: 0.02em;
  }
</style>
