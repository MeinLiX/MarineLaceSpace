<script lang="ts">
  import { page } from '$app/stores';
  import { i18n } from '$i18n/index.svelte';

  const status = $derived($page.status);
  const message = $derived($page.error?.message ?? '');

  const title = $derived(
    status === 404
      ? i18n.t('errorPage.notFoundTitle')
      : status === 403
        ? i18n.t('errorPage.forbiddenTitle')
        : status >= 500
          ? i18n.t('errorPage.serverErrorTitle')
          : i18n.t('errorPage.genericTitle')
  );

  const description = $derived(
    status === 404
      ? i18n.t('errorPage.notFoundMessage')
      : status === 403
        ? i18n.t('errorPage.forbiddenMessage')
        : status >= 500
          ? i18n.t('errorPage.serverErrorMessage')
          : message || i18n.t('errorPage.genericMessage')
  );

  const illustration = $derived(
    status === 404 ? '🔍' : status === 403 ? '🔒' : status >= 500 ? '⚙️' : '⚠️'
  );
</script>

<svelte:head>
  <title>{i18n.t('errorPage.errorCode', { code: status })} | {i18n.t('common.brand')}</title>
</svelte:head>

<div class="error-page">
  <div class="error-container">
    <div class="error-illustration">{illustration}</div>
    <span class="error-code">{status}</span>
    <h1 class="error-title">{title}</h1>
    <p class="error-description">{description}</p>

    <div class="error-actions">
      <a href="/" class="btn btn-primary">{i18n.t('errorPage.goHome')}</a>
      <button class="btn btn-secondary" onclick={() => history.back()}>
        {i18n.t('errorPage.goBack')}
      </button>
    </div>
  </div>
</div>

<style>
  .error-page {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
    padding: var(--space-8) var(--space-4);
  }

  .error-container {
    text-align: center;
    max-width: 480px;
  }

  .error-illustration {
    font-size: 4rem;
    margin-bottom: var(--space-4);
    animation: float 3s ease-in-out infinite;
  }

  .error-code {
    display: block;
    font-family: var(--font-display);
    font-size: 5rem;
    font-weight: 700;
    color: var(--color-primary-light);
    line-height: 1;
    margin-bottom: var(--space-2);
  }

  .error-title {
    font-family: var(--font-display);
    font-size: 1.75rem;
    color: var(--color-text);
    margin-bottom: var(--space-3);
  }

  .error-description {
    font-family: var(--font-body);
    color: var(--color-text-light);
    font-size: 1rem;
    line-height: 1.6;
    margin-bottom: var(--space-8);
  }

  .error-actions {
    display: flex;
    gap: var(--space-3);
    justify-content: center;
    flex-wrap: wrap;
  }

  .btn {
    display: inline-flex;
    align-items: center;
    padding: var(--space-3) var(--space-6);
    border-radius: 8px;
    font-family: var(--font-body);
    font-size: 0.9rem;
    font-weight: 500;
    text-decoration: none;
    cursor: pointer;
    transition: all 0.2s ease;
    border: none;
  }

  .btn-primary {
    background: var(--color-primary);
    color: white;
  }

  .btn-primary:hover {
    background: var(--color-primary-dark);
    transform: translateY(-1px);
  }

  .btn-secondary {
    background: var(--color-surface);
    color: var(--color-text);
    border: 1px solid var(--color-border);
  }

  .btn-secondary:hover {
    background: var(--color-surface-hover);
    transform: translateY(-1px);
  }

  @keyframes float {
    0%, 100% { transform: translateY(0px); }
    50% { transform: translateY(-10px); }
  }
</style>
