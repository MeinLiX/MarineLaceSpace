<script lang="ts">
  import { goto } from '$app/navigation';
  import { authStore } from '$lib/stores/auth';
  import { notificationStore } from '$lib/stores/notification';

  let email = $state('');
  let password = $state('');
  let rememberMe = $state(false);
  let showPassword = $state(false);
  let isSubmitting = $state(false);
  let touched = $state({ email: false, password: false });

  let emailError = $derived.by(() => {
    if (!touched.email) return '';
    if (!email.trim()) return 'Email є обов\'язковим';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Невірний формат email';
    return '';
  });

  let passwordError = $derived.by(() => {
    if (!touched.password) return '';
    if (!password) return 'Пароль є обов\'язковим';
    if (password.length < 6) return 'Пароль має містити мінімум 6 символів';
    return '';
  });

  let isFormValid = $derived(
    email.trim() !== '' &&
    /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) &&
    password.length >= 6
  );

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    touched = { email: true, password: true };

    if (!isFormValid) return;

    isSubmitting = true;
    try {
      await authStore.login({ email, password });
      notificationStore.success('Вхід виконано успішно!');
      await goto('/');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Помилка входу. Перевірте дані та спробуйте ще раз.';
      notificationStore.error(message);
    } finally {
      isSubmitting = false;
    }
  }
</script>

<svelte:head>
  <title>Увійти — MarineLaceSpace</title>
</svelte:head>

<div class="auth-page">
  <div class="auth-card card">
    <div class="auth-brand">
      <span class="brand-icon" aria-hidden="true">✦</span>
      <h1 class="brand-name">MarineLaceSpace</h1>
    </div>

    <h2 class="auth-heading">Увійти в акаунт</h2>

    <form onsubmit={handleSubmit} novalidate>
      <div class="form-group">
        <label for="email" class="form-label">Email</label>
        <input
          id="email"
          type="email"
          class="input"
          class:input-error={emailError}
          placeholder="your@email.com"
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

      <div class="form-group">
        <label for="password" class="form-label">Пароль</label>
        <div class="password-wrapper">
          <input
            id="password"
            type={showPassword ? 'text' : 'password'}
            class="input"
            class:input-error={passwordError}
            placeholder="Введіть пароль"
            autocomplete="current-password"
            bind:value={password}
            onblur={() => touched.password = true}
            aria-invalid={passwordError ? 'true' : undefined}
            aria-describedby={passwordError ? 'password-error' : undefined}
          />
          <button
            type="button"
            class="password-toggle"
            onclick={() => showPassword = !showPassword}
            aria-label={showPassword ? 'Сховати пароль' : 'Показати пароль'}
          >
            {#if showPassword}
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                <line x1="1" y1="1" x2="23" y2="23"/>
              </svg>
            {:else}
              <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
              </svg>
            {/if}
          </button>
        </div>
        {#if passwordError}
          <p id="password-error" class="field-error" role="alert">{passwordError}</p>
        {/if}
      </div>

      <div class="form-row">
        <label class="checkbox-label">
          <input type="checkbox" bind:checked={rememberMe} />
          <span>Запам'ятати мене</span>
        </label>
        <a href="/auth/forgot-password" class="forgot-link">Забули пароль?</a>
      </div>

      <button
        type="submit"
        class="btn btn-primary btn-submit"
        disabled={isSubmitting}
      >
        {#if isSubmitting}
          <span class="spinner" aria-hidden="true"></span>
          Вхід...
        {:else}
          Увійти
        {/if}
      </button>
    </form>

    <div class="divider">
      <span>або</span>
    </div>

    <a href="/auth/register" class="btn btn-outline btn-submit">
      Створити акаунт
    </a>
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

  .input-error:focus {
    box-shadow: 0 0 0 3px rgba(196, 85, 90, 0.15) !important;
  }

  .field-error {
    font-size: 0.8125rem;
    color: var(--color-error);
    margin-top: var(--space-1);
  }

  .password-wrapper {
    position: relative;
  }

  .password-wrapper .input {
    width: 100%;
    padding-right: 44px;
  }

  .password-toggle {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    display: flex;
    align-items: center;
    justify-content: center;
    background: none;
    border: none;
    cursor: pointer;
    color: var(--color-text-muted);
    padding: var(--space-1);
    border-radius: var(--radius-sm);
    transition: color var(--transition-fast);
  }

  .password-toggle:hover {
    color: var(--color-text);
  }

  .form-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
    gap: var(--space-2);
  }

  .checkbox-label {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    font-size: 0.875rem;
    color: var(--color-text);
    cursor: pointer;
  }

  .checkbox-label input[type="checkbox"] {
    width: 16px;
    height: 16px;
    accent-color: var(--color-primary);
    cursor: pointer;
  }

  .forgot-link {
    font-size: 0.875rem;
    color: var(--color-primary);
    text-decoration: none;
    transition: color var(--transition-fast);
  }

  .forgot-link:hover {
    color: var(--color-primary-dark);
    text-decoration: underline;
  }

  .btn-submit {
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-2);
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

  .divider {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    margin: var(--space-6) 0;
    color: var(--color-text-muted);
    font-size: 0.8125rem;
  }

  .divider::before,
  .divider::after {
    content: '';
    flex: 1;
    height: 1px;
    background: var(--color-border);
  }

  @media (max-width: 480px) {
    .auth-card {
      padding: var(--space-6) var(--space-5);
    }
  }
</style>
