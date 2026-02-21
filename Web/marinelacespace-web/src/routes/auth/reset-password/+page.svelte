<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import * as authApi from '$lib/api/auth';
  import { notificationStore } from '$lib/stores/notification';

  let newPassword = $state('');
  let confirmPassword = $state('');
  let showPassword = $state(false);
  let showConfirmPassword = $state(false);
  let isSubmitting = $state(false);
  let touched = $state({ newPassword: false, confirmPassword: false });

  let token = $derived($page.url.searchParams.get('token') ?? '');
  let email = $derived($page.url.searchParams.get('email') ?? '');

  let newPasswordError = $derived.by(() => {
    if (!touched.newPassword) return '';
    if (!newPassword) return 'Пароль є обов\'язковим';
    if (newPassword.length < 8) return 'Пароль має містити мінімум 8 символів';
    return '';
  });

  let confirmPasswordError = $derived.by(() => {
    if (!touched.confirmPassword) return '';
    if (!confirmPassword) return 'Підтвердіть пароль';
    if (confirmPassword !== newPassword) return 'Паролі не збігаються';
    return '';
  });

  let passwordStrength = $derived.by(() => {
    if (!newPassword) return { level: 0, label: '', color: '' };
    let score = 0;
    if (newPassword.length >= 8) score++;
    if (newPassword.length >= 12) score++;
    if (/[A-Z]/.test(newPassword)) score++;
    if (/[0-9]/.test(newPassword)) score++;
    if (/[^A-Za-z0-9]/.test(newPassword)) score++;

    if (score <= 2) return { level: 1, label: 'Слабкий', color: 'var(--color-error)' };
    if (score <= 3) return { level: 2, label: 'Середній', color: 'var(--color-warning)' };
    return { level: 3, label: 'Сильний', color: 'var(--color-success)' };
  });

  let isFormValid = $derived(
    newPassword.length >= 8 &&
    confirmPassword === newPassword &&
    token !== '' &&
    email !== ''
  );

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    touched = { newPassword: true, confirmPassword: true };

    if (!isFormValid) return;

    isSubmitting = true;
    try {
      await authApi.resetPassword({ email, token, newPassword });
      notificationStore.success('Пароль успішно змінено! Увійдіть з новим паролем.');
      await goto('/auth/login');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Помилка зміни пароля. Спробуйте ще раз.';
      notificationStore.error(message);
    } finally {
      isSubmitting = false;
    }
  }
</script>

<svelte:head>
  <title>Новий пароль — MarineLaceSpace</title>
</svelte:head>

<div class="auth-page">
  <div class="auth-card card">
    <div class="auth-brand">
      <span class="brand-icon" aria-hidden="true">✦</span>
      <h1 class="brand-name">MarineLaceSpace</h1>
    </div>

    <h2 class="auth-heading">Новий пароль</h2>

    {#if !token || !email}
      <div class="error-state" role="alert">
        <p class="error-text">Невірне або відсутнє посилання для відновлення пароля.</p>
        <a href="/auth/forgot-password" class="btn btn-primary btn-submit">
          Надіслати посилання повторно
        </a>
      </div>
    {:else}
      <form onsubmit={handleSubmit} novalidate>
        <div class="form-group">
          <label for="newPassword" class="form-label">Новий пароль</label>
          <div class="password-wrapper">
            <input
              id="newPassword"
              type={showPassword ? 'text' : 'password'}
              class="input"
              class:input-error={newPasswordError}
              placeholder="Мінімум 8 символів"
              autocomplete="new-password"
              bind:value={newPassword}
              onblur={() => touched.newPassword = true}
              aria-invalid={newPasswordError ? 'true' : undefined}
              aria-describedby={newPasswordError ? 'newPassword-error' : 'password-strength'}
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
          {#if newPasswordError}
            <p id="newPassword-error" class="field-error" role="alert">{newPasswordError}</p>
          {/if}
          {#if newPassword && !newPasswordError}
            <div id="password-strength" class="strength-indicator" aria-label="Надійність пароля: {passwordStrength.label}">
              <div class="strength-bars">
                {#each [1, 2, 3] as level}
                  <div
                    class="strength-bar"
                    style:background-color={passwordStrength.level >= level ? passwordStrength.color : 'var(--color-border)'}
                  ></div>
                {/each}
              </div>
              <span class="strength-label" style:color={passwordStrength.color}>{passwordStrength.label}</span>
            </div>
          {/if}
        </div>

        <div class="form-group">
          <label for="confirmPassword" class="form-label">Підтвердження пароля</label>
          <div class="password-wrapper">
            <input
              id="confirmPassword"
              type={showConfirmPassword ? 'text' : 'password'}
              class="input"
              class:input-error={confirmPasswordError}
              placeholder="Повторіть новий пароль"
              autocomplete="new-password"
              bind:value={confirmPassword}
              onblur={() => touched.confirmPassword = true}
              aria-invalid={confirmPasswordError ? 'true' : undefined}
              aria-describedby={confirmPasswordError ? 'confirmPassword-error' : undefined}
            />
            <button
              type="button"
              class="password-toggle"
              onclick={() => showConfirmPassword = !showConfirmPassword}
              aria-label={showConfirmPassword ? 'Сховати пароль' : 'Показати пароль'}
            >
              {#if showConfirmPassword}
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
          {#if confirmPasswordError}
            <p id="confirmPassword-error" class="field-error" role="alert">{confirmPasswordError}</p>
          {/if}
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-submit"
          disabled={isSubmitting}
        >
          {#if isSubmitting}
            <span class="spinner" aria-hidden="true"></span>
            Збереження...
          {:else}
            Зберегти пароль
          {/if}
        </button>
      </form>

      <p class="auth-footer">
        <a href="/auth/login" class="auth-link">← Повернутися до входу</a>
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

  .strength-indicator {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-top: var(--space-2);
  }

  .strength-bars {
    display: flex;
    gap: var(--space-1);
    flex: 1;
  }

  .strength-bar {
    height: 4px;
    flex: 1;
    border-radius: var(--radius-full);
    transition: background-color var(--transition-fast);
  }

  .strength-label {
    font-size: 0.75rem;
    font-weight: 500;
    white-space: nowrap;
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

  .error-state {
    text-align: center;
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }

  .error-text {
    color: var(--color-text-light);
    font-size: 0.875rem;
    line-height: 1.6;
  }

  @media (max-width: 480px) {
    .auth-card {
      padding: var(--space-6) var(--space-5);
    }
  }
</style>
