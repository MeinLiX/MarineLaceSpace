<script lang="ts">
  import * as authApi from '$lib/api/auth';
  import { notificationStore } from '$lib/stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';

  let email = $state('');
  let isSubmitting = $state(false);
  let isSent = $state(false);
  let touched = $state({ email: false });

  let emailError = $derived.by(() => {
    if (!touched.email) return '';
    if (!email.trim()) return i18n.t('auth.emailRequired');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return i18n.t('auth.emailInvalid');
    return '';
  });

  let isFormValid = $derived(
    email.trim() !== '' && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
  );

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    touched.email = true;

    if (!isFormValid) return;

    isSubmitting = true;
    try {
      await authApi.forgotPassword({ email });
      isSent = true;
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : i18n.t('auth.forgotPasswordError');
      notificationStore.error(message);
    } finally {
      isSubmitting = false;
    }
  }
</script>

<svelte:head>
  <title>{i18n.t('auth.forgotPasswordTitle')} — MarineLaceSpace</title>
</svelte:head>

<div class="auth-page">
  <div class="auth-card card">
    <div class="auth-brand">
      <span class="brand-icon" aria-hidden="true">✦</span>
      <h1 class="brand-name">MarineLaceSpace</h1>
    </div>

    {#if isSent}
      <div class="success-state" role="status">
        <div class="success-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" width="48" height="48" fill="none" stroke="var(--color-success)" stroke-width="1.5">
            <path d="M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0z"/>
            <path d="M8 12l3 3 5-5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <h2 class="success-heading">{i18n.t('auth.checkEmail')}</h2>
        <p class="success-text">
          {i18n.t('auth.resetLinkSent')} <strong>{email}</strong>.
        </p>
        <a href="/auth/login" class="btn btn-primary btn-submit">
          {i18n.t('auth.backToLogin')}
        </a>
      </div>
    {:else}
      <h2 class="auth-heading">{i18n.t('auth.forgotPasswordHeading')}</h2>
      <p class="auth-description">
        {i18n.t('auth.forgotPasswordDescription')}
      </p>

      <form onsubmit={handleSubmit} novalidate>
        <div class="form-group">
          <label for="email" class="form-label">{i18n.t('auth.email')}</label>
          <input
            id="email"
            type="email"
            class="input"
            class:input-error={emailError}
            placeholder={i18n.t('auth.emailPlaceholder')}
            autocomplete="email"
            bind:value={email}
            onblur={() => touched.email = true}
            aria-invalid={emailError ? 'true' : undefined}
            aria-describedby={emailError ? 'email-error' : undefined}
          />
          {#if emailError}
            <p id="email-error" class="field-error" role="alert">{emailError}</p>
          {/if}
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-submit"
          disabled={isSubmitting}
        >
          {#if isSubmitting}
            <span class="spinner" aria-hidden="true"></span>
            {i18n.t('auth.sending')}
          {:else}
            {i18n.t('auth.sendLink')}
          {/if}
        </button>
      </form>

      <p class="auth-footer">
        <a href="/auth/login" class="auth-link">← {i18n.t('auth.backToLogin')}</a>
      </p>
    {/if}
  </div>
</div>

<style>
  .auth-page {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: calc(100vh - 200px);
    padding: var(--space-6) var(--space-4);
  }

  .auth-card {
    width: 100%;
    max-width: 440px;
    padding: var(--space-10) var(--space-8);
  }

  .auth-brand {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-2);
    margin-bottom: var(--space-6);
  }

  .brand-icon {
    font-size: 2rem;
    color: var(--color-primary);
    line-height: 1;
  }

  .brand-name {
    font-family: var(--font-display);
    font-size: 1.5rem;
    font-weight: 600;
    color: var(--color-text);
    letter-spacing: 0.02em;
  }

  .auth-heading {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    text-align: center;
    color: var(--color-text);
    margin-bottom: var(--space-3);
  }

  .auth-description {
    text-align: center;
    font-size: 0.875rem;
    color: var(--color-text-light);
    line-height: 1.6;
    margin-bottom: var(--space-6);
  }

  .form-group {
    margin-bottom: var(--space-5);
  }

  .form-label {
    display: block;
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
    margin-bottom: var(--space-2);
  }

  .form-group .input {
    width: 100%;
  }

  .input-error {
    border-color: var(--color-error) !important;
  }

  .field-error {
    font-size: 0.8125rem;
    color: var(--color-error);
    margin-top: var(--space-1);
  }

  .btn-submit {
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-2);
    text-decoration: none;
  }

  .spinner {
    display: inline-block;
    width: 18px;
    height: 18px;
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-top-color: #fff;
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }

  .auth-footer {
    text-align: center;
    margin-top: var(--space-6);
    font-size: 0.875rem;
  }

  .auth-link {
    color: var(--color-primary);
    text-decoration: none;
    font-weight: 500;
  }

  .auth-link:hover {
    text-decoration: underline;
  }

  /* Success state */
  .success-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    gap: var(--space-4);
  }

  .success-icon {
    margin-bottom: var(--space-2);
  }

  .success-heading {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .success-text {
    font-size: 0.875rem;
    color: var(--color-text-light);
    line-height: 1.6;
    margin-bottom: var(--space-4);
  }

  @media (max-width: 480px) {
    .auth-card {
      padding: var(--space-6) var(--space-5);
    }
  }
</style>
