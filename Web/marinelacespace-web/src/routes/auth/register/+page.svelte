<script lang="ts">
  import { goto } from '$app/navigation';
  import { authStore } from '$lib/stores/auth';
  import { notificationStore } from '$lib/stores/notification';

  let firstName = $state('');
  let lastName = $state('');
  let email = $state('');
  let password = $state('');
  let confirmPassword = $state('');
  let termsAccepted = $state(false);
  let isSubmitting = $state(false);
  let showPassword = $state(false);
  let showConfirmPassword = $state(false);

  let touched = $state({
    firstName: false,
    lastName: false,
    email: false,
    password: false,
    confirmPassword: false,
    terms: false,
  });

  let firstNameError = $derived.by(() => {
    if (!touched.firstName) return '';
    if (!firstName.trim()) return 'Ім\'я є обов\'язковим';
    return '';
  });

  let lastNameError = $derived.by(() => {
    if (!touched.lastName) return '';
    if (!lastName.trim()) return 'Прізвище є обов\'язковим';
    return '';
  });

  let emailError = $derived.by(() => {
    if (!touched.email) return '';
    if (!email.trim()) return 'Email є обов\'язковим';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Невірний формат email';
    return '';
  });

  let passwordError = $derived.by(() => {
    if (!touched.password) return '';
    if (!password) return 'Пароль є обов\'язковим';
    if (password.length < 8) return 'Пароль має містити мінімум 8 символів';
    return '';
  });

  let confirmPasswordError = $derived.by(() => {
    if (!touched.confirmPassword) return '';
    if (!confirmPassword) return 'Підтвердіть пароль';
    if (confirmPassword !== password) return 'Паролі не збігаються';
    return '';
  });

  let termsError = $derived.by(() => {
    if (!touched.terms) return '';
    if (!termsAccepted) return 'Необхідно прийняти умови використання';
    return '';
  });

  let passwordStrength = $derived.by(() => {
    if (!password) return { level: 0, label: '', color: '' };
    let score = 0;
    if (password.length >= 8) score++;
    if (password.length >= 12) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;

    if (score <= 2) return { level: 1, label: 'Слабкий', color: 'var(--color-error)' };
    if (score <= 3) return { level: 2, label: 'Середній', color: 'var(--color-warning)' };
    return { level: 3, label: 'Сильний', color: 'var(--color-success)' };
  });

  let isFormValid = $derived(
    firstName.trim() !== '' &&
    lastName.trim() !== '' &&
    /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) &&
    password.length >= 8 &&
    confirmPassword === password &&
    termsAccepted
  );

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();
    touched = {
      firstName: true,
      lastName: true,
      email: true,
      password: true,
      confirmPassword: true,
      terms: true,
    };

    if (!isFormValid) return;

    isSubmitting = true;
    try {
      await authStore.register({ email, password, firstName, lastName });
      notificationStore.success('Акаунт створено успішно!');
      await goto('/auth/login');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Помилка реєстрації. Спробуйте ще раз.';
      notificationStore.error(message);
    } finally {
      isSubmitting = false;
    }
  }
</script>

<svelte:head>
  <title>Реєстрація — MarineLaceSpace</title>
</svelte:head>

<div class="auth-page">
  <div class="auth-card card">
    <div class="auth-brand">
      <span class="brand-icon" aria-hidden="true">✦</span>
      <h1 class="brand-name">MarineLaceSpace</h1>
    </div>

    <h2 class="auth-heading">Створити акаунт</h2>

    <form onsubmit={handleSubmit} novalidate>
      <div class="form-row-2">
        <div class="form-group">
          <label for="firstName" class="form-label">Ім'я</label>
          <input
            id="firstName"
            type="text"
            class="input"
            class:input-error={firstNameError}
            placeholder="Марія"
            autocomplete="given-name"
            bind:value={firstName}
            onblur={() => touched.firstName = true}
            aria-invalid={firstNameError ? 'true' : undefined}
            aria-describedby={firstNameError ? 'firstName-error' : undefined}
          />
          {#if firstNameError}
            <p id="firstName-error" class="field-error" role="alert">{firstNameError}</p>
          {/if}
        </div>

        <div class="form-group">
          <label for="lastName" class="form-label">Прізвище</label>
          <input
            id="lastName"
            type="text"
            class="input"
            class:input-error={lastNameError}
            placeholder="Іваненко"
            autocomplete="family-name"
            bind:value={lastName}
            onblur={() => touched.lastName = true}
            aria-invalid={lastNameError ? 'true' : undefined}
            aria-describedby={lastNameError ? 'lastName-error' : undefined}
          />
          {#if lastNameError}
            <p id="lastName-error" class="field-error" role="alert">{lastNameError}</p>
          {/if}
        </div>
      </div>

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
            placeholder="Мінімум 8 символів"
            autocomplete="new-password"
            bind:value={password}
            onblur={() => touched.password = true}
            aria-invalid={passwordError ? 'true' : undefined}
            aria-describedby={passwordError ? 'password-error' : 'password-strength'}
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
        {#if password && !passwordError}
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
            placeholder="Повторіть пароль"
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

      <div class="form-group">
        <label class="checkbox-label">
          <input
            type="checkbox"
            bind:checked={termsAccepted}
            onchange={() => touched.terms = true}
          />
          <span>Я погоджуюсь з <a href="/terms" class="terms-link">умовами використання</a></span>
        </label>
        {#if termsError}
          <p class="field-error" role="alert">{termsError}</p>
        {/if}
      </div>

      <button
        type="submit"
        class="btn btn-primary btn-submit"
        disabled={isSubmitting}
      >
        {#if isSubmitting}
          <span class="spinner" aria-hidden="true"></span>
          Реєстрація...
        {:else}
          Зареєструватися
        {/if}
      </button>
    </form>

    <p class="auth-footer">
      Вже маєте акаунт? <a href="/auth/login" class="auth-link">Увійти</a>
    </p>
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
    max-width: 480px;
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

  .form-row-2 {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-4);
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

  .checkbox-label {
    display: flex;
    align-items: flex-start;
    gap: var(--space-2);
    font-size: 0.875rem;
    color: var(--color-text);
    cursor: pointer;
    line-height: 1.4;
  }

  .checkbox-label input[type="checkbox"] {
    width: 16px;
    height: 16px;
    accent-color: var(--color-primary);
    cursor: pointer;
    margin-top: 2px;
    flex-shrink: 0;
  }

  .terms-link {
    color: var(--color-primary);
    text-decoration: none;
  }

  .terms-link:hover {
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

  .auth-footer {
    text-align: center;
    margin-top: var(--space-6);
    font-size: 0.875rem;
    color: var(--color-text-light);
  }

  .auth-link {
    color: var(--color-primary);
    text-decoration: none;
    font-weight: 500;
  }

  .auth-link:hover {
    text-decoration: underline;
  }

  @media (max-width: 480px) {
    .auth-card {
      padding: var(--space-6) var(--space-5);
    }

    .form-row-2 {
      grid-template-columns: 1fr;
    }
  }
</style>
