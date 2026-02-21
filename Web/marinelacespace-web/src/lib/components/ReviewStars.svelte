<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let {
    rating,
    count = undefined,
    size = 'md'
  }: {
    rating: number;
    count?: number;
    size?: 'sm' | 'md' | 'lg';
  } = $props();

  let stars = $derived(
    Array.from({ length: 5 }, (_, i) => {
      const starIndex = i + 1;
      if (rating >= starIndex) return 'full';
      if (rating >= starIndex - 0.5) return 'half';
      return 'empty';
    })
  );
</script>

<div class="review-stars {size}" aria-label={i18n.t('product.ratingLabel', { rating, count: count ?? 0 })}>
  <div class="stars" aria-hidden="true">
    {#each stars as star, i (i)}
      <span class="star {star}">★</span>
    {/each}
  </div>
  {#if count != null}
    <span class="count">({i18n.t('product.reviewCount', { count })})</span>
  {/if}
</div>

<style>
  .review-stars {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
  }

  .stars {
    display: flex;
    gap: 1px;
  }

  .star {
    line-height: 1;
    transition: color var(--transition-fast);
  }

  .star.full {
    color: var(--color-secondary);
  }

  .star.half {
    color: var(--color-secondary);
    opacity: 0.55;
  }

  .star.empty {
    color: var(--color-border);
  }

  /* Sizes */
  .sm .star {
    font-size: 0.75rem;
  }

  .sm .count {
    font-size: 0.6875rem;
  }

  .md .star {
    font-size: 1rem;
  }

  .md .count {
    font-size: 0.8125rem;
  }

  .lg .star {
    font-size: 1.375rem;
  }

  .lg .count {
    font-size: 0.9375rem;
  }

  .count {
    color: var(--color-text-muted);
    white-space: nowrap;
  }
</style>
