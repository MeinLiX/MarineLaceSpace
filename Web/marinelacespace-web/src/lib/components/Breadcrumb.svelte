<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  interface BreadcrumbItem {
    label: string;
    href?: string;
  }

  let { items }: { items: BreadcrumbItem[] } = $props();
</script>

<nav class="breadcrumb" aria-label="Breadcrumb">
  <ol>
    <li>
      <a href="/">{i18n.t('common.home')}</a>
    </li>
    {#each items as item, idx (idx)}
      <li aria-hidden="true" class="separator">/</li>
      <li>
        {#if item.href && idx < items.length - 1}
          <a href={item.href}>{item.label}</a>
        {:else}
          <span aria-current="page">{item.label}</span>
        {/if}
      </li>
    {/each}
  </ol>
</nav>

<style>
  .breadcrumb {
    margin-bottom: var(--space-4);
    font-size: 0.875rem;
  }

  .breadcrumb ol {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: var(--space-1);
    list-style: none;
    padding: 0;
    margin: 0;
  }

  .separator {
    color: var(--color-text-muted);
    user-select: none;
  }

  .breadcrumb a {
    color: var(--color-text-muted);
    text-decoration: none;
    transition: color var(--transition-fast);
  }

  .breadcrumb a:hover {
    color: var(--color-primary);
    text-decoration: underline;
  }

  .breadcrumb span[aria-current="page"] {
    color: var(--color-text);
    font-weight: 500;
  }
</style>
