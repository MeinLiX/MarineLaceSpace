<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Category } from '$types';

  let loading = $state(true);
  let categories = $state<Category[]>([]);
  let expandedIds = $state<Set<string>>(new Set());

  // Modal state
  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let modalParentId = $state<string | null>(null);
  let saving = $state(false);

  // Delete state
  let showDeleteModal = $state(false);
  let deleteTarget = $state<Category | null>(null);

  let flatCategoriesForSelect = $derived(flattenForSelect(categories));

  function flattenForSelect(
    cats: Category[],
    depth = 0,
    exclude?: string
  ): { id: string; name: string }[] {
    let result: { id: string; name: string }[] = [];
    for (const cat of cats) {
      if (cat.id === exclude) continue;
      result.push({ id: cat.id, name: '\u2003'.repeat(depth) + cat.name });
      if (cat.subcategories?.length) {
        result = result.concat(flattenForSelect(cat.subcategories, depth + 1, exclude));
      }
    }
    return result;
  }

  $effect(() => {
    loadCategories();
  });

  async function loadCategories() {
    try {
      loading = true;
      categories = await catalogApi.getCategoryTree();
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingCategories'));
    } finally {
      loading = false;
    }
  }

  function toggleExpand(id: string) {
    const next = new Set(expandedIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    expandedIds = next;
  }

  function openCreateRoot() {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    modalParentId = null;
    showModal = true;
  }

  function openCreateChild(parentId: string) {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    modalParentId = parentId;
    showModal = true;
  }

  function openEdit(cat: Category) {
    modalMode = 'edit';
    editingId = cat.id;
    modalName = cat.name;
    modalParentId = cat.parentCategoryId;
    showModal = true;
  }

  function confirmDelete(cat: Category) {
    deleteTarget = cat;
    showDeleteModal = true;
  }

  async function saveCategory() {
    if (!modalName.trim()) {
      notificationStore.warning(i18n.t('admin.enterCategoryName'));
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createCategory({
          name: modalName.trim(),
          parentCategoryId: modalParentId ?? undefined,
        });
        notificationStore.success(i18n.t('admin.categoryCreated'));
      } else if (editingId) {
        await catalogApi.updateCategory(editingId, { name: modalName.trim() });
        notificationStore.success(i18n.t('admin.categoryUpdated'));
      }
      showModal = false;
      loadCategories();
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingCategory'));
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteCategory(deleteTarget.id);
      notificationStore.success(i18n.t('admin.categoryDeleted'));
      showDeleteModal = false;
      deleteTarget = null;
      loadCategories();
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingCategoryHasProducts'));
    }
  }
</script>

<div class="categories-page">
  <div class="page-header">
    <h1 class="page-title">{i18n.t('admin.categories')}</h1>
    <button class="btn btn-primary" onclick={openCreateRoot}>
      {i18n.t('admin.addRootCategory')}
    </button>
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingCategories')} />
  {:else if categories.length === 0}
    <div class="empty-state-box">
      <p class="text-muted">{i18n.t('admin.noCategoriesYet')}</p>
      <button class="btn btn-primary mt-4" onclick={openCreateRoot}>
        {i18n.t('admin.createFirstCategory')}
      </button>
    </div>
  {:else}
    <div class="tree-container card">
      <div class="card-body">
        {#snippet categoryNode(cat: Category, depth: number)}
          <div class="tree-node" style="padding-left: {depth * 24}px">
            <div class="node-row">
              <button
                class="expand-btn"
                class:has-children={cat.subcategories?.length > 0}
                onclick={() => toggleExpand(cat.id)}
                aria-label={expandedIds.has(cat.id) ? i18n.t('admin.collapse') : i18n.t('admin.expand')}
              >
                {#if cat.subcategories?.length > 0}
                  <span class="expand-icon" class:expanded={expandedIds.has(cat.id)}>▶</span>
                {:else}
                  <span class="expand-icon placeholder">·</span>
                {/if}
              </button>
              <span class="node-name">{cat.name}</span>
              <span class="badge badge-outline node-level">{i18n.t('admin.level')} {cat.level}</span>
              <span class="text-xs text-muted">{cat.productCount} {i18n.t('admin.productsCount')}</span>
              <div class="node-actions">
                <button class="btn btn-sm btn-ghost" onclick={() => openEdit(cat)}>
                  {i18n.t('common.edit')}
                </button>
                <button class="btn btn-sm btn-ghost" onclick={() => openCreateChild(cat.id)}>
                  + {i18n.t('admin.child')}
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  onclick={() => confirmDelete(cat)}
                  disabled={(cat.productCount ?? 0) > 0}
                  title={(cat.productCount ?? 0) > 0 ? i18n.t('admin.cannotDeleteHasProducts') : ''}
                >
                  {i18n.t('common.delete')}
                </button>
              </div>
            </div>

            {#if cat.subcategories?.length > 0 && expandedIds.has(cat.id)}
              {#each cat.subcategories as child}
                {@render categoryNode(child, depth + 1)}
              {/each}
            {/if}
          </div>
        {/snippet}

        {#each categories as cat}
          {@render categoryNode(cat, 0)}
        {/each}
      </div>
    </div>
  {/if}
</div>

<Modal
  open={showModal}
  title={modalMode === 'create' ? i18n.t('admin.newCategory') : i18n.t('admin.editCategory')}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="catName">{i18n.t('admin.name')}</label>
    <input
      id="catName"
      class="input"
      type="text"
      bind:value={modalName}
      placeholder={i18n.t('admin.categoryNamePlaceholder')}
    />
  </div>
  {#if modalMode === 'create'}
    <div class="form-group mt-4">
      <label class="form-label" for="catParent">{i18n.t('admin.parentCategory')}</label>
      <select id="catParent" class="input" bind:value={modalParentId}>
        <option value={null}>{i18n.t('admin.rootNoParent')}</option>
        {#each flatCategoriesForSelect as opt}
          <option value={opt.id}>{opt.name}</option>
        {/each}
      </select>
    </div>
  {/if}
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-primary" onclick={saveCategory} disabled={saving}>
      {saving ? i18n.t('common.saving') : i18n.t('common.save')}
    </button>
  </div>
</Modal>

<Modal
  open={showDeleteModal}
  title={i18n.t('admin.deleteCategoryQuestion')}
  onclose={() => (showDeleteModal = false)}
>
  <p>{i18n.t('admin.confirmDeleteCategory', { name: deleteTarget?.name ?? '' })}</p>
  <p class="text-sm text-muted mt-2">{i18n.t('admin.subcategoriesAlsoDeleted')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-danger" onclick={executeDelete}>{i18n.t('common.delete')}</button>
  </div>
</Modal>

<style>
  .page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
  }

  .page-title {
    font-size: 1.75rem;
  }

  .empty-state-box {
    text-align: center;
    padding: var(--space-16);
  }

  .tree-container {
    max-width: 900px;
  }

  .tree-node {
    border-bottom: 1px solid var(--color-border-light);
  }

  .tree-node:last-child {
    border-bottom: none;
  }

  .node-row {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-2);
  }

  .node-row:hover {
    background: var(--color-surface-hover);
  }

  .expand-btn {
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    background: none;
    cursor: pointer;
    border-radius: var(--radius-sm);
    flex-shrink: 0;
  }

  .expand-btn.has-children:hover {
    background: var(--color-border-light);
  }

  .expand-icon {
    font-size: 0.625rem;
    transition: transform var(--transition-fast);
    display: inline-block;
  }

  .expand-icon.expanded {
    transform: rotate(90deg);
  }

  .expand-icon.placeholder {
    color: var(--color-text-muted);
  }

  .node-name {
    font-weight: 500;
    font-size: 0.9375rem;
    flex: 1;
  }

  .node-level {
    font-size: 0.6875rem;
  }

  .node-actions {
    display: flex;
    gap: var(--space-1);
    opacity: 0;
    transition: opacity var(--transition-fast);
  }

  .node-row:hover .node-actions {
    opacity: 1;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .form-label {
    font-size: 0.8125rem;
    font-weight: 600;
  }

  .btn-danger {
    background: var(--color-error);
    color: #fff;
    border: none;
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
</style>
