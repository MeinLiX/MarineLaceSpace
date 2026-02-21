<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    open = false,
    title = undefined,
    onclose,
    children
  }: {
    open: boolean;
    title?: string;
    onclose: () => void;
    children: Snippet;
  } = $props();

  function handleBackdropClick(e: MouseEvent) {
    if (e.target === e.currentTarget) {
      onclose();
    }
  }

  function handleKeydown(e: KeyboardEvent) {
    if (e.key === 'Escape') {
      onclose();
    }
  }

  $effect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  });
</script>

<svelte:window on:keydown={handleKeydown} />

{#if open}
  <div
    class="modal-backdrop"
    on:click={handleBackdropClick}
    role="dialog"
    aria-modal="true"
    aria-label={title ?? 'Dialog'}
  >
    <div class="modal-card" role="document">
      <header class="modal-header">
        {#if title}
          <h2 class="modal-title">{title}</h2>
        {/if}
        <button class="modal-close" on:click={onclose} aria-label="Close dialog">
          <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </header>
      <div class="modal-body">
        {@render children()}
      </div>
    </div>
  </div>
{/if}

<style>
  .modal-backdrop {
    position: fixed;
    inset: 0;
    z-index: var(--z-modal);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-4);
    background-color: rgba(0, 0, 0, 0.45);
    animation: fadeIn 200ms ease both;
  }

  .modal-card {
    width: 100%;
    max-width: 560px;
    max-height: calc(100vh - var(--space-16));
    display: flex;
    flex-direction: column;
    background-color: var(--color-surface);
    border-radius: var(--radius-xl);
    box-shadow: var(--shadow-xl);
    overflow: hidden;
    animation: modalScaleIn 250ms ease both;
  }

  .modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: var(--space-5) var(--space-6);
    border-bottom: 1px solid var(--color-border-light);
    gap: var(--space-4);
    min-height: 56px;
  }

  .modal-title {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--color-text);
    margin: 0;
  }

  .modal-close {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    flex-shrink: 0;
    margin-left: auto;
    border: none;
    background: none;
    cursor: pointer;
    border-radius: var(--radius-full);
    color: var(--color-text-muted);
    transition: color var(--transition-fast), background-color var(--transition-fast);
    padding: 0;
  }

  .modal-close:hover {
    color: var(--color-text);
    background-color: var(--color-surface-hover);
  }

  .modal-body {
    padding: var(--space-6);
    overflow-y: auto;
  }

  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }

  @keyframes modalScaleIn {
    from {
      opacity: 0;
      transform: scale(0.95) translateY(8px);
    }
    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }
</style>
