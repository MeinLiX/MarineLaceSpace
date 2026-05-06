<script lang="ts">
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import FileUploadDropzone from '$components/FileUploadDropzone.svelte';
  import Modal from '$components/Modal.svelte';
  import CategoryPickerModal from '$components/CategoryPickerModal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type {
    ProductDetail,
    Category,
    Size,
    Color,
    Material,
    ProductInventoryItem,
    ProductPhoto,
  } from '$types';

  let productId = $derived($page.params.id!);
  let isNew = $derived(productId === 'new');
  let loading = $state(true);
  let saving = $state(false);
  let activeTab = $state<'basic' | 'variants' | 'photos' | 'settings'>('basic');

  let name = $state('');
  let description = $state('');
  let categoryId = $state('');
  let shopId = $state('');
  let isActive = $state(true);
  let allowPersonalization = $state(false);
  let price = $state(0);
  let quantity = $state(0);

  let selectedSizeIds = $state<Set<string>>(new Set());
  let selectedColorIds = $state<Set<string>>(new Set());
  let selectedMaterialIds = $state<Set<string>>(new Set());
  let inventory = $state<ProductInventoryItem[]>([]);

  let photos = $state<ProductPhoto[]>([]);
  let uploading = $state(false);
  let uploadTitle = $state('');
  let uploadIsMain = $state(false);
  let fileInput = $state<HTMLInputElement | null>(null);

  let categories = $state<Category[]>([]);
  let allSizes = $state<Size[]>([]);
  let allColors = $state<Color[]>([]);
  let allMaterials = $state<Material[]>([]);

  let shops = $state<{ id: string; name: string }[]>([]);

  let showDeleteModal = $state(false);
  let showDeleteImageModal = $state(false);
  let deleteImageTarget = $state<string | null>(null);
  let showCategoryModal = $state(false);

  let flatCategories = $derived(flattenCategories(categories));
  let selectedCategoryName = $derived(
    flatCategories.find((c) => c.id === categoryId)?.name ?? ''
  );

  function flattenCategories(
    cats: Category[],
    depth = 0
  ): { id: string; name: string; indent: string }[] {
    let result: { id: string; name: string; indent: string }[] = [];
    for (const cat of cats) {
      result.push({ id: cat.id, name: cat.name, indent: '\u2003'.repeat(depth) });
      if (cat.subcategories?.length) {
        result = result.concat(flattenCategories(cat.subcategories, depth + 1));
      }
    }
    return result;
  }

  $effect(() => {
    loadData(productId);
  });

  async function loadData(id: string) {
    try {
      loading = true;

      const shopsPromise = authStore.isAdmin
        ? catalogApi.getShops({ pageSize: 100 }).then((r) => r.items.map((s) => ({ id: s.id, name: s.name })))
        : catalogApi.getMyShops().then((list) => list.map((s) => ({ id: s.id, name: s.name })));

      const shopsRes = await shopsPromise;
      shops = shopsRes;

      const resolvedShopId = !authStore.isAdmin && shopsRes.length > 0 ? shopsRes[0].id : undefined;

      const [catsRes, sizesRes, colorsRes, materialsRes] = await Promise.all([
        catalogApi.getCategoryTree(),
        catalogApi.getSizes(resolvedShopId),
        catalogApi.getColors(resolvedShopId),
        catalogApi.getMaterials(resolvedShopId),
      ]);

      categories = catsRes;
      allSizes = sizesRes;
      allColors = colorsRes;
      allMaterials = materialsRes;

      if (id !== 'new') {
        const product = await catalogApi.getProductById(id);
        name = product.name;
        description = product.description ?? '';
        categoryId = product.categoryId ?? '';
        shopId = product.shopId ?? '';
        isActive = product.isActive ?? true;
        allowPersonalization = product.allowPersonalization ?? false;
        price = product.price ?? 0;
        quantity = product.totalQuantity ?? 0;
        inventory = product.inventory ?? [];
        photos = product.photos ?? [];

        // Initialize selected variant IDs from existing inventory
        const sIds = new Set<string>();
        const cIds = new Set<string>();
        const mIds = new Set<string>();
        for (const item of inventory) {
          if (item.sizeId) sIds.add(item.sizeId);
          if (item.colorId) cIds.add(item.colorId);
          if (item.materialId) mIds.add(item.materialId);
        }
        selectedSizeIds = sIds;
        selectedColorIds = cIds;
        selectedMaterialIds = mIds;
      }
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingData'));
    } finally {
      loading = false;
    }
  }

  async function save() {
    if (!name.trim()) {
      notificationStore.warning(i18n.t('admin.enterProductName'));
      return;
    }
    try {
      saving = true;
      const data: Partial<ProductDetail> & { price?: number; quantity?: number } = {
        name,
        description,
        categoryId,
        shopId,
        isActive,
        allowPersonalization,
        price,
        quantity,
      };

      if (isNew) {
        if (!shopId) {
          notificationStore.warning(i18n.t('admin.shopRequired'));
          return;
        }
        if (!categoryId) {
          notificationStore.info(i18n.t('admin.categoryRecommended'));
        }
        const created = await catalogApi.createProduct(shopId, data);
        notificationStore.success(i18n.t('admin.productCreated'));
        goto(`/admin/products/${created.id}`);
      } else {
        await catalogApi.updateProduct(productId, data);
        notificationStore.success(i18n.t('admin.productSaved'));
      }
    } catch {
      notificationStore.error(i18n.t('admin.errorSaving'));
    } finally {
      saving = false;
    }
  }

  async function deleteProduct() {
    try {
      await catalogApi.deleteProduct(productId);
      notificationStore.success(i18n.t('admin.productDeleted'));
      goto('/admin/products');
    } catch {
      notificationStore.error(i18n.t('admin.errorDeleting'));
    }
  }

  function toggleSize(id: string) {
    const next = new Set(selectedSizeIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedSizeIds = next;
    regenerateInventory();
  }

  function toggleColor(id: string) {
    const next = new Set(selectedColorIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedColorIds = next;
    regenerateInventory();
  }

  function toggleMaterial(id: string) {
    const next = new Set(selectedMaterialIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedMaterialIds = next;
    regenerateInventory();
  }

  /** Build cartesian product of selected variants, preserving existing prices/quantities */
  function regenerateInventory() {
    const sizes = [...selectedSizeIds];
    const colors = [...selectedColorIds];
    const materials = [...selectedMaterialIds];

    // Build lookup for existing inventory to preserve price/quantity
    const key = (s?: string | null, c?: string | null, m?: string | null) =>
      `${s ?? ''}|${c ?? ''}|${m ?? ''}`;
    const existingMap = new Map<string, ProductInventoryItem>();
    for (const item of inventory) {
      existingMap.set(key(item.sizeId, item.colorId, item.materialId), item);
    }

    const combos: { sizeId?: string | null; colorId?: string | null; materialId?: string | null }[] = [];

    const sArr = sizes.length > 0 ? sizes : [null];
    const cArr = colors.length > 0 ? colors : [null];
    const mArr = materials.length > 0 ? materials : [null];

    for (const s of sArr) {
      for (const c of cArr) {
        for (const m of mArr) {
          combos.push({ sizeId: s, colorId: c, materialId: m });
        }
      }
    }

    // If no variants selected at all, keep a single base row
    if (combos.length === 1 && !combos[0].sizeId && !combos[0].colorId && !combos[0].materialId) {
      const base = existingMap.get(key(null, null, null));
      inventory = base ? [base] : [{ price: price || 0, quantity: quantity || 0 }];
      return;
    }

    inventory = combos.map((combo) => {
      const existing = existingMap.get(key(combo.sizeId, combo.colorId, combo.materialId));
      return existing
        ? { ...existing }
        : { sizeId: combo.sizeId, colorId: combo.colorId, materialId: combo.materialId, price: price || 0, quantity: 0 };
    });
  }

  async function deleteImage(imageId: string) {
    try {
      await catalogApi.deleteProductImage(shopId, productId, imageId);
      photos = photos.filter((img) => img.id !== imageId);
      notificationStore.success(i18n.t('admin.imageDeleted'));
      showDeleteImageModal = false;
      deleteImageTarget = null;
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingImage'));
    }
  }

  function confirmDeleteImage(imageId: string) {
    deleteImageTarget = imageId;
    showDeleteImageModal = true;
  }

  function triggerFileInput() {
    fileInput?.click();
  }

  async function handleDropzoneFile(file: File) {
    if (!shopId) {
      notificationStore.warning(i18n.t('admin.selectShop'));
      return;
    }

    try {
      uploading = true;
      const newPhoto = await catalogApi.uploadProductImage(
        shopId,
        productId,
        file,
        uploadTitle.trim() || undefined,
        uploadIsMain
      );
      photos = [...photos, newPhoto];
      uploadTitle = '';
      uploadIsMain = false;
      notificationStore.success(i18n.t('admin.imageUploaded'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      notificationStore.error(msg || i18n.t('admin.errorUploadingImage'));
    } finally {
      uploading = false;
    }
  }

  async function handleFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!shopId) {
      notificationStore.warning(i18n.t('admin.selectShop'));
      return;
    }

    try {
      uploading = true;
      const newPhoto = await catalogApi.uploadProductImage(
        shopId,
        productId,
        file,
        uploadTitle.trim() || undefined,
        uploadIsMain
      );
      photos = [...photos, newPhoto];
      uploadTitle = '';
      uploadIsMain = false;
      notificationStore.success(i18n.t('admin.imageUploaded'));
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      notificationStore.error(msg || i18n.t('admin.errorUploadingImage'));
    } finally {
      uploading = false;
      if (input) input.value = '';
    }
  }

  function updateInventoryField(index: number, field: 'price' | 'quantity', value: string) {
    const numValue = parseFloat(value) || 0;
    inventory = inventory.map((item, i) => {
      if (i !== index) return item;
      if (field === 'quantity') return { ...item, quantity: Math.floor(numValue) };
      return { ...item, price: numValue };
    });
  }

  async function saveInventory() {
    try {
      saving = true;
      await catalogApi.updateProductInventory(productId, inventory);
      notificationStore.success(i18n.t('admin.inventorySaved'));
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingInventory'));
    } finally {
      saving = false;
    }
  }

  function getSizeName(id: string): string {
    return allSizes.find((s) => s.id === id)?.name ?? id;
  }

  function getColorName(id: string): string {
    return allColors.find((c) => c.id === id)?.name ?? id;
  }

  function getMaterialName(id: string): string {
    return allMaterials.find((m) => m.id === id)?.name ?? id;
  }

  function updatePhotoVariant(photoId: string, field: 'colorId' | 'materialId', value: string) {
    photos = photos.map((p) => {
      if (p.id !== photoId) return p;
      return { ...p, [field]: value || null };
    });
  }

  async function saveVariationImages() {
    try {
      saving = true;
      const associations = photos.map((p) => ({
        imageId: p.id,
        sizeId: p.sizeId ?? null,
        colorId: p.colorId ?? null,
        materialId: p.materialId ?? null,
      }));
      await catalogApi.associateVariationImages(productId, associations);
      notificationStore.success(i18n.t('admin.variationImagesSaved'));
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingVariationImages'));
    } finally {
      saving = false;
    }
  }
</script>

<div class="product-editor">
  {#if loading}
    <LoadingSpinner message={i18n.t('common.loading')} />
  {:else}
    <div class="editor-header">
      <div class="header-left">
        <a href="/admin/products" class="back-link">← {i18n.t('admin.backToProducts')}</a>
        <h1 class="page-title">{isNew ? i18n.t('admin.newProduct') : i18n.t('admin.editProduct')}</h1>
      </div>
      <div class="header-actions">
        {#if !isNew}
          <a href="/products/{productId}" class="btn btn-outline btn-sm" target="_blank" rel="noopener">🌐 {i18n.t('admin.viewPublicPage')}</a>
        {/if}
        <button class="btn btn-primary" onclick={save} disabled={saving}>
          {saving ? i18n.t('common.saving') : i18n.t('common.save')}
        </button>
      </div>
    </div>

    <div class="tabs">
      <button
        class="tab"
        class:active={activeTab === 'basic'}
        onclick={() => (activeTab = 'basic')}
      >
        {i18n.t('admin.tabBasic')}
      </button>
      <button
        class="tab"
        class:active={activeTab === 'variants'}
        onclick={() => (activeTab = 'variants')}
        disabled={isNew}
      >
        {i18n.t('admin.tabVariants')}
      </button>
      <button
        class="tab"
        class:active={activeTab === 'photos'}
        onclick={() => (activeTab = 'photos')}
        disabled={isNew}
      >
        {i18n.t('admin.tabPhotos')}
      </button>
      <button
        class="tab"
        class:active={activeTab === 'settings'}
        onclick={() => (activeTab = 'settings')}
        disabled={isNew}
      >
        {i18n.t('admin.tabSettings')}
      </button>
    </div>

    <!-- Tab: Basic -->
    {#if activeTab === 'basic'}
      <div class="tab-content card">
        <div class="card-body">
          <div class="form-grid">
            <div class="form-group full">
              <label class="form-label required" for="title">{i18n.t('admin.name')}</label>
              <input id="title" class="input" type="text" bind:value={name} placeholder={i18n.t('admin.productNamePlaceholder')} />
            </div>

            <div class="form-group full">
              <label class="form-label" for="description">{i18n.t('admin.description')}</label>
              <textarea
                id="description"
                class="input"
                rows="6"
                bind:value={description}
                placeholder={i18n.t('admin.productDescriptionPlaceholder')}
              ></textarea>
            </div>

            <div class="form-group">
              <label class="form-label" for="category">{i18n.t('admin.category')}</label>
              <div class="category-picker-field">
                {#if categoryId && selectedCategoryName}
                  <span class="selected-category-name">{selectedCategoryName}</span>
                  <button class="btn btn-outline btn-sm" type="button" onclick={() => (showCategoryModal = true)}>
                    {i18n.t('admin.changeCategory')}
                  </button>
                  <button class="btn btn-outline btn-sm" type="button" onclick={() => (categoryId = '')}>
                    {i18n.t('admin.clearCategory')}
                  </button>
                {:else}
                  <button class="btn btn-outline" type="button" onclick={() => (showCategoryModal = true)}>
                    {i18n.t('admin.selectCategory')}
                  </button>
                {/if}
              </div>
            </div>

            <div class="form-group">
              <label class="form-label" class:required={isNew} for="shop">{i18n.t('admin.shop')}</label>
              <select id="shop" class="input" bind:value={shopId} disabled={!isNew}>
                <option value="">{i18n.t('admin.selectShop')}</option>
                {#each shops as shop}
                  <option value={shop.id}>{shop.name}</option>
                {/each}
              </select>
            </div>

            <div class="form-group">
              <span class="form-label">{i18n.t('admin.status')}</span>
              <label class="toggle-label">
                <input type="checkbox" bind:checked={isActive} />
                <span>{i18n.t('admin.active')}</span>
              </label>
            </div>

            <div class="form-group">
              <span class="form-label">{i18n.t('admin.personalization')}</span>
              <label class="toggle-label">
                <input type="checkbox" bind:checked={allowPersonalization} />
                <span>{i18n.t('admin.allowPersonalization')}</span>
              </label>
            </div>

            <div class="form-group">
              <label class="form-label" for="price">{i18n.t('admin.price')}</label>
              <input id="price" class="input" type="number" min="0" step="0.01" bind:value={price} placeholder="0.00" />
            </div>

            <div class="form-group">
              <label class="form-label" for="quantity">{i18n.t('admin.quantity')}</label>
              <input id="quantity" class="input" type="number" min="0" step="1" bind:value={quantity} placeholder="0" />
            </div>
          </div>
        </div>
      </div>
    {/if}

    <!-- Tab: Variants & Prices -->
    {#if activeTab === 'variants'}
      <div class="tab-content card">
        <div class="card-body">
          <h3 class="section-title">{i18n.t('admin.sizes')}</h3>
          <div class="checkbox-grid">
            {#each allSizes as size}
              <label class="checkbox-item">
                <input
                  type="checkbox"
                  checked={selectedSizeIds.has(size.id)}
                  onchange={() => toggleSize(size.id)}
                />
                <span>{size.name}</span>
                <span class="badge badge-outline">{size.gender}</span>
              </label>
            {/each}
          </div>

          <h3 class="section-title mt-6">{i18n.t('admin.colors')}</h3>
          <div class="checkbox-grid">
            {#each allColors as color}
              <label class="checkbox-item">
                <input
                  type="checkbox"
                  checked={selectedColorIds.has(color.id)}
                  onchange={() => toggleColor(color.id)}
                />
                <span
                  class="color-swatch"
                  style="background: {color.hexCode}"
                ></span>
                <span>{color.name}</span>
              </label>
            {/each}
          </div>

          <h3 class="section-title mt-6">{i18n.t('admin.materials')}</h3>
          <div class="checkbox-grid">
            {#each allMaterials as material}
              <label class="checkbox-item">
                <input
                  type="checkbox"
                  checked={selectedMaterialIds.has(material.id)}
                  onchange={() => toggleMaterial(material.id)}
                />
                <span>{material.name}</span>
              </label>
            {/each}
          </div>

          {#if inventory.length > 0}
            <h3 class="section-title mt-6">{i18n.t('admin.priceMatrix')}</h3>
            <div class="table-wrapper">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>{i18n.t('admin.size')}</th>
                    <th>{i18n.t('admin.color')}</th>
                    <th>{i18n.t('admin.material')}</th>
                    <th>{i18n.t('admin.price')}</th>
                    <th>{i18n.t('admin.quantity')}</th>
                  </tr>
                </thead>
                <tbody>
                  {#each inventory as item, i}
                    <tr>
                      <td>
                        {#if item.sizeId}
                          {#if authStore.isAdmin}
                            <a href="/admin/sizes" class="dict-link">{getSizeName(item.sizeId)}</a>
                          {:else}
                            {getSizeName(item.sizeId)}
                          {/if}
                        {:else}—{/if}
                      </td>
                      <td>
                        {#if item.colorId}
                          {#if authStore.isAdmin}
                            <a href="/admin/colors" class="dict-link">{getColorName(item.colorId)}</a>
                          {:else}
                            {getColorName(item.colorId)}
                          {/if}
                        {:else}—{/if}
                      </td>
                      <td>
                        {#if item.materialId}
                          {#if authStore.isAdmin}
                            <a href="/admin/materials" class="dict-link">{getMaterialName(item.materialId)}</a>
                          {:else}
                            {getMaterialName(item.materialId)}
                          {/if}
                        {:else}—{/if}
                      </td>
                      <td>
                        <input
                          class="input input-sm price-input"
                          type="number"
                          step="0.01"
                          min="0"
                          value={item.price}
                          oninput={(e) =>
                            updateInventoryField(i, 'price', e.currentTarget.value)}
                        />
                      </td>
                      <td>
                        <input
                          class="input input-sm price-input"
                          type="number"
                          step="1"
                          min="0"
                          value={item.quantity}
                          oninput={(e) =>
                            updateInventoryField(i, 'quantity', e.currentTarget.value)}
                        />
                      </td>
                    </tr>
                  {/each}
                </tbody>
              </table>
            </div>
            <div class="mt-4">
              <button class="btn btn-primary" onclick={saveInventory} disabled={saving}>
                {i18n.t('admin.saveInventory')}
              </button>
            </div>
          {:else}
            <p class="text-muted mt-4">
              {i18n.t('admin.saveProductToCreatePriceMatrix')}
            </p>
          {/if}
        </div>
      </div>
    {/if}

    <!-- Tab: Photos -->
    {#if activeTab === 'photos'}
      <div class="tab-content card">
        <div class="card-body">
          <div class="photos-header">
            <h3 class="section-title">{i18n.t('admin.productPhotos')}</h3>
          </div>

          <!-- Upload form -->
          <div class="upload-area">
            <FileUploadDropzone
              accept="image/*"
              uploading={uploading}
              onfile={handleDropzoneFile}
            />
            <div class="upload-options">
              <input
                class="input input-sm"
                type="text"
                placeholder="Image title (optional)"
                bind:value={uploadTitle}
                style="max-width: 200px;"
              />
              <label class="toggle-label text-sm">
                <input type="checkbox" bind:checked={uploadIsMain} />
                <span>{i18n.t('admin.main')}</span>
              </label>
            </div>
          </div>

          {#if photos.length > 0}
            <div class="photos-grid">
              {#each photos as img}
                <div class="photo-card" class:main={img.isMain}>
                  {#if img.isMain}
                    <span class="main-badge">{i18n.t('admin.main')}</span>
                  {/if}
                  <img src={img.url} alt={img.altText ?? 'Product'} class="photo-img" loading="lazy" />
                  <div class="photo-meta">
                    {#if img.sortOrder}
                      <span class="text-xs text-muted">{i18n.t('admin.sortOrder')}: {img.sortOrder}</span>
                    {/if}
                    <label class="photo-variant-label">
                      <span class="text-xs">{i18n.t('admin.color')}:</span>
                      <select
                        class="input input-xs"
                        value={img.colorId ?? ''}
                        onchange={(e) => updatePhotoVariant(img.id, 'colorId', e.currentTarget.value)}
                      >
                        <option value="">—</option>
                        {#each allColors as color}
                          <option value={color.id}>{color.name}</option>
                        {/each}
                      </select>
                    </label>
                    <label class="photo-variant-label">
                      <span class="text-xs">{i18n.t('admin.material')}:</span>
                      <select
                        class="input input-xs"
                        value={img.materialId ?? ''}
                        onchange={(e) => updatePhotoVariant(img.id, 'materialId', e.currentTarget.value)}
                      >
                        <option value="">—</option>
                        {#each allMaterials as material}
                          <option value={material.id}>{material.name}</option>
                        {/each}
                      </select>
                    </label>
                  </div>
                  <button
                    class="btn btn-sm btn-danger-text photo-delete"
                    onclick={() => confirmDeleteImage(img.id)}
                  >
                    {i18n.t('common.delete')}
                  </button>
                </div>
              {/each}
            </div>
            <div class="mt-4">
              <button class="btn btn-primary" onclick={saveVariationImages} disabled={saving}>
                {i18n.t('admin.saveVariantBindings')}
              </button>
            </div>
          {:else}
            <p class="text-muted mt-4">{i18n.t('admin.noImagesYet')}</p>
          {/if}
        </div>
      </div>
    {/if}

    <!-- Tab: Settings -->
    {#if activeTab === 'settings'}
      <div class="tab-content card">
        <div class="card-body">
          {#if !isNew}
            <div class="danger-zone">
              <h3 class="danger-title">{i18n.t('admin.dangerZone')}</h3>
              <p class="text-sm text-muted mb-4">
                {i18n.t('admin.deleteProductWarning')}
              </p>
              <button
                class="btn btn-danger"
                onclick={() => (showDeleteModal = true)}
              >
                {i18n.t('admin.deleteProduct')}
              </button>
            </div>
          {/if}
        </div>
      </div>
    {/if}
  {/if}
</div>

<Modal
  open={showDeleteModal}
  title={i18n.t('admin.deleteProductQuestion')}
  onclose={() => (showDeleteModal = false)}
>
  <p>{i18n.t('admin.confirmDeleteProduct')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>
      {i18n.t('common.cancel')}
    </button>
    <button class="btn btn-danger" onclick={deleteProduct}>{i18n.t('admin.deleteForever')}</button>
  </div>
</Modal>

<Modal
  open={showDeleteImageModal}
  title={i18n.t('admin.deleteImage')}
  onclose={() => (showDeleteImageModal = false)}
>
  <p>{i18n.t('admin.confirmDeleteImage')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteImageModal = false)}>
      {i18n.t('common.cancel')}
    </button>
    <button class="btn btn-danger" onclick={() => deleteImageTarget && deleteImage(deleteImageTarget)}>
      {i18n.t('common.delete')}
    </button>
  </div>
</Modal>

<CategoryPickerModal
  open={showCategoryModal}
  selectedCategoryId={categoryId}
  onclose={() => (showCategoryModal = false)}
  onselect={(cat) => {
    categoryId = cat.id;
    showCategoryModal = false;
  }}
/>

<style>
  .editor-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    margin-bottom: var(--space-4);
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .header-left {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .back-link {
    font-size: 0.8125rem;
    color: var(--color-text-light);
    text-decoration: none;
  }

  .back-link:hover {
    color: var(--color-primary);
  }

  .page-title {
    font-size: 1.75rem;
  }

  .header-actions {
    display: flex;
    gap: var(--space-3);
    align-items: center;
  }

  .tabs {
    display: flex;
    gap: 0;
    border-bottom: 2px solid var(--color-border);
    margin-bottom: var(--space-4);
  }

  .tab {
    padding: var(--space-3) var(--space-5);
    font-size: 0.875rem;
    font-weight: 500;
    border: none;
    background: none;
    cursor: pointer;
    color: var(--color-text-light);
    border-bottom: 2px solid transparent;
    margin-bottom: -2px;
    transition: all var(--transition-fast);
    position: relative;
  }

  .tab:hover:not(:disabled) {
    color: var(--color-text);
    background: var(--color-surface-hover);
  }

  .tab.active {
    color: var(--color-primary);
    border-bottom-color: var(--color-primary);
    font-weight: 600;
  }

  .tab:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  .tab-content {
    animation: fadeIn 200ms ease;
  }

  .form-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-4);
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .form-group.full {
    grid-column: 1 / -1;
  }

  .form-label {
    font-size: 0.8125rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .required::after {
    content: ' *';
    color: var(--color-error, #c4555a);
    font-weight: 700;
  }

  .toggle-label {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    cursor: pointer;
    font-size: 0.875rem;
  }

  .category-picker-field {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    flex-wrap: wrap;
  }

  .selected-category-name {
    font-size: 0.875rem;
    color: var(--color-text);
    font-weight: 500;
    padding: var(--space-2) 0;
  }

  .section-title {
    font-size: 1rem;
    font-weight: 600;
    margin-bottom: var(--space-3);
    color: var(--color-text);
    font-family: var(--font-body);
  }

  .checkbox-grid {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .checkbox-item {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-2) var(--space-3);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    cursor: pointer;
    font-size: 0.875rem;
    transition: border-color var(--transition-fast);
  }

  .checkbox-item:hover {
    border-color: var(--color-primary-light);
  }

  .color-swatch {
    width: 16px;
    height: 16px;
    border-radius: var(--radius-full);
    border: 1px solid var(--color-border);
    flex-shrink: 0;
  }

  .table-wrapper {
    overflow-x: auto;
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
  }

  .data-table {
    width: 100%;
    font-size: 0.875rem;
  }

  .data-table th {
    text-align: left;
    padding: var(--space-2) var(--space-3);
    font-weight: 600;
    color: var(--color-text-light);
    border-bottom: 2px solid var(--color-border);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    white-space: nowrap;
  }

  .data-table td {
    padding: var(--space-2) var(--space-3);
    border-bottom: 1px solid var(--color-border-light);
    vertical-align: middle;
  }

  .price-input {
    width: 100px;
  }

  .photos-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-4);
  }

  .photos-header .section-title {
    margin-bottom: 0;
  }

  .upload-area {
    margin-bottom: var(--space-5);
    padding: var(--space-4);
    border: 2px dashed var(--color-border);
    border-radius: var(--radius-md);
    background: var(--color-surface-hover);
  }

  .upload-options {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    flex-wrap: wrap;
    margin-top: var(--space-3);
  }

  .main-badge {
    position: absolute;
    top: var(--space-2);
    left: var(--space-2);
    background: var(--color-primary);
    color: #fff;
    font-size: 0.65rem;
    font-weight: 600;
    padding: 2px 8px;
    border-radius: 999px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    z-index: 1;
  }

  .dict-link {
    color: var(--color-primary);
    text-decoration: none;
    font-size: 0.8125rem;
  }

  .dict-link:hover {
    text-decoration: underline;
  }

  .photos-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
    gap: var(--space-4);
  }

  .photo-card {
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
    overflow: hidden;
    position: relative;
  }

  .photo-card.main {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 2px rgba(139, 94, 107, 0.15);
  }

  .photo-img {
    width: 100%;
    aspect-ratio: 1;
    object-fit: cover;
  }

  .photo-meta {
    padding: var(--space-2) var(--space-3);
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .photo-variant-label {
    display: flex;
    align-items: center;
    gap: var(--space-1);
    font-size: 0.75rem;
  }

  .photo-variant-label select {
    flex: 1;
    font-size: 0.75rem;
    padding: 2px 4px;
  }

  .photo-delete {
    width: 100%;
    border-radius: 0;
  }

  .danger-zone {
    border: 1px solid var(--color-error);
    border-radius: var(--radius-md);
    padding: var(--space-6);
    background: rgba(196, 85, 90, 0.03);
  }

  .danger-title {
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-error);
    margin-bottom: var(--space-2);
    font-family: var(--font-body);
  }

  .btn-danger {
    background: var(--color-error);
    color: #fff;
    border: none;
  }

  .btn-danger:hover {
    opacity: 0.9;
  }

  .btn-danger-text {
    color: var(--color-error);
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }

  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }

  @media (max-width: 768px) {
    .form-grid {
      grid-template-columns: 1fr;
    }
    .photos-grid {
      grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    }
  }
</style>
