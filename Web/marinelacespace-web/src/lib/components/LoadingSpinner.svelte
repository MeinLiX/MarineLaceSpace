<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let {
    size = 'md',
    message = undefined
  }: {
    size?: 'sm' | 'md' | 'lg';
    message?: string;
  } = $props();

  const sizeMap = {
    sm: 24,
    md: 40,
    lg: 56
  };

  let dim = $derived(sizeMap[size]);
</script>

<div class="loading-spinner" role="status" aria-label={message ?? i18n.t('common.loading')}>
  <svg
    class="spinner"
    width={dim}
    height={dim}
    viewBox="0 0 50 50"
    aria-hidden="true"
  >
    <circle
      class="spinner-track"
      cx="25"
      cy="25"
      r="20"
      fill="none"
      stroke-width="4"
    />
    <circle
      class="spinner-head"
      cx="25"
      cy="25"
      r="20"
      fill="none"
      stroke-width="4"
      stroke-linecap="round"
    />
  </svg>
  {#if message}
    <p class="spinner-message {size}">{message}</p>
  {/if}
</div>

<style>
  .loading-spinner {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: var(--space-3);
    padding: var(--space-6);
  }

  .spinner {
    animation: spin 0.9s linear infinite;
  }

  .spinner-track {
    stroke: var(--color-border-light);
  }

  .spinner-head {
    stroke: var(--color-primary);
    stroke-dasharray: 80, 200;
    stroke-dashoffset: 0;
  }

  .spinner-message {
    color: var(--color-text-muted);
    text-align: center;
  }

  .spinner-message.sm {
    font-size: 0.75rem;
  }

  .spinner-message.md {
    font-size: 0.875rem;
  }

  .spinner-message.lg {
    font-size: 1rem;
  }

  @keyframes spin {
    from {
      transform: rotate(0deg);
    }
    to {
      transform: rotate(360deg);
    }
  }
</style>
