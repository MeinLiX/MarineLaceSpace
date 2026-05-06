<script lang="ts">
  import Modal from '$components/Modal.svelte';
  import { i18n } from '$i18n/index.svelte';
  import * as catalogApi from '$api/catalog';
  import type { Category } from '$types';

  let {
    open = false,
    onselect,
    onclose,
    selectedCategoryId = '',
  }: {
    open: boolean;
    onselect: (cat: { id: string; name: string }) => void;
    onclose: () => void;
    selectedCategoryId?: string;
  } = $props();

  let searchQuery = $state('');
  let treeCategories = $state<Category[]>([]);
  let searchResults = $state<Category[]>([]);
  let loadingTree = $state(false);
  let loadingSearch = $state(false);
  let expandedIds = $state<Set<string>>(new Set());
  let searchTimeout: ReturnType<typeof setTimeout> | undefined;

  let isSearchMode = $derived(searchQuery.length >= 3);

  $effect(() => {
    if (open) {
      searchQuery = '';
      searchResults = [];
      loadTree();
    }
  });

  async function loadTree() {
    if (treeCategories.length > 0) return;
    try {
      loadingTree = true;
      treeCategories = await catalogApi.getCategoryTree();
    } catch {
      treeCategories = [];
    } finally {
      loadingTree = false;
    }
  }

  function handleSearchInput() {
    if (searchTimeout) clearTimeout(searchTimeout);
    if (searchQuery.length < 3) {
      searchResults = [];
      return;
    }
    searchTimeout = setTimeout(async () => {
      try {
        loadingSearch = true;
        searchResults = await catalogApi.searchCategories(searchQuery);
      } catch {
        searchResults = [];
      } finally {
        loadingSearch = false;
      }
    }, 300);
  }

  function selectCategory(cat: { id: string; name: string }) {
    onselect(cat);
  }

  function toggleExpand(id: string) {
    const next = new Set(expandedIds);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    expandedIds = next;
  }

  function isExpanded(id: string): boolean {
    return expandedIds.has(id);
  }
</script>

<Modal {open} title={i18n.t('admin.selectCategoryModal')} {onclose}>
  <div class="picker">
    <div class="search-box">
      <svg class="search-icon" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <circle cx="11" cy="11" r="8" />
        <line x1="21" y1="21" x2="16.65" y2="16.65" />
      </svg>
      <input
        class="search-input"
        type="text"
        placeholder={i18n.t('admin.searchCategories')}
        bind:value={searchQuery}
        oninput={handleSearchInput}
      />
    </div>

    <div class="category-list">
      {#if loadingTree || loadingSearch}
        <div class="loading-state">
          <span class="spinner"></span>
          {i18n.t('common.loading')}
        </div>
      {:else if isSearchMode}
        {#if searchResults.length === 0}
          <div class="empty-state">{i18n.t('admin.noCategoriesFound')}</div>
        {:else}
          {#each searchResults as cat}
            <button
              class="category-row flat-result"
              class:selected={cat.id === selectedCategoryId}
              onclick={() => selectCategory({ id: cat.id, name: cat.name })}
            >
              <span class="cat-name">{cat.name}</span>
              {#if cat.fullPath}
                <span class="cat-path">{cat.fullPath}</span>
              {/if}
            </button>
          {/each}
        {/if}
      {:else}
        {#if treeCategories.length === 0}
          <div class="empty-state">{i18n.t('admin.noCategoriesFound')}</div>
        {:else}
          {#each treeCategories as cat}
            {@render treeNode(cat, 0)}
          {/each}
        {/if}
      {/if}
    </div>
  </div>
</Modal>

{#snippet treeNode(cat: Category, depth: number)}
  <div class="tree-item" style="--depth: {depth}">
    <div
      class="category-row"
      class:selected={cat.id === selectedCategoryId}
    >
      {#if cat.subcategories?.length}
        <button
          class="expand-btn"
          onclick={() => toggleExpand(cat.id)}
          aria-label={isExpanded(cat.id) ? i18n.t('common.collapse') : i18n.t('common.expand')}
        >
          <svg
            class="chevron"
            class:rotated={isExpanded(cat.id)}
            viewBox="0 0 24 24"
            width="16"
            height="16"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            aria-hidden="true"
          >
            <polyline points="9 18 15 12 9 6" />
          </svg>
        </button>
      {:else}
        <span class="expand-placeholder"></span>
      {/if}
      <button
        class="cat-select-btn"
        onclick={() => selectCategory({ id: cat.id, name: cat.name })}
      >
        <span class="cat-name">{cat.name}</span>
      </button>
    </div>
    {#if cat.subcategories?.length && isExpanded(cat.id)}
      {#each cat.subcategories as sub}
        {@render treeNode(sub, depth + 1)}
      {/each}
    {/if}
  </div>
{/snippet}

<style>
  .picker {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .search-box {
    position: relative;
    display: flex;
    align-items: center;
  }

  .search-icon {
    position: absolute;
    left: 12px;
    color: var(--color-text-muted);
    pointer-events: none;
  }

  .search-input {
    width: 100%;
    padding: var(--space-3) var(--space-3) var(--space-3) 40px;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    font-size: 0.875rem;
    color: var(--color-text);
    background: var(--color-surface);
    transition: border-color var(--transition-fast);
  }

  .search-input:focus {
    outline: none;
    border-color: var(--color-primary);
    box-shadow: 0 0 0 3px rgba(139, 94, 107, 0.1);
  }

  .search-input::placeholder {
    color: var(--color-text-muted);
  }

  .category-list {
    max-height: 360px;
    overflow-y: auto;
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
  }

  .loading-state,
  .empty-state {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-2);
    padding: var(--space-8);
    color: var(--color-text-muted);
    font-size: 0.875rem;
  }

  .spinner {
    width: 18px;
    height: 18px;
    border: 2px solid var(--color-border);
    border-top-color: var(--color-primary);
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }

  .tree-item {
    padding-left: calc(var(--depth) * 20px);
  }

  .category-row {
    display: flex;
    align-items: center;
    width: 100%;
    gap: var(--space-1);
    padding: var(--space-2) var(--space-3);
    border: none;
    background: none;
    text-align: left;
    font-size: 0.875rem;
    color: var(--color-text);
    cursor: default;
    transition: background-color var(--transition-fast);
    border-bottom: 1px solid var(--color-border-light);
  }

  .category-row:hover {
    background-color: var(--color-surface-hover);
  }

  .category-row.selected {
    background-color: rgba(139, 94, 107, 0.08);
    font-weight: 600;
  }

  .category-row.flat-result {
    flex-direction: column;
    align-items: flex-start;
    gap: 2px;
    cursor: pointer;
    padding: var(--space-3);
  }

  .cat-path {
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  .expand-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    flex-shrink: 0;
    border: none;
    background: none;
    cursor: pointer;
    border-radius: var(--radius-sm);
    color: var(--color-text-muted);
    padding: 0;
    transition: color var(--transition-fast), background-color var(--transition-fast);
  }

  .expand-btn:hover {
    color: var(--color-text);
    background-color: var(--color-surface-hover);
  }

  .expand-placeholder {
    width: 24px;
    height: 24px;
    flex-shrink: 0;
  }

  .chevron {
    transition: transform var(--transition-fast);
  }

  .chevron.rotated {
    transform: rotate(90deg);
  }

  .cat-select-btn {
    flex: 1;
    display: flex;
    align-items: center;
    border: none;
    background: none;
    cursor: pointer;
    padding: var(--space-1) var(--space-2);
    border-radius: var(--radius-sm);
    color: inherit;
    font: inherit;
    font-weight: inherit;
    text-align: left;
  }

  .cat-select-btn:hover {
    background-color: rgba(139, 94, 107, 0.06);
  }

  .cat-name {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
</style>
