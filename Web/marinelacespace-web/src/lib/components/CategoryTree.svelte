<script lang="ts">
  import { goto } from '$app/navigation';
  import CategoryTree from './CategoryTree.svelte';
  import { i18n } from '$i18n/index.svelte';

  interface CategoryItem {
    id: string;
    name: string;
    children?: CategoryItem[];
  }

  interface Props {
    categories: CategoryItem[];
    selectedId?: string;
    level?: number;
  }

  let {
    categories,
    selectedId = undefined,
    level = 0
  }: Props = $props();

  let expandedIds = $state<Set<string>>(new Set());

  function toggleExpand(id: string, e: Event) {
    e.preventDefault();
    e.stopPropagation();
    const next = new Set(expandedIds);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    expandedIds = next;
  }

  function navigateToCategory(id: string) {
    goto(`/catalog?category=${id}`);
  }

  function handleKeydown(id: string, hasChildren: boolean, e: KeyboardEvent) {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      if (hasChildren) {
        toggleExpand(id, e);
      }
      navigateToCategory(id);
    }
  }
</script>

<ul class="category-tree" role={level === 0 ? 'tree' : 'group'} style="--indent: {level}">
  {#each categories as category (category.id)}
    {@const hasChildren = category.children != null && category.children.length > 0}
    {@const isExpanded = expandedIds.has(category.id)}
    {@const isActive = category.id === selectedId}
    <li class="tree-item" role="treeitem" aria-selected={isActive} aria-expanded={hasChildren ? isExpanded : undefined}>
      <div
        class="tree-node"
        class:active={isActive}
        style="padding-left: calc(var(--space-3) + {level} * var(--space-5))"
      >
        {#if hasChildren}
          <button
            class="expand-btn"
            aria-label={isExpanded ? i18n.t('common.collapse') : i18n.t('common.expand')}
            onclick={(e) => toggleExpand(category.id, e)}
          >
            <svg
              class="expand-icon"
              class:rotated={isExpanded}
              viewBox="0 0 24 24"
              width="14"
              height="14"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              aria-hidden="true"
            >
              <polyline points="9 18 15 12 9 6" />
            </svg>
          </button>
        {:else}
          <span class="expand-spacer"></span>
        {/if}

        <button
          class="tree-label"
          class:active={isActive}
          onclick={() => navigateToCategory(category.id)}
          onkeydown={(e) => handleKeydown(category.id, hasChildren, e)}
        >
          {category.name}
        </button>
      </div>

      {#if hasChildren && isExpanded}
        <CategoryTree
          categories={category.children ?? []}
          {selectedId}
          level={level + 1}
        />
      {/if}
    </li>
  {/each}
</ul>

<style>
  .category-tree {
    list-style: none;
    padding: 0;
    margin: 0;
  }

  .tree-item {
    margin: 0;
  }

  .tree-node {
    display: flex;
    align-items: center;
    gap: var(--space-1);
    padding-block: var(--space-1);
    padding-right: var(--space-3);
    border-radius: var(--radius-md);
    transition: background-color var(--transition-fast);
  }

  .tree-node:hover {
    background-color: var(--color-surface-hover);
  }

  .tree-node.active {
    background-color: rgba(139, 94, 107, 0.08);
  }

  .expand-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 22px;
    height: 22px;
    flex-shrink: 0;
    border: none;
    background: none;
    cursor: pointer;
    border-radius: var(--radius-sm);
    color: var(--color-text-muted);
    transition: color var(--transition-fast), background-color var(--transition-fast);
    padding: 0;
  }

  .expand-btn:hover {
    color: var(--color-text);
    background-color: var(--color-border-light);
  }

  .expand-icon {
    transition: transform var(--transition-fast);
  }

  .expand-icon.rotated {
    transform: rotate(90deg);
  }

  .expand-spacer {
    width: 22px;
    flex-shrink: 0;
  }

  .tree-label {
    flex: 1;
    text-align: left;
    padding: var(--space-1) var(--space-2);
    font-size: 0.875rem;
    color: var(--color-text);
    border: none;
    background: none;
    cursor: pointer;
    border-radius: var(--radius-sm);
    transition: color var(--transition-fast);
    font-family: inherit;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .tree-label:hover {
    color: var(--color-primary);
  }

  .tree-label.active {
    color: var(--color-primary);
    font-weight: 600;
  }
</style>
