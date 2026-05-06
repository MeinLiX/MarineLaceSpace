<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let visible = $state(false);

  function handleScroll() {
    visible = window.scrollY > 400;
  }

  function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
</script>

<svelte:window onscroll={handleScroll} />

{#if visible}
  <button
    class="scroll-to-top"
    onclick={scrollToTop}
    aria-label={i18n.t('common.scrollToTop')}
    title={i18n.t('common.scrollToTop')}
  >
    <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
      <polyline points="18 15 12 9 6 15" />
    </svg>
  </button>
{/if}

<style>
  .scroll-to-top {
    position: fixed;
    bottom: var(--space-6);
    right: var(--space-6);
    z-index: 50;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 44px;
    height: 44px;
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-full);
    background-color: var(--color-surface);
    box-shadow: var(--shadow-md);
    color: var(--color-text-muted);
    cursor: pointer;
    transition: color var(--transition-fast), background-color var(--transition-fast), box-shadow var(--transition-fast);
    animation: fadeIn var(--transition-base) both;
    padding: 0;
  }

  .scroll-to-top:hover {
    color: var(--color-primary);
    background-color: var(--color-surface-hover);
    box-shadow: var(--shadow-lg);
  }

  @keyframes fadeIn {
    from { opacity: 0; transform: translateY(8px); }
    to { opacity: 1; transform: translateY(0); }
  }
</style>
