<script lang="ts">
  interface BreadcrumbItem {
    label: string;
    href?: string;
  }

  let {
    items
  }: {
    items: BreadcrumbItem[];
  } = $props();
</script>

<nav class="breadcrumb" aria-label="Breadcrumb">
  <ol class="breadcrumb-list">
    {#each items as item, i (i)}
      <li class="breadcrumb-item">
        {#if i < items.length - 1 && item.href}
          <a href={item.href} class="breadcrumb-link">{item.label}</a>
          <span class="breadcrumb-separator" aria-hidden="true">/</span>
        {:else}
          <span class="breadcrumb-current" aria-current="page">{item.label}</span>
        {/if}
      </li>
    {/each}
  </ol>
</nav>

<style>
  .breadcrumb {
    padding: var(--space-3) 0;
  }

  .breadcrumb-list {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: var(--space-1);
    list-style: none;
    padding: 0;
    margin: 0;
  }

  .breadcrumb-item {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
  }

  .breadcrumb-link {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
    text-decoration: none;
    transition: color var(--transition-fast);
    white-space: nowrap;
  }

  .breadcrumb-link:hover {
    color: var(--color-primary);
    text-decoration: underline;
  }

  .breadcrumb-separator {
    font-size: 0.75rem;
    color: var(--color-border);
    user-select: none;
    margin-inline: var(--space-1);
  }

  .breadcrumb-current {
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text);
    white-space: nowrap;
  }
</style>
