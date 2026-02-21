<script lang="ts">
  import { goto } from '$app/navigation';
  import type { BasketItem } from '$types';
  import { basketStore } from '$stores/basket.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import PriceDisplay from '$components/PriceDisplay.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';

  let showClearModal = $state(false);
  let itemToRemove = $state<BasketItem | null>(null);

  const itemCountLabel = $derived(() => {
    const count = basketStore.itemCount;
    if (count === 0) return '';
    return `(${i18n.t('basket.itemCount', { count })})`;
  });

  const subtotal = $derived(
    basketStore.basket?.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0) ?? 0
  );

  const FREE_SHIPPING_THRESHOLD = 2000;
  const SHIPPING_COST = 150;

  const shippingCost = $derived(subtotal >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_COST);
  const total = $derived(subtotal + shippingCost);
  const freeShippingRemaining = $derived(
    subtotal < FREE_SHIPPING_THRESHOLD ? FREE_SHIPPING_THRESHOLD - subtotal : 0
  );

  $effect(() => {
    basketStore.loadBasket();
  });

  async function handleQuantityChange(item: BasketItem, delta: number) {
    const newQty = item.quantity + delta;
    if (newQty < 1 || newQty > 99) return;
    try {
      await basketStore.updateItem(item.itemId, { quantity: newQty, personalization: item.personalization ?? undefined });
    } catch {
      notificationStore.error(i18n.t('basket.updateError'));
    }
  }

  async function confirmRemoveItem() {
    if (!itemToRemove) return;
    try {
      await basketStore.removeItem(itemToRemove.itemId);
      notificationStore.success(i18n.t('basket.itemRemoved'));
    } catch {
      notificationStore.error(i18n.t('basket.removeError'));
    } finally {
      itemToRemove = null;
    }
  }

  async function confirmClearBasket() {
    try {
      await basketStore.clearBasket();
      notificationStore.success(i18n.t('basket.cleared'));
    } catch {
      notificationStore.error(i18n.t('basket.clearError'));
    } finally {
      showClearModal = false;
    }
  }

  function proceedToCheckout() {
    goto('/checkout');
  }
</script>

<svelte:head>
  <title>{i18n.t('basket.title')} — MarineLaceSpace</title>
</svelte:head>

<section class="basket-page container" aria-label={i18n.t('basket.title')}>
  {#if basketStore.isLoading && !basketStore.basket}
    <LoadingSpinner size="lg" message={i18n.t('basket.loading')} />
  {:else if !basketStore.basket || basketStore.basket.items.length === 0}
    <EmptyState
      icon="🛒"
      title={i18n.t('basket.empty')}
      description={i18n.t('basket.emptyDescription')}
      actionLabel={i18n.t('basket.continueShopping')}
      actionHref="/catalog"
    />
  {:else}
    <header class="basket-header">
      <div class="basket-title-row">
        <h1 class="basket-title">{i18n.t('basket.title')} <span class="item-count">{itemCountLabel()}</span></h1>
        <button
          class="clear-basket-link"
          onclick={() => (showClearModal = true)}
          aria-label={i18n.t('basket.clear')}
        >
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <polyline points="3 6 5 6 21 6" />
            <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2" />
          </svg>
          {i18n.t('basket.clear')}
        </button>
      </div>
    </header>

    <div class="basket-layout">
      <!-- Items Column -->
      <div class="basket-items" role="list" aria-label={i18n.t('basket.itemsInBasket')}>
        {#each basketStore.basket.items as item (item.itemId)}
          <article class="basket-item card" role="listitem">
            <div class="item-image-wrap">
              {#if item.imageUrl}
                <img
                  src={item.imageUrl}
                  alt={item.productName}
                  class="item-image"
                  width="80"
                  height="80"
                  loading="lazy"
                />
              {:else}
                <div class="item-image-placeholder" aria-hidden="true">📷</div>
              {/if}
            </div>

            <div class="item-details">
              <div class="item-info">
                <a href="/products/{item.productId}" class="item-name">{item.productName}</a>

                <div class="item-variants">
                  {#if item.sizeName}
                    <span class="badge badge-outline">{item.sizeName}</span>
                  {/if}
                  {#if item.colorName}
                    <span class="badge badge-outline color-badge">
                      {#if item.colorId}
                        <span class="color-swatch" style="background-color: var(--swatch-color, #ccc);" aria-hidden="true"></span>
                      {/if}
                      {item.colorName}
                    </span>
                  {/if}
                  {#if item.materialName}
                    <span class="badge badge-outline">{item.materialName}</span>
                  {/if}
                </div>

                {#if item.personalization}
                  <p class="item-personalization">✨ {item.personalization}</p>
                {/if}
              </div>

              <div class="item-actions-row">
                <div class="item-price-unit">
                  <PriceDisplay price={item.unitPrice} />
                </div>

                <div class="quantity-control" role="group" aria-label={i18n.t('basket.quantityFor', { name: item.productName })}>
                  <button
                    class="qty-btn"
                    onclick={() => handleQuantityChange(item, -1)}
                    disabled={item.quantity <= 1 || basketStore.isLoading}
                    aria-label={i18n.t('basket.decreaseQuantity')}
                  >
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                      <line x1="5" y1="12" x2="19" y2="12" />
                    </svg>
                  </button>
                  <span class="qty-value" aria-live="polite">{item.quantity}</span>
                  <button
                    class="qty-btn"
                    onclick={() => handleQuantityChange(item, 1)}
                    disabled={item.quantity >= 99 || basketStore.isLoading}
                    aria-label={i18n.t('basket.increaseQuantity')}
                  >
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                      <line x1="12" y1="5" x2="12" y2="19" />
                      <line x1="5" y1="12" x2="19" y2="12" />
                    </svg>
                  </button>
                </div>

                <div class="item-line-total">
                  <PriceDisplay price={item.unitPrice * item.quantity} />
                </div>

                <button
                  class="remove-btn"
                  onclick={() => (itemToRemove = item)}
                  aria-label={i18n.t('basket.removeItem', { name: item.productName })}
                >
                  <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                    <polyline points="3 6 5 6 21 6" />
                    <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2" />
                    <line x1="10" y1="11" x2="10" y2="17" />
                    <line x1="14" y1="11" x2="14" y2="17" />
                  </svg>
                </button>
              </div>
            </div>
          </article>
        {/each}
      </div>

      <!-- Order Summary -->
      <aside class="order-summary" aria-label={i18n.t('basket.orderSummary')}>
        <div class="summary-card card">
          <div class="card-body">
            <h2 class="summary-heading">{i18n.t('basket.orderSummary')}</h2>

            <dl class="summary-lines">
              <div class="summary-row">
                <dt>{i18n.t('basket.items', { count: basketStore.itemCount })}</dt>
                <dd>{i18n.t('common.currency', { amount: subtotal.toFixed(2) })}</dd>
              </div>
              <div class="summary-row">
                <dt>{i18n.t('basket.shipping')}</dt>
                <dd>
                  {#if shippingCost === 0}
                    <span class="free-shipping">{i18n.t('basket.shippingFree')}</span>
                  {:else}
                    {i18n.t('common.currency', { amount: shippingCost.toFixed(2) })}
                  {/if}
                </dd>
              </div>
            </dl>

            {#if freeShippingRemaining > 0}
              <p class="shipping-hint">
                🚚 {i18n.t('basket.freeShippingHint', { threshold: FREE_SHIPPING_THRESHOLD, remaining: freeShippingRemaining.toFixed(2) })}
              </p>
            {/if}

            <hr class="divider" />

            <div class="summary-total">
              <span>{i18n.t('basket.total')}</span>
              <span class="total-price">{i18n.t('common.currency', { amount: total.toFixed(2) })}</span>
            </div>

            <div class="summary-actions">
              <button
                class="btn btn-primary btn-lg w-full"
                onclick={proceedToCheckout}
                disabled={basketStore.isLoading}
              >
                {i18n.t('basket.checkout')}
              </button>
              <a href="/catalog" class="btn btn-outline w-full">
                {i18n.t('basket.continueShopping')}
              </a>
            </div>

            <p class="summary-trust">{i18n.t('basket.trustBadge')}</p>
          </div>
        </div>
      </aside>
    </div>

    <!-- Mobile sticky bar -->
    <div class="mobile-sticky-bar">
      <div class="mobile-sticky-inner">
        <div class="mobile-sticky-total">
          <span class="mobile-total-label">{i18n.t('basket.total')}:</span>
          <span class="mobile-total-price">{i18n.t('common.currency', { amount: total.toFixed(2) })}</span>
        </div>
        <button
          class="btn btn-primary btn-lg mobile-checkout-btn"
          onclick={proceedToCheckout}
          disabled={basketStore.isLoading}
        >
          {i18n.t('basket.checkoutShort')}
        </button>
      </div>
    </div>
  {/if}

  <!-- Remove item confirm modal -->
  <Modal open={itemToRemove !== null} title={i18n.t('basket.removeConfirmTitle')} onclose={() => (itemToRemove = null)}>
    {#if itemToRemove}
      <p>{i18n.t('basket.removeConfirmMessage', { name: itemToRemove.productName })}</p>
      <div class="modal-actions">
        <button class="btn btn-outline" onclick={() => (itemToRemove = null)}>{i18n.t('common.cancel')}</button>
        <button class="btn btn-primary" onclick={confirmRemoveItem} disabled={basketStore.isLoading}>{i18n.t('basket.remove')}</button>
      </div>
    {/if}
  </Modal>

  <!-- Clear basket confirm modal -->
  <Modal open={showClearModal} title={i18n.t('basket.clearConfirmTitle')} onclose={() => (showClearModal = false)}>
    <p>{i18n.t('basket.clearConfirmMessage')}</p>
    <div class="modal-actions">
      <button class="btn btn-outline" onclick={() => (showClearModal = false)}>{i18n.t('common.cancel')}</button>
      <button class="btn btn-primary" onclick={confirmClearBasket} disabled={basketStore.isLoading}>{i18n.t('basket.clear')}</button>
    </div>
  </Modal>
</section>

<style>
  /* ── Page Layout ── */
  .basket-page {
    padding-block: var(--space-4) var(--space-12);
  }

  .basket-header {
    margin-bottom: var(--space-6);
  }

  .basket-title-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .basket-title {
    font-family: var(--font-display);
    font-size: 1.75rem;
    font-weight: 600;
  }

  .item-count {
    font-family: var(--font-body);
    font-size: 1rem;
    font-weight: 400;
    color: var(--color-text-light);
  }

  .clear-basket-link {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    font-size: 0.8125rem;
    color: var(--color-text-light);
    transition: color var(--transition-fast);
    cursor: pointer;
  }

  .clear-basket-link:hover {
    color: var(--color-error);
  }

  .basket-layout {
    display: flex;
    flex-direction: column;
    gap: var(--space-8);
  }

  /* ── Basket Items ── */
  .basket-items {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .basket-item {
    display: flex;
    gap: var(--space-4);
    padding: var(--space-4);
    transition: box-shadow var(--transition-base);
  }

  .basket-item:hover {
    box-shadow: var(--shadow-md);
  }

  .item-image-wrap {
    flex-shrink: 0;
  }

  .item-image {
    width: 80px;
    height: 80px;
    object-fit: cover;
    border-radius: var(--radius-md);
    background-color: var(--color-surface-hover);
  }

  .item-image-placeholder {
    width: 80px;
    height: 80px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    background-color: var(--color-surface-hover);
    border-radius: var(--radius-md);
  }

  .item-details {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    min-width: 0;
  }

  .item-name {
    font-family: var(--font-display);
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text);
    text-decoration: none;
    line-height: 1.3;
    transition: color var(--transition-fast);
  }

  .item-name:hover {
    color: var(--color-primary);
  }

  .item-variants {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
    margin-top: var(--space-1);
  }

  .color-badge {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
  }

  .color-swatch {
    width: 10px;
    height: 10px;
    border-radius: var(--radius-full);
    border: 1px solid var(--color-border);
    display: inline-block;
  }

  .item-personalization {
    font-size: 0.8125rem;
    font-style: italic;
    color: var(--color-text-light);
  }

  .item-actions-row {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    flex-wrap: wrap;
    margin-top: auto;
  }

  .item-price-unit {
    font-size: 0.875rem;
    color: var(--color-text-light);
  }

  /* ── Quantity Control ── */
  .quantity-control {
    display: inline-flex;
    align-items: center;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    overflow: hidden;
  }

  .qty-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    background: var(--color-surface);
    border: none;
    cursor: pointer;
    color: var(--color-text);
    transition: background-color var(--transition-fast);
  }

  .qty-btn:hover:not(:disabled) {
    background-color: var(--color-surface-hover);
  }

  .qty-btn:disabled {
    opacity: 0.35;
    cursor: not-allowed;
  }

  .qty-value {
    min-width: 36px;
    text-align: center;
    font-size: 0.875rem;
    font-weight: 600;
    user-select: none;
  }

  .item-line-total {
    margin-left: auto;
    font-weight: 700;
    font-size: 1rem;
    white-space: nowrap;
  }

  .remove-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: var(--radius-full);
    color: var(--color-text-muted);
    transition: color var(--transition-fast), background-color var(--transition-fast);
    cursor: pointer;
    flex-shrink: 0;
  }

  .remove-btn:hover {
    color: var(--color-error);
    background-color: rgba(196, 85, 90, 0.08);
  }

  /* ── Order Summary ── */
  .order-summary {
    width: 100%;
  }

  .summary-card {
    position: static;
  }

  .summary-heading {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    margin-bottom: var(--space-5);
  }

  .summary-lines {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .summary-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.9375rem;
  }

  .summary-row dt {
    color: var(--color-text-light);
  }

  .summary-row dd {
    font-weight: 500;
  }

  .free-shipping {
    color: var(--color-success);
    font-weight: 600;
  }

  .shipping-hint {
    margin-top: var(--space-3);
    font-size: 0.8125rem;
    color: var(--color-text-light);
    background-color: var(--color-surface-hover);
    border-radius: var(--radius-md);
    padding: var(--space-2) var(--space-3);
  }

  .summary-total {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 1.125rem;
    font-weight: 700;
  }

  .total-price {
    font-size: 1.5rem;
    color: var(--color-primary);
    font-family: var(--font-display);
  }

  .summary-actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }

  .summary-trust {
    margin-top: var(--space-4);
    text-align: center;
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  /* ── Mobile Sticky Bar ── */
  .mobile-sticky-bar {
    display: block;
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: var(--z-sticky);
    background-color: var(--color-surface);
    border-top: 1px solid var(--color-border);
    box-shadow: 0 -4px 12px rgba(0, 0, 0, 0.08);
    padding: var(--space-3) var(--space-4);
  }

  .mobile-sticky-inner {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-4);
    max-width: var(--container-xl);
    margin-inline: auto;
  }

  .mobile-sticky-total {
    display: flex;
    flex-direction: column;
  }

  .mobile-total-label {
    font-size: 0.75rem;
    color: var(--color-text-light);
  }

  .mobile-total-price {
    font-size: 1.25rem;
    font-weight: 700;
    color: var(--color-primary);
    font-family: var(--font-display);
  }

  .mobile-checkout-btn {
    flex-shrink: 0;
  }

  /* ── Modal Actions ── */
  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }

  /* ── Desktop ── */
  @media (min-width: 768px) {
    .basket-title {
      font-size: 2rem;
    }

    .basket-item {
      padding: var(--space-5);
    }

    .item-image,
    .item-image-placeholder {
      width: 100px;
      height: 100px;
    }
  }

  @media (min-width: 1024px) {
    .basket-layout {
      flex-direction: row;
      align-items: flex-start;
    }

    .basket-items {
      flex: 2;
    }

    .order-summary {
      flex: 1;
      max-width: 380px;
    }

    .summary-card {
      position: sticky;
      top: var(--space-6);
    }

    .mobile-sticky-bar {
      display: none;
    }

    .basket-page {
      padding-bottom: var(--space-16);
    }
  }

  /* Ensure space for mobile sticky bar */
  @media (max-width: 1023px) {
    .basket-page {
      padding-bottom: 100px;
    }
  }
</style>
