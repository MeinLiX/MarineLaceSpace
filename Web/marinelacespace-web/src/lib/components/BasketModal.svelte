<script lang="ts">
  import { goto } from '$app/navigation';
  import { basketStore } from '$stores/basket.svelte';
  import PriceDisplay from '$components/PriceDisplay.svelte';
  import { i18n } from '$i18n/index.svelte';

  let { open = false, onclose }: { open: boolean; onclose: () => void } = $props();

  const FREE_SHIPPING_THRESHOLD = 2000;
  const SHIPPING_COST = 150;

  const subtotal = $derived(
    basketStore.basket?.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0) ?? 0
  );
  const shippingCost = $derived(subtotal >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_COST);
  const total = $derived(subtotal + shippingCost);
  const freeShippingRemaining = $derived(
    subtotal < FREE_SHIPPING_THRESHOLD ? FREE_SHIPPING_THRESHOLD - subtotal : 0
  );

  const isEmpty = $derived(!basketStore.basket || basketStore.basket.items.length === 0);

  function handleKeydown(e: KeyboardEvent) {
    if (e.key === 'Escape' && open) {
      onclose();
    }
  }

  function handleBackdropClick() {
    onclose();
  }

  async function handleQuantityChange(itemId: string, currentQty: number, delta: number) {
    const newQty = currentQty + delta;
    if (newQty < 1 || newQty > 99) return;
    try {
      await basketStore.updateItem(itemId, { quantity: newQty });
    } catch {
      // silently fail — notification store could be added
    }
  }

  async function handleRemoveItem(itemId: string) {
    try {
      await basketStore.removeItem(itemId);
    } catch {
      // silently fail
    }
  }

  function goToCheckout() {
    onclose();
    goto('/checkout');
  }

  $effect(() => {
    if (open) {
      basketStore.loadBasket();
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  });
</script>

<svelte:window onkeydown={handleKeydown} />

{#if open}
  <!-- svelte-ignore a11y_no_static_element_interactions -->
  <div class="basket-modal-backdrop" class:visible={open} onclick={handleBackdropClick} onkeydown={() => {}}></div>
{/if}

<div class="basket-modal-panel" class:open aria-label={i18n.t('basket.title')} role="dialog" aria-modal="true">
  <!-- Header -->
  <div class="bm-header">
    <h2 class="bm-title">{i18n.t('basket.title')}</h2>
    <button class="bm-close" onclick={onclose} aria-label={i18n.t('basket.close')}>
      <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <line x1="18" y1="6" x2="6" y2="18" />
        <line x1="6" y1="6" x2="18" y2="18" />
      </svg>
    </button>
  </div>

  <!-- Body -->
  <div class="bm-body">
    {#if basketStore.isLoading && isEmpty}
      <div class="bm-loading">
        <span class="bm-spinner"></span>
        <p>{i18n.t('common.loading')}</p>
      </div>
    {:else if isEmpty}
      <div class="bm-empty">
        <svg class="bm-empty-icon" viewBox="0 0 24 24" width="56" height="56" fill="none" stroke="currentColor" stroke-width="1.2" aria-hidden="true">
          <path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z" />
          <line x1="3" y1="6" x2="21" y2="6" />
          <path d="M16 10a4 4 0 01-8 0" />
        </svg>
        <p class="bm-empty-title">{i18n.t('basket.empty')}</p>
        <p class="bm-empty-desc">{i18n.t('basket.emptyDesc')}</p>
        <button class="bm-continue-btn" onclick={onclose}>{i18n.t('basket.continueShopping')}</button>
      </div>
    {:else}
      <ul class="bm-items">
        {#each basketStore.basket!.items as item (item.itemId)}
          <li class="bm-item">
            <div class="bm-item-image">
              {#if item.imageUrl}
                <img src={item.imageUrl} alt={item.productName} loading="lazy" />
              {:else}
                <div class="bm-item-placeholder">
                  <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
                    <rect x="3" y="3" width="18" height="18" rx="2" />
                    <circle cx="8.5" cy="8.5" r="1.5" />
                    <path d="M21 15l-5-5L5 21" />
                  </svg>
                </div>
              {/if}
            </div>

            <div class="bm-item-details">
              <div class="bm-item-top">
                <a href="/products/{item.productId}" class="bm-item-name" onclick={onclose}>{item.productName}</a>
                <button class="bm-item-remove" onclick={() => handleRemoveItem(item.itemId)} aria-label={i18n.t('basket.removeItem', { name: item.productName })} disabled={basketStore.isLoading}>
                  <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                    <line x1="18" y1="6" x2="6" y2="18" />
                    <line x1="6" y1="6" x2="18" y2="18" />
                  </svg>
                </button>
              </div>

              {#if item.sizeName || item.colorName}
                <p class="bm-item-variant">
                  {#if item.sizeName}{i18n.t('product.size')}: {item.sizeName}{/if}
                  {#if item.sizeName && item.colorName} · {/if}
                  {#if item.colorName}{i18n.t('product.color')}: {item.colorName}{/if}
                </p>
              {/if}

              <div class="bm-item-bottom">
                <div class="bm-qty-controls">
                  <button
                    class="bm-qty-btn"
                    onclick={() => handleQuantityChange(item.itemId, item.quantity, -1)}
                    disabled={item.quantity <= 1 || basketStore.isLoading}
                    aria-label={i18n.t('basket.decreaseQuantity')}
                  >−</button>
                  <span class="bm-qty-value">{item.quantity}</span>
                  <button
                    class="bm-qty-btn"
                    onclick={() => handleQuantityChange(item.itemId, item.quantity, 1)}
                    disabled={item.quantity >= 99 || basketStore.isLoading}
                    aria-label={i18n.t('basket.increaseQuantity')}
                  >+</button>
                </div>
                <PriceDisplay price={item.unitPrice * item.quantity} />
              </div>
            </div>
          </li>
        {/each}
      </ul>
    {/if}
  </div>

  <!-- Footer (only if not empty) -->
  {#if !isEmpty}
    <div class="bm-footer">
      {#if freeShippingRemaining > 0}
        <p class="bm-shipping-hint">
          {@html i18n.t('basket.freeShippingHint', { amount: `<strong>${i18n.t('common.currency')}${freeShippingRemaining.toFixed(2)}</strong>` })}
        </p>
      {:else}
        <p class="bm-shipping-free">✓ {i18n.t('basket.freeShipping')}</p>
      {/if}

      <div class="bm-summary">
        <div class="bm-summary-row">
          <span>{i18n.t('basket.subtotal')}</span>
          <span>{i18n.t('common.currency')}{subtotal.toFixed(2)}</span>
        </div>
        <div class="bm-summary-row">
          <span>{i18n.t('basket.shipping')}</span>
          <span>{shippingCost === 0 ? i18n.t('basket.free') : `${i18n.t('common.currency')}${shippingCost.toFixed(2)}`}</span>
        </div>
        <div class="bm-summary-row bm-total">
          <span>{i18n.t('basket.total')}</span>
          <span>{i18n.t('common.currency')}{total.toFixed(2)}</span>
        </div>
      </div>

      <button class="bm-checkout-btn" onclick={goToCheckout} disabled={basketStore.isLoading}>
        {i18n.t('basket.checkout')}
      </button>

      <button class="bm-continue-link" onclick={onclose}>
        {i18n.t('basket.continueShopping')}
      </button>
    </div>
  {/if}
</div>

<style>
  /* Backdrop */
  .basket-modal-backdrop {
    position: fixed;
    inset: 0;
    background-color: rgba(0, 0, 0, 0.4);
    z-index: calc(var(--z-modal) - 1);
    opacity: 0;
    animation: fadeIn 250ms ease forwards;
  }

  @keyframes fadeIn {
    to { opacity: 1; }
  }

  /* Panel */
  .basket-modal-panel {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    width: 420px;
    max-width: 100%;
    background-color: var(--color-surface);
    z-index: var(--z-modal);
    display: flex;
    flex-direction: column;
    transform: translateX(100%);
    transition: transform 300ms cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: -4px 0 24px rgba(0, 0, 0, 0.12);
  }

  .basket-modal-panel.open {
    transform: translateX(0);
  }

  /* Header */
  .bm-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: var(--space-4) var(--space-5);
    border-bottom: 1px solid var(--color-border);
    flex-shrink: 0;
  }

  .bm-title {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--color-text);
    margin: 0;
  }

  .bm-close {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border: none;
    background: none;
    color: var(--color-text-light);
    cursor: pointer;
    border-radius: var(--radius-full);
    transition: background-color var(--transition-fast), color var(--transition-fast);
  }

  .bm-close:hover {
    background-color: var(--color-surface-hover);
    color: var(--color-text);
  }

  /* Body */
  .bm-body {
    flex: 1;
    overflow-y: auto;
    padding: var(--space-4) var(--space-5);
  }

  /* Loading */
  .bm-loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: var(--space-3);
    padding: var(--space-10) 0;
    color: var(--color-text-muted);
    font-size: 0.875rem;
  }

  .bm-spinner {
    width: 28px;
    height: 28px;
    border: 2px solid var(--color-border);
    border-top-color: var(--color-primary);
    border-radius: 50%;
    animation: spin 0.7s linear infinite;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }

  /* Empty state */
  .bm-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    padding: var(--space-12) var(--space-4);
    gap: var(--space-3);
  }

  .bm-empty-icon {
    color: var(--color-text-muted);
    margin-bottom: var(--space-2);
  }

  .bm-empty-title {
    font-family: var(--font-display);
    font-size: 1.125rem;
    font-weight: 600;
    color: var(--color-text);
    margin: 0;
  }

  .bm-empty-desc {
    font-size: 0.875rem;
    color: var(--color-text-light);
    margin: 0;
  }

  .bm-continue-btn {
    margin-top: var(--space-4);
    padding: var(--space-2) var(--space-6);
    border: 1px solid var(--color-primary);
    background: none;
    color: var(--color-primary);
    font-size: 0.875rem;
    font-weight: 500;
    border-radius: var(--radius-full);
    cursor: pointer;
    transition: background-color var(--transition-fast), color var(--transition-fast);
  }

  .bm-continue-btn:hover {
    background-color: var(--color-primary);
    color: #fff;
  }

  /* Items list */
  .bm-items {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .bm-item {
    display: flex;
    gap: var(--space-3);
    padding-bottom: var(--space-4);
    border-bottom: 1px solid var(--color-border-light);
  }

  .bm-item:last-child {
    border-bottom: none;
    padding-bottom: 0;
  }

  .bm-item-image {
    width: 72px;
    height: 90px;
    border-radius: var(--radius-md);
    overflow: hidden;
    flex-shrink: 0;
    background-color: var(--color-surface-hover);
  }

  .bm-item-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .bm-item-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--color-text-muted);
  }

  .bm-item-details {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .bm-item-top {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: var(--space-2);
  }

  .bm-item-name {
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
    text-decoration: none;
    line-height: 1.4;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    line-clamp: 2;
    -webkit-box-orient: vertical;
    transition: color var(--transition-fast);
  }

  .bm-item-name:hover {
    color: var(--color-primary);
  }

  .bm-item-remove {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border: none;
    background: none;
    color: var(--color-text-muted);
    cursor: pointer;
    flex-shrink: 0;
    border-radius: var(--radius-sm);
    transition: color var(--transition-fast), background-color var(--transition-fast);
  }

  .bm-item-remove:hover {
    color: #c44;
    background-color: rgba(204, 68, 68, 0.08);
  }

  .bm-item-remove:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  .bm-item-variant {
    font-size: 0.75rem;
    color: var(--color-text-muted);
    margin: 0;
  }

  .bm-item-bottom {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: auto;
    padding-top: var(--space-1);
  }

  /* Quantity controls */
  .bm-qty-controls {
    display: flex;
    align-items: center;
    gap: 0;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-sm);
    overflow: hidden;
  }

  .bm-qty-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border: none;
    background: none;
    color: var(--color-text);
    cursor: pointer;
    font-size: 0.875rem;
    transition: background-color var(--transition-fast);
  }

  .bm-qty-btn:hover:not(:disabled) {
    background-color: var(--color-surface-hover);
  }

  .bm-qty-btn:disabled {
    color: var(--color-text-muted);
    cursor: not-allowed;
  }

  .bm-qty-value {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 28px;
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text);
    border-left: 1px solid var(--color-border);
    border-right: 1px solid var(--color-border);
  }

  /* Footer */
  .bm-footer {
    border-top: 1px solid var(--color-border);
    padding: var(--space-4) var(--space-5);
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .bm-shipping-hint {
    font-size: 0.8125rem;
    color: var(--color-text-light);
    text-align: center;
    margin: 0;
    padding: var(--space-2) var(--space-3);
    background-color: var(--color-surface-hover);
    border-radius: var(--radius-sm);
  }

  .bm-shipping-free {
    font-size: 0.8125rem;
    color: var(--color-success);
    text-align: center;
    font-weight: 500;
    margin: 0;
    padding: var(--space-2) var(--space-3);
    background-color: rgba(74, 139, 110, 0.08);
    border-radius: var(--radius-sm);
  }

  .bm-summary {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .bm-summary-row {
    display: flex;
    justify-content: space-between;
    font-size: 0.8125rem;
    color: var(--color-text-light);
  }

  .bm-summary-row.bm-total {
    font-size: 0.9375rem;
    font-weight: 600;
    color: var(--color-text);
    padding-top: var(--space-2);
    border-top: 1px solid var(--color-border-light);
  }

  .bm-checkout-btn {
    width: 100%;
    padding: var(--space-3) var(--space-4);
    background-color: var(--color-primary);
    color: #fff;
    border: none;
    border-radius: var(--radius-full);
    font-size: 0.9375rem;
    font-weight: 600;
    cursor: pointer;
    transition: background-color var(--transition-fast);
  }

  .bm-checkout-btn:hover:not(:disabled) {
    background-color: var(--color-primary-dark);
  }

  .bm-checkout-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  .bm-continue-link {
    background: none;
    border: none;
    color: var(--color-text-light);
    font-size: 0.8125rem;
    text-decoration: underline;
    cursor: pointer;
    text-align: center;
    padding: var(--space-1) 0;
    transition: color var(--transition-fast);
  }

  .bm-continue-link:hover {
    color: var(--color-primary);
  }

  /* Mobile */
  @media (max-width: 480px) {
    .basket-modal-panel {
      width: 100%;
    }
  }
</style>
