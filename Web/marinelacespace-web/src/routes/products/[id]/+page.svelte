<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
  import { page } from '$app/stores';
  import ReviewStars from '$components/ReviewStars.svelte';
  import PriceDisplay from '$components/PriceDisplay.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Breadcrumb from '$components/Breadcrumb.svelte';
  import Pagination from '$components/Pagination.svelte';
  import { getProductById, getProductReviews, getReviewSummary, createReview, getCategoryById, getSizes, getColors, getMaterials } from '$api/catalog';
  import { basketStore } from '$stores/basket.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import type {
    ProductDetail,
    ProductPhoto,
    ProductInventoryItem,
    ProductReview,
    ReviewSummary,
    Category,
    Size,
    Color,
    Material,
    PaginatedResponse,
    AddToBasketRequest,
  } from '$types';

  let isLoading = $state(true);
  let loadError = $state(false);
  let product = $state<ProductDetail | null>(null);
  let category = $state<Category | null>(null);
  let reviewSummary = $state<ReviewSummary | null>(null);
  let reviews = $state<ProductReview[]>([]);
  let reviewPage = $state(1);
  let reviewTotalPages = $state(1);
  let isReviewsLoading = $state(false);

  let allSizes = $state<Size[]>([]);
  let allColors = $state<Color[]>([]);
  let allMaterials = $state<Material[]>([]);

  let selectedImageIndex = $state(0);
  let allPhotosImageIndex = $state(0);

  let allPhotos = $derived<ProductPhoto[]>(product?.photos ?? []);

  let filteredImages = $derived.by<ProductPhoto[]>(() => {
    if (!product) return [];
    const imgs = product.photos;
    if (!selectedColor && !selectedMaterial) return imgs;

    if (selectedColor && selectedMaterial) {
      const both = imgs.filter(img => img.colorId === selectedColor && img.materialId === selectedMaterial);
      if (both.length > 0) return both;
    }

    const partial = imgs.filter(img => {
      if (selectedColor && img.colorId === selectedColor) return true;
      if (selectedMaterial && img.materialId === selectedMaterial) return true;
      return false;
    });
    return partial.length > 0 ? partial : imgs;
  });

  let mainImage = $derived(filteredImages.length > 0 ? filteredImages[selectedImageIndex] ?? filteredImages[0] : null);

  let selectedSize = $state<string | undefined>(undefined);
  let selectedColor = $state<string | undefined>(undefined);
  let selectedMaterial = $state<string | undefined>(undefined);
  let quantity = $state(1);
  let personalization = $state('');

  let hasFilteredSubset = $derived(
    (selectedColor || selectedMaterial) && filteredImages.length < allPhotos.length
  );

  let matchedInventory = $derived.by<ProductInventoryItem | undefined>(() => {
    if (!product) return undefined;
    return product.inventory.find(
      (p) =>
        (!selectedSize || p.sizeId === selectedSize) &&
        (!selectedColor || p.colorId === selectedColor) &&
        (!selectedMaterial || p.materialId === selectedMaterial)
    );
  });

  let displayPrice = $derived(matchedInventory?.price ?? product?.price ?? 0);
  let isUnlimited = $derived(product?.isUnlimitedQuantity === true);
  let maxQuantity = $derived(isUnlimited ? 50 : (matchedInventory?.quantity ?? 0));
  let inStock = $derived(isUnlimited || (matchedInventory?.quantity ?? 0) > 0);

  let productSizes = $derived.by(() => {
    if (!product) return [];
    const sizeIds = [...new Set(product.inventory.map(i => i.sizeId).filter(Boolean) as string[])];
    return allSizes.filter(s => sizeIds.includes(s.id));
  });

  let productColors = $derived.by(() => {
    if (!product) return [];
    const colorIds = [...new Set(product.inventory.map(i => i.colorId).filter(Boolean) as string[])];
    return allColors.filter(c => colorIds.includes(c.id));
  });

  let productMaterials = $derived.by(() => {
    if (!product) return [];
    const materialIds = [...new Set(product.inventory.map(i => i.materialId).filter(Boolean) as string[])];
    return allMaterials.filter(m => materialIds.includes(m.id));
  });

  let hasSizes = $derived(productSizes.length > 0);
  let hasColors = $derived(productColors.length > 0);
  let hasMaterials = $derived(productMaterials.length > 0);

  let allVariantsSelected = $derived(
    (!hasSizes || !!selectedSize) &&
    (!hasColors || !!selectedColor) &&
    (!hasMaterials || !!selectedMaterial)
  );

  let canAddToCart = $derived(inStock && allVariantsSelected && maxQuantity > 0);

  let isAddingToCart = $state(false);

  async function handleAddToCart() {
    if (!product) return;
    isAddingToCart = true;
    try {
      const selectedSizeObj = productSizes.find(s => s.id === selectedSize);
      const selectedColorObj = productColors.find(c => c.id === selectedColor);
      const selectedMaterialObj = productMaterials.find(m => m.id === selectedMaterial);
      const mainImg = filteredImages.find(img => img.isMain) ?? filteredImages[0];

      const request: AddToBasketRequest = {
        productId: product.id,
        productName: product.name,
        sizeId: selectedSize,
        sizeName: selectedSizeObj?.name,
        colorId: selectedColor,
        colorName: selectedColorObj?.name,
        materialId: selectedMaterial,
        materialName: selectedMaterialObj?.name,
        unitPrice: displayPrice,
        quantity,
        personalization: product.allowPersonalization && personalization.trim() ? personalization.trim() : undefined,
        imageUrl: mainImg?.url ?? product.mainImageUrl ?? undefined,
        shopId: product.shopId,
      };
      await basketStore.addItem(request);
      notificationStore.success(i18n.t('product.addedToBasket'));
    } catch (err) {
      notificationStore.error(i18n.t('product.addToBasketError'));
      console.error(err);
    } finally {
      isAddingToCart = false;
    }
  }

  let showReviewForm = $state(false);
  let reviewRating = $state(0);
  let reviewTitle = $state('');
  let reviewText = $state('');
  let isSubmittingReview = $state(false);
  let reviewValidationError = $state('');

  async function submitReview() {
    reviewValidationError = '';
    if (!product) return;
    if (reviewRating < 1) {
      reviewValidationError = i18n.t('product.ratingRequired');
      return;
    }
    if (!reviewText.trim()) {
      reviewValidationError = i18n.t('product.reviewTextRequired');
      return;
    }
    isSubmittingReview = true;
    try {
      await createReview(product.id, {
        rating: reviewRating,
        title: reviewTitle.trim() || undefined,
        text: reviewText.trim(),
      });
      notificationStore.success(i18n.t('product.reviewSubmitted'));
      showReviewForm = false;
      reviewRating = 0;
      reviewTitle = '';
      reviewText = '';
      await loadReviews();
      reviewSummary = await getReviewSummary(product.id);
    } catch {
      notificationStore.error(i18n.t('product.reviewSubmitError'));
    } finally {
      isSubmittingReview = false;
    }
  }

  async function loadProduct(id: string) {
    isLoading = true;
    loadError = false;
    try {
      const [productRes, sizesRes, colorsRes, materialsRes] = await Promise.all([
        getProductById(id),
        getSizes(),
        getColors(),
        getMaterials(),
      ]);
      product = productRes;
      allSizes = sizesRes;
      allColors = colorsRes;
      allMaterials = materialsRes;

      const sizeIds = [...new Set(productRes.inventory.map(i => i.sizeId).filter(Boolean) as string[])];
      const colorIds = [...new Set(productRes.inventory.map(i => i.colorId).filter(Boolean) as string[])];
      const materialIds = [...new Set(productRes.inventory.map(i => i.materialId).filter(Boolean) as string[])];

      selectedSize = sizeIds.length === 1 ? sizeIds[0] : undefined;
      selectedColor = colorIds.length === 1 ? colorIds[0] : undefined;
      selectedMaterial = materialIds.length === 1 ? materialIds[0] : undefined;

      prevColor = selectedColor;
      prevMaterial = selectedMaterial;
      selectedImageIndex = 0;
      allPhotosImageIndex = 0;

      if (product.categoryId) {
        try { category = await getCategoryById(product.categoryId); } catch { /* noop */ }
      }

      reviewSummary = await getReviewSummary(id);
      await loadReviews();
    } catch (err) {
      console.error('Failed to load product', err);
      loadError = true;
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
      reviews = Array.isArray(result) ? result : (result.items ?? []);
      reviewTotalPages = Array.isArray(result) ? 1 : (result.totalPages ?? 1);
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

  let prevProductId = $state('');

  $effect(() => {
    const id = $page.params.id;
    if (id && id !== prevProductId) {
      prevProductId = id;
      loadProduct(id);
    }
  });

  let prevColor = $state<string | undefined>(undefined);
  let prevMaterial = $state<string | undefined>(undefined);

  function findBestImageIndex(
    images: ProductPhoto[],
    primaryField: 'colorId' | 'materialId',
    primaryValue: string | undefined,
    secondaryField: 'colorId' | 'materialId',
    secondaryValue: string | undefined,
    currentIndex: number
  ): number {
    if (!primaryValue) return 0;

    if (secondaryValue) {
      const idx = images.findIndex(
        img => img[primaryField] === primaryValue && img[secondaryField] === secondaryValue
      );
      if (idx !== -1) return idx;
    }

    const idxNoSecondary = images.findIndex(
      img => img[primaryField] === primaryValue && !img[secondaryField]
    );
    if (idxNoSecondary !== -1) return idxNoSecondary;

    const idxAny = images.findIndex(img => img[primaryField] === primaryValue);
    if (idxAny !== -1) return idxAny;

    return Math.min(currentIndex, Math.max(images.length - 1, 0));
  }

  $effect(() => {
    const colorChanged = selectedColor !== prevColor;
    const materialChanged = selectedMaterial !== prevMaterial;

    if (colorChanged || materialChanged) {
      const imgs = filteredImages;

      if (materialChanged && !colorChanged) {
        selectedImageIndex = findBestImageIndex(
          imgs, 'materialId', selectedMaterial, 'colorId', selectedColor, selectedImageIndex
        );
      } else if (colorChanged && !materialChanged) {
        selectedImageIndex = findBestImageIndex(
          imgs, 'colorId', selectedColor, 'materialId', selectedMaterial, selectedImageIndex
        );
      } else {
        selectedImageIndex = 0;
      }

      prevColor = selectedColor;
      prevMaterial = selectedMaterial;
    }

    void selectedSize;
    if (quantity > maxQuantity && maxQuantity > 0) quantity = maxQuantity;
    if (maxQuantity === 0 && quantity !== 1) quantity = 1;
  });

  function selectImageFromAllPhotos(photo: ProductPhoto) {
    const idx = filteredImages.findIndex(img => img.id === photo.id);
    if (idx !== -1) {
      selectedImageIndex = idx;
    } else {
      selectedImageIndex = 0;
    }
    allPhotosImageIndex = allPhotos.findIndex(img => img.id === photo.id);
  }

  function formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('uk-UA', { year: 'numeric', month: 'long', day: 'numeric' });
  }

  function getInventoryForSize(sizeId: string): number {
    if (!product) return 0;
    return product.inventory
      .filter((p) => p.sizeId === sizeId && (!selectedColor || p.colorId === selectedColor) && (!selectedMaterial || p.materialId === selectedMaterial))
      .reduce((sum, p) => sum + p.quantity, 0);
  }

  function getSelectedSizeName(): string {
    return productSizes.find(s => s.id === selectedSize)?.name ?? '';
  }

  function getSelectedColorName(): string {
    return productColors.find(c => c.id === selectedColor)?.name ?? '';
  }

  function getSelectedMaterialName(): string {
    return productMaterials.find(m => m.id === selectedMaterial)?.name ?? '';
  }
</script>

<svelte:head>
  <title>{product?.name ?? i18n.t('product.product')} — MarineLaceSpace</title>
</svelte:head>

{#if isLoading}
  <div class="container product-loading">
    <LoadingSpinner size="lg" message={i18n.t('common.loading')} />
  </div>
{:else if loadError}
  <div class="container product-loading">
    <EmptyState
      title={i18n.t('product.notFound')}
      description={i18n.t('common.tryAgain')}
      icon="⚠️"
      actionLabel={i18n.t('common.tryAgain')}
      onaction={() => { const id = $page.params.id; if (id) loadProduct(id); }}
    />
  </div>
{:else if product}
  <div class="product-page">
    <div class="container">
      <Breadcrumb items={[
        { label: i18n.t('catalog.title'), href: '/catalog' },
        ...(category ? [{ label: category.name, href: `/catalog?category=${category.id}` }] : []),
        { label: product.name }
      ]} />

      <div class="product-main">
        <div class="gallery">
          <div class="gallery-main">
            {#if mainImage}
              <img
                src={mainImage.url}
                alt={mainImage.altText ?? product.name}
                class="gallery-main-image"
                loading="eager"
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
            <div class="gallery-thumbs" role="list" aria-label={i18n.t('product.images')}>
              {#each filteredImages as img, i (img.id)}
                <button
                  class="thumb"
                  class:active={i === selectedImageIndex}
                  onclick={() => (selectedImageIndex = i)}
                  aria-label={i18n.t('product.imageN', { n: i + 1 })}
                >
                  <img src={img.url} alt={img.altText ?? `${product.name} — ${i + 1}`} loading="lazy" />
                </button>
              {/each}
            </div>
          {/if}

          {#if hasFilteredSubset && allPhotos.length > 1}
            <div class="gallery-all-section">
              <span class="gallery-all-label">{i18n.t('product.allPhotos')}</span>
              <div class="gallery-thumbs gallery-thumbs-all" role="list">
                {#each allPhotos as img, i (img.id)}
                  <button
                    class="thumb thumb-small"
                    class:active={allPhotosImageIndex === i}
                    onclick={() => selectImageFromAllPhotos(img)}
                    aria-label={i18n.t('product.imageN', { n: i + 1 })}
                  >
                    <img src={img.url} alt={img.altText ?? `${product.name} — ${i + 1}`} loading="lazy" />
                  </button>
                {/each}
              </div>
            </div>
          {/if}
        </div>

        <div class="product-info">
          <a href="/shops/{product.shopId}" class="shop-link">{product.shopName}</a>

          <h1 class="product-title">{product.name}</h1>

          <div class="product-meta">
            {#if reviewSummary}
              <ReviewStars rating={reviewSummary.averageRating} count={reviewSummary.totalCount} />
            {/if}
          </div>

          <div class="product-price-display">
            <PriceDisplay price={displayPrice} />
          </div>

          {#if hasSizes}
            <div class="variant-section" class:variant-missing={!selectedSize}>
              <label class="variant-label" for="size-select">
                {i18n.t('product.size')}
                {#if selectedSize}
                  <span class="variant-selected-value">: {getSelectedSizeName()}</span>
                {:else}
                  <span class="variant-hint">— {i18n.t('product.selectSize')}</span>
                {/if}
              </label>
              {#if productSizes.length <= 5}
                <div class="variant-options">
                  {#each productSizes as size (size.id)}
                    {@const stock = getInventoryForSize(size.id)}
                    <button
                      class="variant-btn"
                      class:selected={selectedSize === size.id}
                      class:out-of-stock={!isUnlimited && stock === 0}
                      onclick={() => (selectedSize = size.id)}
                      disabled={!isUnlimited && stock === 0}
                      aria-pressed={selectedSize === size.id}
                      title={!isUnlimited && stock === 0 ? i18n.t('product.outOfStock') : size.name}
                    >
                      {size.name}
                    </button>
                  {/each}
                </div>
              {:else}
                <select
                  id="size-select"
                  class="variant-select"
                  value={selectedSize ?? ''}
                  onchange={(e) => {
                    const val = (e.target as HTMLSelectElement).value;
                    selectedSize = val || undefined;
                  }}
                >
                  <option value="">{i18n.t('product.selectSize')}</option>
                  {#each productSizes as size (size.id)}
                    {@const stock = getInventoryForSize(size.id)}
                    <option value={size.id} disabled={!isUnlimited && stock === 0}>
                      {size.name}{!isUnlimited && stock === 0 ? ` (${i18n.t('product.outOfStock')})` : ''}
                    </option>
                  {/each}
                </select>
              {/if}
            </div>
          {/if}

          {#if hasColors}
            <div class="variant-section" class:variant-missing={!selectedColor}>
              <label class="variant-label" for="color-select">
                {i18n.t('product.color')}
                {#if selectedColor}
                  <span class="variant-selected-value">: {getSelectedColorName()}</span>
                {:else}
                  <span class="variant-hint">— {i18n.t('product.selectColor')}</span>
                {/if}
              </label>
              {#if productColors.length <= 5}
                <div class="variant-options color-options">
                  {#each productColors as color (color.id)}
                    <button
                      class="color-circle"
                      class:selected={selectedColor === color.id}
                      style="--clr: {color.hexCode}"
                      onclick={() => (selectedColor = color.id)}
                      aria-label={color.name}
                      aria-pressed={selectedColor === color.id}
                      title={color.name}
                    ></button>
                  {/each}
                </div>
              {:else}
                <select
                  id="color-select"
                  class="variant-select"
                  value={selectedColor ?? ''}
                  onchange={(e) => {
                    const val = (e.target as HTMLSelectElement).value;
                    selectedColor = val || undefined;
                  }}
                >
                  <option value="">{i18n.t('product.selectColor')}</option>
                  {#each productColors as color (color.id)}
                    <option value={color.id}>{color.name}</option>
                  {/each}
                </select>
              {/if}
            </div>
          {/if}

          {#if hasMaterials}
            <div class="variant-section" class:variant-missing={!selectedMaterial}>
              <label class="variant-label" for="material-select">
                {i18n.t('product.material')}
                {#if selectedMaterial}
                  <span class="variant-selected-value">: {getSelectedMaterialName()}</span>
                {:else}
                  <span class="variant-hint">— {i18n.t('product.selectMaterial')}</span>
                {/if}
              </label>
              {#if productMaterials.length <= 5}
                <div class="variant-options">
                  {#each productMaterials as material (material.id)}
                    <button
                      class="variant-btn"
                      class:selected={selectedMaterial === material.id}
                      onclick={() => (selectedMaterial = material.id)}
                      aria-pressed={selectedMaterial === material.id}
                    >
                      {material.name}
                    </button>
                  {/each}
                </div>
              {:else}
                <select
                  id="material-select"
                  class="variant-select"
                  value={selectedMaterial ?? ''}
                  onchange={(e) => {
                    const val = (e.target as HTMLSelectElement).value;
                    selectedMaterial = val || undefined;
                  }}
                >
                  <option value="">{i18n.t('product.selectMaterial')}</option>
                  {#each productMaterials as material (material.id)}
                    <option value={material.id}>{material.name}</option>
                  {/each}
                </select>
              {/if}
            </div>
          {/if}

          {#if product.allowPersonalization}
            <div class="variant-section">
              <label class="variant-label" for="personalization-input">{i18n.t('product.personalization')}</label>
              <textarea
                id="personalization-input"
                class="input personalization-input"
                bind:value={personalization}
                maxlength="512"
                placeholder={i18n.t('product.personalizationPlaceholder')}
                rows="3"
              ></textarea>
              <small class="text-muted">{personalization.length}/512</small>
            </div>
          {/if}

          {#if allVariantsSelected}
            <div class="stock-info" class:low-stock={!isUnlimited && maxQuantity > 0 && maxQuantity <= 5}>
              {#if isUnlimited}
                <span class="stock-badge in-stock">{i18n.t('product.madeToOrder')}</span>
              {:else if maxQuantity > 0}
                <span class="stock-badge in-stock">{i18n.t('product.inStock')}: {maxQuantity}</span>
              {:else}
                <span class="stock-badge out-of-stock-badge">{i18n.t('product.outOfStock')}</span>
              {/if}
            </div>
          {/if}

          <div class="add-to-cart">
            <div class="quantity-selector">
              <button
                class="btn btn-icon quantity-btn"
                onclick={() => (quantity = Math.max(1, quantity - 1))}
                disabled={quantity <= 1 || !canAddToCart}
                aria-label={i18n.t('product.decreaseQuantity')}
              >−</button>
              <span class="quantity-value" aria-live="polite">{quantity}</span>
              <button
                class="btn btn-icon quantity-btn"
                onclick={() => (quantity = Math.min(maxQuantity, quantity + 1))}
                disabled={quantity >= maxQuantity || !canAddToCart}
                aria-label={i18n.t('product.increaseQuantity')}
              >+</button>
            </div>
            <button
              class="btn btn-primary btn-lg add-to-cart-btn"
              onclick={handleAddToCart}
              disabled={isAddingToCart || !canAddToCart}
            >
              {#if isAddingToCart}
                {i18n.t('product.adding')}
              {:else if !allVariantsSelected}
                {i18n.t('product.selectVariants')}
              {:else if !inStock}
                {i18n.t('product.outOfStock')}
              {:else}
                {i18n.t('product.addToBasket')}
              {/if}
            </button>
          </div>

          <div class="product-description">
            <h2>{i18n.t('product.description')}</h2>
            <div class="description-text">{product.description}</div>
          </div>
        </div>
      </div>

      <section class="reviews-section" aria-label={i18n.t('product.reviews')}>
        <h2>{i18n.t('product.reviews')}</h2>

        {#if reviewSummary}
          <div class="review-summary-bar">
            <div class="summary-overall">
              <span class="summary-avg">{reviewSummary.averageRating.toFixed(1)}</span>
              <ReviewStars rating={reviewSummary.averageRating} size="lg" />
              <span class="summary-count">{i18n.t('product.reviewsCount', { count: reviewSummary.totalCount })}</span>
            </div>
            <div class="summary-distribution">
              {#each [5, 4, 3, 2, 1] as star (star)}
                {@const count = reviewSummary.distribution?.[star] ?? 0}
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

        {#if authStore.isAuthenticated}
          <button class="btn btn-outline" onclick={() => (showReviewForm = !showReviewForm)}>
            {showReviewForm ? i18n.t('common.cancel') : i18n.t('product.writeReview')}
          </button>
        {:else}
          <a href="/auth/login" class="btn btn-outline">{i18n.t('product.loginToReview')}</a>
        {/if}

        {#if showReviewForm}
          <form class="review-form" onsubmit={(e) => { e.preventDefault(); submitReview(); }}>
            <div class="review-form-rating">
              <span class="variant-label">{i18n.t('product.rating')}</span>
              <div class="star-picker">
                {#each [1, 2, 3, 4, 5] as star (star)}
                  <button
                    type="button"
                    class="star-pick"
                    class:filled={star <= reviewRating}
                    onclick={() => (reviewRating = star)}
                    aria-label={i18n.t('product.ratingN', { n: star })}
                  >★</button>
                {/each}
              </div>
            </div>
            <input
              class="input"
              placeholder={i18n.t('product.reviewTitlePlaceholder')}
              bind:value={reviewTitle}
            />
            <textarea
              class="input"
              placeholder={i18n.t('product.reviewTextPlaceholder')}
              bind:value={reviewText}
              rows="4"
              required
            ></textarea>
            {#if reviewValidationError}
              <p class="review-validation-error" role="alert">{reviewValidationError}</p>
            {/if}
            <button class="btn btn-primary" type="submit" disabled={isSubmittingReview}>
              {isSubmittingReview ? i18n.t('product.submittingReview') : i18n.t('product.submitReview')}
            </button>
          </form>
        {/if}

        {#if isReviewsLoading}
          <LoadingSpinner message={i18n.t('common.loading')} />
        {:else if reviews.length > 0}
          <ul class="review-list">
            {#each reviews as review (review.id)}
              <li class="review-item card">
                <div class="card-body">
                  <div class="review-header">
                    <ReviewStars rating={review.rating} size="sm" />
                    {#if review.isVerifiedPurchase}
                      <span class="badge badge-success">{i18n.t('product.verifiedPurchase')}</span>
                    {/if}
                  </div>
                  {#if review.title}
                    <h4 class="review-title">{review.title}</h4>
                  {/if}
                  <p class="review-text">{review.text}</p>
                  <div class="review-footer">
                    <span class="text-muted text-sm">
                      {review.guestName ?? i18n.t('product.buyer')} • {formatDate(review.createdAt)}
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
          <p class="text-muted mt-4">{i18n.t('product.noReviews')}</p>
        {/if}
      </section>
    </div>
  </div>
{:else}
  <div class="container product-loading">
    <p>{i18n.t('product.notFound')}</p>
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

  .product-main {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-10);
    margin-top: var(--space-4);
  }

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
    scrollbar-width: thin;
  }

  .thumb {
    width: 64px;
    height: 80px;
    border-radius: var(--radius-md);
    overflow: hidden;
    border: 2px solid var(--color-border-light);
    cursor: pointer;
    flex-shrink: 0;
    transition: border-color var(--transition-fast), opacity var(--transition-fast);
    padding: 0;
    background: none;
  }

  .thumb:hover {
    border-color: var(--color-primary-light);
  }

  .thumb.active {
    border-color: var(--color-primary);
  }

  .thumb img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .thumb-small {
    width: 48px;
    height: 60px;
    opacity: 0.7;
  }

  .thumb-small:hover,
  .thumb-small.active {
    opacity: 1;
  }

  .gallery-all-section {
    margin-top: var(--space-3);
    padding-top: var(--space-3);
    border-top: 1px solid var(--color-border-light);
  }

  .gallery-all-label {
    display: block;
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--color-text-muted);
    margin-bottom: var(--space-2);
  }

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

  .variant-selected-value {
    font-weight: 500;
    color: var(--color-text);
    text-transform: none;
    letter-spacing: normal;
  }

  .variant-hint {
    font-weight: 400;
    font-size: 0.75rem;
    color: var(--color-warning, #D4A040);
    text-transform: none;
    letter-spacing: normal;
  }

  .variant-missing {
    border-left: 3px solid var(--color-warning, #D4A040);
    padding-left: var(--space-3);
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
    background: var(--color-surface-hover);
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

  .variant-select {
    appearance: none;
    padding: var(--space-2) var(--space-4);
    padding-right: var(--space-8);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    background: var(--color-surface) url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%236B6B6B' d='M2.5 4.5L6 8l3.5-3.5'/%3E%3C/svg%3E") no-repeat right var(--space-3) center;
    font-size: 0.875rem;
    font-weight: 500;
    font-family: var(--font-body);
    color: var(--color-text);
    cursor: pointer;
    transition: border-color var(--transition-fast);
    width: 100%;
    max-width: 320px;
  }

  .variant-select:hover {
    border-color: var(--color-primary-light);
  }

  .variant-select:focus {
    outline: none;
    border-color: var(--color-primary);
    box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 15%, transparent);
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

  .personalization-input {
    min-height: 80px;
    resize: vertical;
  }

  .stock-info {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }

  .stock-badge {
    font-size: 0.8125rem;
    font-weight: 600;
    padding: var(--space-1) var(--space-3);
    border-radius: var(--radius-sm);
  }

  .stock-badge.in-stock {
    color: var(--color-success);
    background: color-mix(in srgb, var(--color-success) 10%, transparent);
  }

  .stock-badge.out-of-stock-badge {
    color: var(--color-error);
    background: color-mix(in srgb, var(--color-error) 10%, transparent);
  }

  .low-stock .stock-badge {
    color: var(--color-warning);
    background: color-mix(in srgb, var(--color-warning) 10%, transparent);
  }

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

  .review-validation-error {
    font-size: 0.875rem;
    color: var(--color-error);
    margin: 0;
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

    .variant-select {
      max-width: 100%;
    }
  }
</style>
