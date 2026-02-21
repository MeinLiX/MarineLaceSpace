<script lang="ts">
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
  import type {
    ProductDetail,
    Category,
    Size,
    Color,
    Material,
    ProductPrice,
    ProductImage,
  } from '$types';

  let productId = $derived($page.params.id);
  let isNew = $derived(productId === 'new');
  let loading = $state(true);
  let saving = $state(false);
  let activeTab = $state<'basic' | 'variants' | 'photos' | 'settings'>('basic');

  // Form state — basic
  let title = $state('');
  let description = $state('');
  let categoryId = $state('');
  let shopId = $state('');
  let status = $state<'Draft' | 'Active' | 'Inactive'>('Draft');
  let isPersonalizationEnabled = $state(false);
  let personalizationPrompt = $state('');

  // Variants
  let selectedSizeIds = $state<Set<string>>(new Set());
  let selectedColorIds = $state<Set<string>>(new Set());
  let selectedMaterialIds = $state<Set<string>>(new Set());
  let prices = $state<ProductPrice[]>([]);

  // Photos
  let images = $state<ProductImage[]>([]);

  // Dictionaries
  let categories = $state<Category[]>([]);
  let allSizes = $state<Size[]>([]);
  let allColors = $state<Color[]>([]);
  let allMaterials = $state<Material[]>([]);

  // Shops for select
  let shops = $state<{ id: string; name: string }[]>([]);

  // Delete
  let showDeleteModal = $state(false);

  let flatCategories = $derived(flattenCategories(categories));

  function flattenCategories(
    cats: Category[],
    depth = 0
  ): { id: string; name: string; indent: string }[] {
    let result: { id: string; name: string; indent: string }[] = [];
    for (const cat of cats) {
      result.push({ id: cat.id, name: cat.name, indent: '\u2003'.repeat(depth) });
      if (cat.childCategories?.length) {
        result = result.concat(flattenCategories(cat.childCategories, depth + 1));
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
      const [catsRes, sizesRes, colorsRes, materialsRes, shopsRes] = await Promise.all([
        catalogApi.getCategoryTree(),
        catalogApi.getSizes(),
        catalogApi.getColors(),
        catalogApi.getMaterials(),
        catalogApi.getShops({ pageSize: 100 }),
      ]);

      categories = catsRes;
      allSizes = sizesRes;
      allColors = colorsRes;
      allMaterials = materialsRes;
      shops = shopsRes.items.map((s) => ({ id: s.id, name: s.name }));

      if (id !== 'new') {
        const product = await catalogApi.getProductById(id);
        title = product.title;
        description = product.description;
        categoryId = product.categoryId;
        shopId = product.shopId;
        status = product.status as 'Draft' | 'Active' | 'Inactive';
        isPersonalizationEnabled = product.isPersonalizationEnabled;
        personalizationPrompt = product.personalizationPrompt ?? '';
        selectedSizeIds = new Set(product.sizes.map((s) => s.sizeId));
        selectedColorIds = new Set(product.colors.map((c) => c.colorId));
        selectedMaterialIds = new Set(product.materials.map((m) => m.materialId));
        prices = product.prices;
        images = product.images;
      }
    } catch {
      notificationStore.error('Помилка завантаження даних');
    } finally {
      loading = false;
    }
  }

  async function save() {
    if (!title.trim()) {
      notificationStore.warning('Введіть назву товару');
      return;
    }
    try {
      saving = true;
      const data: Partial<ProductDetail> = {
        title,
        description,
        categoryId,
        shopId,
        status: status as any,
        isPersonalizationEnabled,
        personalizationPrompt: personalizationPrompt || null,
      };

      if (isNew) {
        if (!shopId) {
          notificationStore.warning('Оберіть магазин');
          return;
        }
        const created = await catalogApi.createProduct(shopId, data);
        notificationStore.success('Товар створено');
        goto(`/admin/products/${created.id}`);
      } else {
        await catalogApi.updateProduct(productId, data);
        notificationStore.success('Товар збережено');
      }
    } catch {
      notificationStore.error('Помилка збереження');
    } finally {
      saving = false;
    }
  }

  async function deleteProduct() {
    try {
      await catalogApi.deleteProduct(productId);
      notificationStore.success('Товар видалено');
      goto('/admin/products');
    } catch {
      notificationStore.error('Помилка видалення');
    }
  }

  function toggleSize(id: string) {
    const next = new Set(selectedSizeIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedSizeIds = next;
  }

  function toggleColor(id: string) {
    const next = new Set(selectedColorIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedColorIds = next;
  }

  function toggleMaterial(id: string) {
    const next = new Set(selectedMaterialIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    selectedMaterialIds = next;
  }

  async function deleteImage(imageId: string) {
    try {
      await catalogApi.deleteProductImage(productId, imageId);
      images = images.filter((img) => img.id !== imageId);
      notificationStore.success('Зображення видалено');
    } catch {
      notificationStore.error('Помилка видалення зображення');
    }
  }

  function updatePriceField(index: number, field: 'basePrice' | 'oldPrice' | 'quantity', value: string) {
    const numValue = parseFloat(value) || 0;
    prices = prices.map((p, i) => {
      if (i !== index) return p;
      if (field === 'oldPrice') return { ...p, oldPrice: numValue || null };
      if (field === 'quantity') return { ...p, quantity: Math.floor(numValue) };
      return { ...p, basePrice: numValue };
    });
  }

  async function saveInventory() {
    try {
      saving = true;
      for (const price of prices) {
        await catalogApi.updatePrice(productId, price.id, {
          basePrice: price.basePrice,
          oldPrice: price.oldPrice ?? undefined,
        });
        await catalogApi.updateInventory(productId, price.id, {
          quantity: price.quantity,
        });
      }
      notificationStore.success('Інвентар збережено');
    } catch {
      notificationStore.error('Помилка збереження інвентарю');
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
</script>

<div class="product-editor">
  {#if loading}
    <LoadingSpinner message="Завантаження..." />
  {:else}
    <div class="editor-header">
      <h1 class="page-title">{isNew ? 'Новий товар' : 'Редагування товару'}</h1>
      <div class="header-actions">
        <a href="/admin/products" class="btn btn-outline">Скасувати</a>
        <button class="btn btn-primary" on:click={save} disabled={saving}>
          {saving ? 'Збереження...' : 'Зберегти'}
        </button>
      </div>
    </div>

    <div class="tabs">
      <button
        class="tab"
        class:active={activeTab === 'basic'}
        on:click={() => (activeTab = 'basic')}
      >
        Основне
      </button>
      <button
        class="tab"
        class:active={activeTab === 'variants'}
        on:click={() => (activeTab = 'variants')}
        disabled={isNew}
      >
        Варіації та ціни
      </button>
      <button
        class="tab"
        class:active={activeTab === 'photos'}
        on:click={() => (activeTab = 'photos')}
        disabled={isNew}
      >
        Фотографії
      </button>
      <button
        class="tab"
        class:active={activeTab === 'settings'}
        on:click={() => (activeTab = 'settings')}
        disabled={isNew}
      >
        Налаштування
      </button>
    </div>

    <!-- Tab: Basic -->
    {#if activeTab === 'basic'}
      <div class="tab-content card">
        <div class="card-body">
          <div class="form-grid">
            <div class="form-group full">
              <label class="form-label" for="title">Назва</label>
              <input id="title" class="input" type="text" bind:value={title} placeholder="Назва товару" />
            </div>

            <div class="form-group full">
              <label class="form-label" for="description">Опис</label>
              <textarea
                id="description"
                class="input"
                rows="6"
                bind:value={description}
                placeholder="Детальний опис товару..."
              ></textarea>
            </div>

            <div class="form-group">
              <label class="form-label" for="category">Категорія</label>
              <select id="category" class="input" bind:value={categoryId}>
                <option value="">Оберіть категорію</option>
                {#each flatCategories as cat}
                  <option value={cat.id}>{cat.indent}{cat.name}</option>
                {/each}
              </select>
            </div>

            <div class="form-group">
              <label class="form-label" for="shop">Магазин</label>
              <select id="shop" class="input" bind:value={shopId} disabled={!isNew}>
                <option value="">Оберіть магазин</option>
                {#each shops as shop}
                  <option value={shop.id}>{shop.name}</option>
                {/each}
              </select>
            </div>

            <div class="form-group">
              <label class="form-label" for="status">Статус</label>
              <select id="status" class="input" bind:value={status}>
                <option value="Draft">Чернетка</option>
                <option value="Active">Активний</option>
                <option value="Inactive">Неактивний</option>
              </select>
            </div>

            <div class="form-group">
              <label class="form-label">Персоналізація</label>
              <label class="toggle-label">
                <input type="checkbox" bind:checked={isPersonalizationEnabled} />
                <span>Дозволити персоналізацію</span>
              </label>
            </div>

            {#if isPersonalizationEnabled}
              <div class="form-group full">
                <label class="form-label" for="personalizationPrompt">
                  Текст підказки для персоналізації
                </label>
                <textarea
                  id="personalizationPrompt"
                  class="input"
                  rows="3"
                  bind:value={personalizationPrompt}
                  placeholder="Наприклад: Введіть текст для гравіювання..."
                ></textarea>
              </div>
            {/if}
          </div>
        </div>
      </div>
    {/if}

    <!-- Tab: Variants & Prices -->
    {#if activeTab === 'variants'}
      <div class="tab-content card">
        <div class="card-body">
          <h3 class="section-title">Розміри</h3>
          <div class="checkbox-grid">
            {#each allSizes as size}
              <label class="checkbox-item">
                <input
                  type="checkbox"
                  checked={selectedSizeIds.has(size.id)}
                  on:change={() => toggleSize(size.id)}
                />
                <span>{size.name}</span>
                <span class="badge badge-outline">{size.gender}</span>
              </label>
            {/each}
          </div>

          <h3 class="section-title mt-6">Кольори</h3>
          <div class="checkbox-grid">
            {#each allColors as color}
              <label class="checkbox-item">
                <input
                  type="checkbox"
                  checked={selectedColorIds.has(color.id)}
                  on:change={() => toggleColor(color.id)}
                />
                <span
                  class="color-swatch"
                  style="background: {color.hexCode}"
                ></span>
                <span>{color.name}</span>
              </label>
            {/each}
          </div>

          <h3 class="section-title mt-6">Матеріали</h3>
          <div class="checkbox-grid">
            {#each allMaterials as material}
              <label class="checkbox-item">
                <input
                  type="checkbox"
                  checked={selectedMaterialIds.has(material.id)}
                  on:change={() => toggleMaterial(material.id)}
                />
                <span>{material.name}</span>
              </label>
            {/each}
          </div>

          {#if prices.length > 0}
            <h3 class="section-title mt-6">Цінова матриця</h3>
            <div class="table-wrapper">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Розмір</th>
                    <th>Колір</th>
                    <th>Матеріал</th>
                    <th>Ціна (₴)</th>
                    <th>Стара ціна (₴)</th>
                    <th>Кількість</th>
                  </tr>
                </thead>
                <tbody>
                  {#each prices as price, i}
                    <tr>
                      <td>{getSizeName(price.sizeId)}</td>
                      <td>{getColorName(price.colorId)}</td>
                      <td>{getMaterialName(price.materialId)}</td>
                      <td>
                        <input
                          class="input input-sm price-input"
                          type="number"
                          step="0.01"
                          min="0"
                          value={price.basePrice}
                          on:input={(e) =>
                            updatePriceField(i, 'basePrice', e.currentTarget.value)}
                        />
                      </td>
                      <td>
                        <input
                          class="input input-sm price-input"
                          type="number"
                          step="0.01"
                          min="0"
                          value={price.oldPrice ?? ''}
                          on:input={(e) =>
                            updatePriceField(i, 'oldPrice', e.currentTarget.value)}
                        />
                      </td>
                      <td>
                        <input
                          class="input input-sm price-input"
                          type="number"
                          step="1"
                          min="0"
                          value={price.quantity}
                          on:input={(e) =>
                            updatePriceField(i, 'quantity', e.currentTarget.value)}
                        />
                      </td>
                    </tr>
                  {/each}
                </tbody>
              </table>
            </div>
            <div class="mt-4">
              <button class="btn btn-primary" on:click={saveInventory} disabled={saving}>
                Зберегти інвентар
              </button>
            </div>
          {:else}
            <p class="text-muted mt-4">
              Збережіть товар і додайте варіації, щоб створити цінову матрицю.
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
            <h3 class="section-title">Фотографії товару</h3>
            <button class="btn btn-outline btn-sm" disabled>
              Завантажити зображення
            </button>
          </div>

          {#if images.length > 0}
            <div class="photos-grid">
              {#each images as img}
                <div class="photo-card" class:main={img.isMain}>
                  <img src={img.url} alt={img.altText ?? 'Product'} class="photo-img" />
                  <div class="photo-meta">
                    <label class="toggle-label text-sm">
                      <input type="checkbox" checked={img.isMain} disabled />
                      <span>Головне</span>
                    </label>
                    <span class="text-xs text-muted">Порядок: {img.sortOrder}</span>
                    {#if img.colorId}
                      <span class="text-xs">Колір: {getColorName(img.colorId)}</span>
                    {/if}
                    {#if img.materialId}
                      <span class="text-xs">Матеріал: {getMaterialName(img.materialId)}</span>
                    {/if}
                  </div>
                  <button
                    class="btn btn-sm btn-danger-text photo-delete"
                    on:click={() => deleteImage(img.id)}
                  >
                    Видалити
                  </button>
                </div>
              {/each}
            </div>
          {:else}
            <p class="text-muted mt-4">Зображень ще немає. Завантажте першу фотографію.</p>
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
              <h3 class="danger-title">Небезпечна зона</h3>
              <p class="text-sm text-muted mb-4">
                Видалення товару є незворотною дією. Усі дані, включаючи зображення та
                варіації, будуть втрачені.
              </p>
              <button
                class="btn btn-danger"
                on:click={() => (showDeleteModal = true)}
              >
                Видалити товар
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
  title="Видалити товар?"
  onclose={() => (showDeleteModal = false)}
>
  <p>Ви впевнені, що хочете видалити цей товар? Усі дані будуть втрачені.</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showDeleteModal = false)}>
      Скасувати
    </button>
    <button class="btn btn-danger" on:click={deleteProduct}>Видалити назавжди</button>
  </div>
</Modal>

<style>
  .editor-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-4);
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .page-title {
    font-size: 1.75rem;
  }

  .header-actions {
    display: flex;
    gap: var(--space-3);
  }

  .tabs {
    display: flex;
    gap: var(--space-1);
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
  }

  .tab:hover:not(:disabled) {
    color: var(--color-text);
  }

  .tab.active {
    color: var(--color-primary);
    border-bottom-color: var(--color-primary);
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

  .toggle-label {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    cursor: pointer;
    font-size: 0.875rem;
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
