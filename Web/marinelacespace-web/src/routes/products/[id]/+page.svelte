<script lang="ts">
  import { page } from '$app/stores';
  import Breadcrumb from '$components/Breadcrumb.svelte';
  import ReviewStars from '$components/ReviewStars.svelte';
  import PriceDisplay from '$components/PriceDisplay.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import { getProductById, getProductReviews, getReviewSummary, createReview, getCategoryById } from '$api/catalog';
  import { addItem } from '$api/basket';
  import { basketStore } from '$stores/basket';
  import { notificationStore } from '$stores/notification';
  import type {
    ProductDetail,
    ProductImage,
    ProductSize,
    ProductColor,
    ProductMaterial,
    ProductPrice,
    ProductReview,
    ReviewSummary,
    Category,
    PaginatedResponse,
    AddToBasketRequest,
  } from '$types';

  // ─── State ─────────────────────────────────────────────────────────────────

  let isLoading = $state(true);
  let product = $state<ProductDetail | null>(null);
  let category = $state<Category | null>(null);
  let reviewSummary = $state<ReviewSummary | null>(null);
  let reviews = $state<ProductReview[]>([]);
  let reviewPage = $state(1);
  let reviewTotalPages = $state(1);
  let isReviewsLoading = $state(false);

  // ─── Gallery ───────────────────────────────────────────────────────────────

  let selectedImageIndex = $state(0);

  let filteredImages = $derived.by<ProductImage[]>(() => {
    if (!product) return [];
    const imgs = product.images;
    if (!selectedColor && !selectedMaterial) return imgs;
    const variantImgs = imgs.filter((img) => {
      if (selectedColor && img.colorId === selectedColor) return true;
      if (selectedMaterial && img.materialId === selectedMaterial) return true;
      return false;
    });
    return variantImgs.length > 0 ? variantImgs : imgs;
  });

  let mainImage = $derived(filteredImages.length > 0 ? filteredImages[selectedImageIndex] ?? filteredImages[0] : null);

  // ─── Selected Variants ─────────────────────────────────────────────────────

  let selectedSize = $state<string | undefined>(undefined);
  let selectedColor = $state<string | undefined>(undefined);
  let selectedMaterial = $state<string | undefined>(undefined);
  let quantity = $state(1);
  let personalization = $state('');

  // ─── Derived Price ─────────────────────────────────────────────────────────

  let matchedPrice = $derived.by<ProductPrice | undefined>(() => {
    if (!product) return undefined;
    return product.prices.find(
      (p) =>
        (!selectedSize || p.sizeId === selectedSize) &&
        (!selectedColor || p.colorId === selectedColor) &&
        (!selectedMaterial || p.materialId === selectedMaterial)
    );
  });

  let displayPrice = $derived(matchedPrice?.basePrice ?? product?.minPrice ?? 0);
  let displayOldPrice = $derived(matchedPrice?.oldPrice ?? undefined);
  let displayDiscount = $derived(matchedPrice?.discountPercentage ?? undefined);
  let inStock = $derived((matchedPrice?.quantity ?? 0) > 0);

  // ─── Status Badge ──────────────────────────────────────────────────────────

  const statusLabel: Record<string, string> = {
    Active: 'В наявності',
    SoldOut: 'Розпродано',
    Draft: 'Чернетка',
    Inactive: 'Неактивний',
  };

  const statusClass: Record<string, string> = {
    Active: 'badge-success',
    SoldOut: 'badge-error',
    Draft: 'badge-outline',
    Inactive: 'badge-warning',
  };

  // ─── Add to Cart ───────────────────────────────────────────────────────────

  let isAddingToCart = $state(false);

  async function handleAddToCart() {
    if (!product) return;
    isAddingToCart = true;
    try {
      const request: AddToBasketRequest = {
        productId: product.id,
        sizeId: selectedSize,
        colorId: selectedColor,
        materialId: selectedMaterial,
        quantity,
        personalization: product.isPersonalizationEnabled && personalization.trim() ? personalization.trim() : undefined,
      };
      await addItem(request);
      await basketStore.loadBasket();
      notificationStore.success('Товар додано до кошика!');
    } catch (err) {
      notificationStore.error('Не вдалося додати товар до кошика');
      console.error(err);
    } finally {
      isAddingToCart = false;
    }
  }

  // ─── Review Form ───────────────────────────────────────────────────────────

  let showReviewForm = $state(false);
  let reviewRating = $state(0);
  let reviewTitle = $state('');
  let reviewText = $state('');
  let isSubmittingReview = $state(false);

  async function submitReview() {
    if (!product || reviewRating < 1 || !reviewText.trim()) return;
    isSubmittingReview = true;
    try {
      await createReview(product.id, {
        rating: reviewRating,
        title: reviewTitle.trim() || undefined,
        text: reviewText.trim(),
      });
      notificationStore.success('Відгук надіслано!');
      showReviewForm = false;
      reviewRating = 0;
      reviewTitle = '';
      reviewText = '';
      await loadReviews();
      reviewSummary = await getReviewSummary(product.id);
    } catch {
      notificationStore.error('Не вдалося надіслати відгук');
    } finally {
      isSubmittingReview = false;
    }
  }

  // ─── Data Loading ──────────────────────────────────────────────────────────

  async function loadProduct(id: string) {
    isLoading = true;
    try {
      product = await getProductById(id);
      // Pre-select first available variants
      if (product.sizes.length > 0) selectedSize = product.sizes[0].sizeId;
      if (product.colors.length > 0) selectedColor = product.colors[0].colorId;
      if (product.materials.length > 0) selectedMaterial = product.materials[0].materialId;

      // Load category for breadcrumb
      if (product.categoryId) {
        try { category = await getCategoryById(product.categoryId); } catch { /* noop */ }
      }

      // Load reviews
      reviewSummary = await getReviewSummary(id);
      await loadReviews();
    } catch (err) {
      console.error('Failed to load product', err);
    } finally {
      isLoading = false;
    }
  }

  async function loadReviews() {
    if (!product) return;
    isReviewsLoading = true;
    try {
      const result: PaginatedResponse<ProductReview> = await getProductReviews(product.id, {
        page: reviewPage,
        pageSize: 5,
      });
      reviews = result.items;
      reviewTotalPages = result.totalPages;
    } catch {
      reviews = [];
    } finally {
      isReviewsLoading = false;
    }
  }

  function handleReviewPageChange(p: number) {
    reviewPage = p;
    loadReviews();
  }

  // ─── Lifecycle ─────────────────────────────────────────────────────────────

  $effect(() => {
    const id = $page.params.id;
    if (id) loadProduct(id);
  });

  // ─── Reset image index when variant changes ───────────────────────────────

  $effect(() => {
    // Track dependencies
    void selectedColor;
    void selectedMaterial;
    selectedImageIndex = 0;
  });

  // ─── Breadcrumb ────────────────────────────────────────────────────────────

  let breadcrumbItems = $derived([
    { label: 'Головна', href: '/' },
    { label: 'Каталог', href: '/catalog' },
    ...(category ? [{ label: category.name, href: `/catalog?category=${category.id}` }] : []),
    ...(product ? [{ label: product.title }] : []),
  ]);

  // ─── Helpers ───────────────────────────────────────────────────────────────

  function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('uk-UA', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  function getInventoryForSize(sizeId: string): number {
    if (!product) return 0;
    return product.prices
      .filter((p) => p.sizeId === sizeId && (!selectedColor || p.colorId === selectedColor) && (!selectedMaterial || p.materialId === selectedMaterial))
      .reduce((sum, p) => sum + p.quantity, 0);
  }
</script>

<svelte:head>
  <title>{product?.title ?? 'Товар'} — MarineLaceSpace</title>
</svelte:head>

{#if isLoading}
  <div class="container product-loading">
    <LoadingSpinner size="lg" message="Завантаження товару…" />
  </div>
{:else if product}
  <div class="product-page">
    <div class="container">
      <Breadcrumb items={breadcrumbItems} />

      <div class="product-main">
        <!-- ─── Gallery ──────────────────────────────────────────────────── -->
        <div class="gallery">
          <div class="gallery-main">
            {#if mainImage}
              <img
                src={mainImage.url}
                alt={mainImage.altText ?? product.title}
                class="gallery-main-image"
              />
            {:else}
              <div class="gallery-placeholder" aria-hidden="true">
                <svg viewBox="0 0 24 24" width="64" height="64" fill="none" stroke="currentColor" stroke-width="1" opacity="0.25">
                  <rect x="3" y="3" width="18" height="18" rx="2" />
                  <circle cx="8.5" cy="8.5" r="1.5" />
                  <polyline points="21 15 16 10 5 21" />
                </svg>
              </div>
            {/if}
          </div>
          {#if filteredImages.length > 1}
            <div class="gallery-thumbs" role="list" aria-label="Зображення товару">
              {#each filteredImages as img, i (img.id)}
                <button
                  class="thumb"
                  class:active={i === selectedImageIndex}
                  on:click={() => (selectedImageIndex = i)}
                  aria-label="Зображення {i + 1}"
                  role="listitem"
                >
                  <img src={img.url} alt={img.altText ?? `${product.title} — ${i + 1}`} />
                </button>
              {/each}
            </div>
          {/if}
        </div>

        <!-- ─── Info ─────────────────────────────────────────────────────── -->
        <div class="product-info">
          <a href="/shops/{product.shopId}" class="shop-link">{product.shopName}</a>

          <h1 class="product-title">{product.title}</h1>

          <div class="product-meta">
            <ReviewStars rating={product.averageRating} count={product.reviewCount} />
            <span class="badge {statusClass[product.status] ?? 'badge-outline'}">{statusLabel[product.status] ?? product.status}</span>
          </div>

          <div class="product-price-display">
            <PriceDisplay price={displayPrice} oldPrice={displayOldPrice} discountPercentage={displayDiscount} />
          </div>

          <!-- ─── Size Selector ──────────────────────────────────────────── -->
          {#if product.sizes.length > 0}
            <div class="variant-section">
              <h3 class="variant-label">Розмір</h3>
              <div class="variant-options">
                {#each product.sizes as size (size.sizeId)}
                  {@const stock = getInventoryForSize(size.sizeId)}
                  <button
                    class="variant-btn"
                    class:selected={selectedSize === size.sizeId}
                    class:out-of-stock={stock === 0}
                    on:click={() => (selectedSize = size.sizeId)}
                    disabled={stock === 0}
                    aria-pressed={selectedSize === size.sizeId}
                    title={stock === 0 ? 'Немає в наявності' : size.sizeName}
                  >
                    {size.sizeName}
                  </button>
                {/each}
              </div>
            </div>
          {/if}

          <!-- ─── Color Selector ─────────────────────────────────────────── -->
          {#if product.colors.length > 0}
            <div class="variant-section">
              <h3 class="variant-label">Колір</h3>
              <div class="variant-options color-options">
                {#each product.colors as color (color.colorId)}
                  <button
                    class="color-circle"
                    class:selected={selectedColor === color.colorId}
                    style="--clr: {color.hexCode}"
                    on:click={() => (selectedColor = color.colorId)}
                    aria-label={color.colorName}
                    aria-pressed={selectedColor === color.colorId}
                    title={color.colorName}
                  ></button>
                {/each}
              </div>
            </div>
          {/if}

          <!-- ─── Material Selector ──────────────────────────────────────── -->
          {#if product.materials.length > 0}
            <div class="variant-section">
              <h3 class="variant-label">Матеріал</h3>
              <div class="variant-options">
                {#each product.materials as material (material.materialId)}
                  <button
                    class="variant-btn"
                    class:selected={selectedMaterial === material.materialId}
                    on:click={() => (selectedMaterial = material.materialId)}
                    aria-pressed={selectedMaterial === material.materialId}
                  >
                    {material.materialName}
                  </button>
                {/each}
              </div>
            </div>
          {/if}

          <!-- ─── Personalization ────────────────────────────────────────── -->
          {#if product.isPersonalizationEnabled}
            <div class="variant-section">
              <h3 class="variant-label">Персоналізація</h3>
              {#if product.personalizationPrompt}
                <p class="personalization-prompt">{product.personalizationPrompt}</p>
              {/if}
              <textarea
                class="input personalization-input"
                bind:value={personalization}
                maxlength="512"
                placeholder="Введіть ваш текст…"
                rows="3"
              ></textarea>
              <small class="text-muted">{personalization.length}/512</small>
            </div>
          {/if}

          <!-- ─── Add to Cart ────────────────────────────────────────────── -->
          <div class="add-to-cart">
            <div class="quantity-selector">
              <button
                class="btn btn-icon quantity-btn"
                on:click={() => (quantity = Math.max(1, quantity - 1))}
                disabled={quantity <= 1}
                aria-label="Зменшити кількість"
              >−</button>
              <span class="quantity-value" aria-live="polite">{quantity}</span>
              <button
                class="btn btn-icon quantity-btn"
                on:click={() => (quantity = quantity + 1)}
                aria-label="Збільшити кількість"
              >+</button>
            </div>
            <button
              class="btn btn-primary btn-lg add-to-cart-btn"
              on:click={handleAddToCart}
              disabled={isAddingToCart || !inStock}
            >
              {#if isAddingToCart}
                Додаємо…
              {:else if !inStock}
                Немає в наявності
              {:else}
                Додати в кошик
              {/if}
            </button>
          </div>

          <!-- ─── Description ────────────────────────────────────────────── -->
          <div class="product-description">
            <h2>Опис</h2>
            <div class="description-text">{product.description}</div>
          </div>
        </div>
      </div>

      <!-- ─── Reviews Section ──────────────────────────────────────────────── -->
      <section class="reviews-section" aria-label="Відгуки">
        <h2>Відгуки</h2>

        {#if reviewSummary}
          <div class="review-summary-bar">
            <div class="summary-overall">
              <span class="summary-avg">{reviewSummary.averageRating.toFixed(1)}</span>
              <ReviewStars rating={reviewSummary.averageRating} size="lg" />
              <span class="summary-count">{reviewSummary.totalCount} відгуків</span>
            </div>
            <div class="summary-distribution">
              {#each [5, 4, 3, 2, 1] as star (star)}
                {@const count = reviewSummary.distribution[star] ?? 0}
                {@const pct = reviewSummary.totalCount > 0 ? (count / reviewSummary.totalCount) * 100 : 0}
                <div class="distrib-row">
                  <span class="distrib-star">{star}★</span>
                  <div class="distrib-bar">
                    <div class="distrib-fill" style="width: {pct}%"></div>
                  </div>
                  <span class="distrib-count">{count}</span>
                </div>
              {/each}
            </div>
          </div>
        {/if}

        <button class="btn btn-outline" on:click={() => (showReviewForm = !showReviewForm)}>
          {showReviewForm ? 'Скасувати' : 'Залишити відгук'}
        </button>

        {#if showReviewForm}
          <form class="review-form" on:submit|preventDefault={submitReview}>
            <div class="review-form-rating">
              <span class="variant-label">Оцінка</span>
              <div class="star-picker">
                {#each [1, 2, 3, 4, 5] as star (star)}
                  <button
                    type="button"
                    class="star-pick"
                    class:filled={star <= reviewRating}
                    on:click={() => (reviewRating = star)}
                    aria-label="Оцінка {star}"
                  >★</button>
                {/each}
              </div>
            </div>
            <input
              class="input"
              placeholder="Заголовок (необов'язково)"
              bind:value={reviewTitle}
            />
            <textarea
              class="input"
              placeholder="Ваш відгук…"
              bind:value={reviewText}
              rows="4"
              required
            ></textarea>
            <button class="btn btn-primary" type="submit" disabled={isSubmittingReview || reviewRating < 1 || !reviewText.trim()}>
              {isSubmittingReview ? 'Надсилаємо…' : 'Надіслати відгук'}
            </button>
          </form>
        {/if}

        <!-- Review List -->
        {#if isReviewsLoading}
          <LoadingSpinner message="Завантаження відгуків…" />
        {:else if reviews.length > 0}
          <ul class="review-list">
            {#each reviews as review (review.id)}
              <li class="review-item card">
                <div class="card-body">
                  <div class="review-header">
                    <ReviewStars rating={review.rating} size="sm" />
                    {#if review.isVerifiedPurchase}
                      <span class="badge badge-success">Підтверджена покупка</span>
                    {/if}
                  </div>
                  {#if review.title}
                    <h4 class="review-title">{review.title}</h4>
                  {/if}
                  <p class="review-text">{review.text}</p>
                  <div class="review-footer">
                    <span class="text-muted text-sm">
                      {review.guestName ?? 'Покупець'} • {formatDate(review.createdAt)}
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
        {:else}
          <p class="text-muted mt-4">Ще немає відгуків. Будьте першими!</p>
        {/if}
      </section>
    </div>
  </div>
{:else}
  <div class="container product-loading">
    <p>Товар не знайдено.</p>
  </div>
{/if}

<style>
  .product-loading {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
  }

  .product-page {
    padding-block: var(--space-4) var(--space-16);
  }

  /* ─── Main Layout ─────────────────────────────────────────────────────── */

  .product-main {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-10);
    margin-top: var(--space-4);
  }

  /* ─── Gallery ─────────────────────────────────────────────────────────── */

  .gallery {
    position: sticky;
    top: var(--space-4);
    align-self: start;
  }

  .gallery-main {
    aspect-ratio: 3 / 4;
    border-radius: var(--radius-lg);
    overflow: hidden;
    background-color: var(--color-surface-hover);
    margin-bottom: var(--space-3);
  }

  .gallery-main-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .gallery-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .gallery-thumbs {
    display: flex;
    gap: var(--space-2);
    overflow-x: auto;
    padding-bottom: var(--space-2);
  }

  .thumb {
    width: 64px;
    height: 80px;
    border-radius: var(--radius-md);
    overflow: hidden;
    border: 2px solid var(--color-border-light);
    cursor: pointer;
    flex-shrink: 0;
    transition: border-color var(--transition-fast);
    padding: 0;
    background: none;
  }

  .thumb.active {
    border-color: var(--color-primary);
  }

  .thumb img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  /* ─── Product Info ────────────────────────────────────────────────────── */

  .product-info {
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
  }

  .shop-link {
    font-size: 0.875rem;
    color: var(--color-primary);
    text-decoration: none;
    font-weight: 500;
    transition: color var(--transition-fast);
  }

  .shop-link:hover {
    color: var(--color-primary-dark);
    text-decoration: underline;
  }

  .product-title {
    font-family: var(--font-display);
    font-size: 2rem;
    font-weight: 700;
    line-height: 1.2;
    margin: 0;
  }

  .product-meta {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    flex-wrap: wrap;
  }

  .product-price-display {
    font-size: 1.5rem;
  }

  /* ─── Variant Selectors ───────────────────────────────────────────────── */

  .variant-section {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .variant-label {
    font-family: var(--font-body);
    font-size: 0.8125rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-text-light);
    margin: 0;
  }

  .variant-options {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2);
  }

  .variant-btn {
    padding: var(--space-2) var(--space-4);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    background: var(--color-surface);
    font-size: 0.875rem;
    font-weight: 500;
    cursor: pointer;
    transition: all var(--transition-fast);
    color: var(--color-text);
  }

  .variant-btn:hover:not(:disabled) {
    border-color: var(--color-primary-light);
  }

  .variant-btn.selected {
    border-color: var(--color-primary);
    background-color: var(--color-primary);
    color: #fff;
  }

  .variant-btn.out-of-stock {
    opacity: 0.4;
    text-decoration: line-through;
    cursor: not-allowed;
  }

  .color-circle {
    width: 36px;
    height: 36px;
    border-radius: var(--radius-full);
    background-color: var(--clr);
    border: 3px solid var(--color-border);
    cursor: pointer;
    transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
    padding: 0;
  }

  .color-circle:hover {
    border-color: var(--color-primary-light);
  }

  .color-circle.selected {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 2px var(--color-primary-light);
  }

  .personalization-prompt {
    font-size: 0.875rem;
    color: var(--color-text-light);
    margin: 0;
  }

  .personalization-input {
    min-height: 80px;
    resize: vertical;
  }

  /* ─── Add to Cart ─────────────────────────────────────────────────────── */

  .add-to-cart {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    flex-wrap: wrap;
  }

  .quantity-selector {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    padding: var(--space-1);
  }

  .quantity-btn {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
    border-radius: var(--radius-md);
    color: var(--color-text);
  }

  .quantity-btn:hover:not(:disabled) {
    background-color: var(--color-surface-hover);
  }

  .quantity-value {
    min-width: 32px;
    text-align: center;
    font-weight: 600;
    font-size: 1rem;
  }

  .add-to-cart-btn {
    flex: 1;
    min-width: 200px;
  }

  /* ─── Description ─────────────────────────────────────────────────────── */

  .product-description h2 {
    font-size: 1.25rem;
    margin-bottom: var(--space-3);
  }

  .description-text {
    font-size: 0.9375rem;
    line-height: 1.7;
    color: var(--color-text);
    white-space: pre-line;
  }

  /* ─── Reviews Section ─────────────────────────────────────────────────── */

  .reviews-section {
    margin-top: var(--space-16);
    border-top: 1px solid var(--color-border-light);
    padding-top: var(--space-8);
  }

  .reviews-section h2 {
    font-size: 1.5rem;
    margin-bottom: var(--space-6);
  }

  .review-summary-bar {
    display: flex;
    gap: var(--space-10);
    align-items: flex-start;
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
  }

  .summary-overall {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-2);
  }

  .summary-avg {
    font-family: var(--font-display);
    font-size: 3rem;
    font-weight: 700;
    line-height: 1;
    color: var(--color-text);
  }

  .summary-count {
    font-size: 0.875rem;
    color: var(--color-text-muted);
  }

  .summary-distribution {
    flex: 1;
    min-width: 200px;
    max-width: 400px;
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .distrib-row {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }

  .distrib-star {
    font-size: 0.8125rem;
    color: var(--color-text-light);
    width: 28px;
    text-align: right;
  }

  .distrib-bar {
    flex: 1;
    height: 8px;
    background-color: var(--color-border-light);
    border-radius: var(--radius-full);
    overflow: hidden;
  }

  .distrib-fill {
    height: 100%;
    background-color: var(--color-secondary);
    border-radius: var(--radius-full);
    transition: width var(--transition-base);
  }

  .distrib-count {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
    width: 28px;
  }

  /* ─── Review Form ─────────────────────────────────────────────────────── */

  .review-form {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    margin-top: var(--space-4);
    padding: var(--space-6);
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
  }

  .review-form-rating {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .star-picker {
    display: flex;
    gap: 2px;
  }

  .star-pick {
    font-size: 1.5rem;
    background: none;
    border: none;
    cursor: pointer;
    color: var(--color-border);
    transition: color var(--transition-fast);
    padding: 0;
    line-height: 1;
  }

  .star-pick.filled {
    color: var(--color-secondary);
  }

  .star-pick:hover {
    color: var(--color-secondary);
  }

  /* ─── Review List ─────────────────────────────────────────────────────── */

  .review-list {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    margin-top: var(--space-6);
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

  .pagination-wrapper {
    margin-top: var(--space-6);
    display: flex;
    justify-content: center;
  }

  /* ─── Responsive ────────────────────────────────────────────────────────── */

  @media (max-width: 768px) {
    .product-main {
      grid-template-columns: 1fr;
      gap: var(--space-6);
    }

    .gallery {
      position: static;
    }

    .product-title {
      font-size: 1.5rem;
    }

    .review-summary-bar {
      flex-direction: column;
      gap: var(--space-4);
    }

    .summary-distribution {
      max-width: 100%;
    }
  }
</style>
