<script lang="ts">
  let {
    title,
    description = undefined,
    icon = '📦',
    actionLabel = undefined,
    actionHref = undefined
  }: {
    title: string;
    description?: string;
    icon?: string;
    actionLabel?: string;
    actionHref?: string;
  } = $props();
</script>

<div class="empty-state" role="status">
  <span class="empty-icon" aria-hidden="true">{icon}</span>
  <h3 class="empty-title">{title}</h3>
  {#if description}
    <p class="empty-description">{description}</p>
  {/if}
  {#if actionLabel}
    {#if actionHref}
      <a href={actionHref} class="btn btn-primary empty-action">{actionLabel}</a>
    {:else}
      <button class="btn btn-primary empty-action">{actionLabel}</button>
    {/if}
  {/if}
</div>

<style>
  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    padding: var(--space-16) var(--space-6);
    max-width: 400px;
    margin-inline: auto;
  }

  .empty-icon {
    font-size: 3rem;
    margin-bottom: var(--space-4);
    line-height: 1;
    opacity: 0.7;
  }

  .empty-title {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-2);
  }

  .empty-description {
    font-size: 0.875rem;
    color: var(--color-text-muted);
    line-height: 1.6;
    margin-bottom: var(--space-6);
  }

  .empty-action {
    text-decoration: none;
  }
</style>
