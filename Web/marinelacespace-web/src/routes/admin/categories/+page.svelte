<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
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
      if (cat.childCategories?.length) {
        result = result.concat(flattenForSelect(cat.childCategories, depth + 1, exclude));
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
      notificationStore.error('Помилка завантаження категорій');
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
      notificationStore.warning('Введіть назву категорії');
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createCategory({
          name: modalName.trim(),
          parentCategoryId: modalParentId ?? undefined,
        });
        notificationStore.success('Категорію створено');
      } else if (editingId) {
        await catalogApi.updateCategory(editingId, { name: modalName.trim() });
        notificationStore.success('Категорію оновлено');
      }
      showModal = false;
      loadCategories();
    } catch {
      notificationStore.error('Помилка збереження категорії');
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteCategory(deleteTarget.id);
      notificationStore.success('Категорію видалено');
      showDeleteModal = false;
      deleteTarget = null;
      loadCategories();
    } catch {
      notificationStore.error('Помилка видалення категорії. Можливо, вона містить товари.');
    }
  }
</script>

<div class="categories-page">
  <div class="page-header">
    <h1 class="page-title">Категорії</h1>
    <button class="btn btn-primary" on:click={openCreateRoot}>
      Додати кореневу категорію
    </button>
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження категорій..." />
  {:else if categories.length === 0}
    <div class="empty-state-box">
      <p class="text-muted">Категорій ще немає.</p>
      <button class="btn btn-primary mt-4" on:click={openCreateRoot}>
        Створити першу категорію
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
                class:has-children={cat.childCategories?.length > 0}
                on:click={() => toggleExpand(cat.id)}
                aria-label={expandedIds.has(cat.id) ? 'Згорнути' : 'Розгорнути'}
              >
                {#if cat.childCategories?.length > 0}
                  <span class="expand-icon" class:expanded={expandedIds.has(cat.id)}>▶</span>
                {:else}
                  <span class="expand-icon placeholder">·</span>
                {/if}
              </button>
              <span class="node-name">{cat.name}</span>
              <span class="badge badge-outline node-level">Рівень {cat.level}</span>
              <span class="text-xs text-muted">{cat.productCount} товарів</span>
              <div class="node-actions">
                <button class="btn btn-sm btn-ghost" on:click={() => openEdit(cat)}>
                  Редагувати
                </button>
                <button class="btn btn-sm btn-ghost" on:click={() => openCreateChild(cat.id)}>
                  + Дочірня
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  on:click={() => confirmDelete(cat)}
                  disabled={cat.productCount > 0}
                  title={cat.productCount > 0 ? 'Не можна видалити: є товари' : ''}
                >
                  Видалити
                </button>
              </div>
            </div>

            {#if cat.childCategories?.length > 0 && expandedIds.has(cat.id)}
              {#each cat.childCategories as child}
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
  title={modalMode === 'create' ? 'Нова категорія' : 'Редагувати категорію'}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="catName">Назва</label>
    <input
      id="catName"
      class="input"
      type="text"
      bind:value={modalName}
      placeholder="Назва категорії"
    />
  </div>
  {#if modalMode === 'create'}
    <div class="form-group mt-4">
      <label class="form-label" for="catParent">Батьківська категорія</label>
      <select id="catParent" class="input" bind:value={modalParentId}>
        <option value={null}>Коренева (без батьківської)</option>
        {#each flatCategoriesForSelect as opt}
          <option value={opt.id}>{opt.name}</option>
        {/each}
      </select>
    </div>
  {/if}
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showModal = false)}>Скасувати</button>
    <button class="btn btn-primary" on:click={saveCategory} disabled={saving}>
      {saving ? 'Збереження...' : 'Зберегти'}
    </button>
  </div>
</Modal>

<Modal
  open={showDeleteModal}
  title="Видалити категорію?"
  onclose={() => (showDeleteModal = false)}
>
  <p>Ви впевнені, що хочете видалити категорію <strong>{deleteTarget?.name}</strong>?</p>
  <p class="text-sm text-muted mt-2">Дочірні категорії також будуть видалені.</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showDeleteModal = false)}>Скасувати</button>
    <button class="btn btn-danger" on:click={executeDelete}>Видалити</button>
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
