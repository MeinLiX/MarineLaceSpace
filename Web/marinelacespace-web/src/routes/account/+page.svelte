<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
  import { authStore } from '$lib/stores/auth.svelte';
  import { notificationStore } from '$lib/stores/notification.svelte';
  import * as authApi from '$lib/api/auth';

  let isEditing = $state(false);
  let isSaving = $state(false);

  let editFirstName = $state('');
  let editLastName = $state('');
  let editPhone = $state('');

  let user = $derived(authStore.currentUser);

  let initials = $derived.by(() => {
    if (!user) return '?';
    const f = user.firstName?.[0] ?? '';
    const l = user.lastName?.[0] ?? '';
    return (f + l).toUpperCase() || '?';
  });

  function startEditing() {
    if (!user) return;
    editFirstName = user.firstName;
    editLastName = user.lastName;
    editPhone = '';
    isEditing = true;
  }

  function cancelEditing() {
    isEditing = false;
  }

  let touched = $state({ firstName: false, lastName: false });

  let firstNameError = $derived.by(() => {
    if (!touched.firstName) return '';
    if (!editFirstName.trim()) return i18n.t('account.firstNameRequired');
    return '';
  });

  let lastNameError = $derived.by(() => {
    if (!touched.lastName) return '';
    if (!editLastName.trim()) return i18n.t('account.lastNameRequired');
    return '';
  });

  async function handleSave(e: SubmitEvent) {
    e.preventDefault();
    touched = { firstName: true, lastName: true };

    if (!editFirstName.trim() || !editLastName.trim()) return;

    isSaving = true;
    try {
      await authApi.updateMe({
        firstName: editFirstName.trim(),
        lastName: editLastName.trim(),
        phoneNumber: editPhone.trim() || undefined,
      });
      await authStore.loadUser();
      notificationStore.success(i18n.t('account.profileUpdated'));
      isEditing = false;
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : i18n.t('account.saveError');
      notificationStore.error(message);
    } finally {
      isSaving = false;
    }
  }

  // Password change
  let showPasswordForm = $state(false);
  let currentPassword = $state('');
  let newPassword = $state('');
  let confirmPassword = $state('');
  let isChangingPassword = $state(false);
  let pwTouched = $state({ current: false, newPw: false, confirm: false });

  let currentPasswordError = $derived.by(() => {
    if (!pwTouched.current) return '';
    if (!currentPassword) return i18n.t('account.currentPasswordRequired');
    return '';
  });

  let newPasswordError = $derived.by(() => {
    if (!pwTouched.newPw) return '';
    if (!newPassword) return i18n.t('account.newPasswordRequired');
    if (newPassword.length < 8) return i18n.t('account.passwordMinLength');
    return '';
  });

  let confirmPasswordError = $derived.by(() => {
    if (!pwTouched.confirm) return '';
    if (newPassword !== confirmPassword) return i18n.t('account.passwordsDoNotMatch');
    return '';
  });

  async function handleChangePassword(e: SubmitEvent) {
    e.preventDefault();
    pwTouched = { current: true, newPw: true, confirm: true };

    if (!currentPassword || !newPassword || newPassword.length < 8 || newPassword !== confirmPassword) return;

    isChangingPassword = true;
    try {
      await authApi.changePassword({ currentPassword, newPassword });
      notificationStore.success(i18n.t('account.passwordChanged'));
      showPasswordForm = false;
      currentPassword = '';
      newPassword = '';
      confirmPassword = '';
      pwTouched = { current: false, newPw: false, confirm: false };
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : i18n.t('account.passwordChangeError');
      notificationStore.error(message);
    } finally {
      isChangingPassword = false;
    }
  }
</script>

<svelte:head>
  <title>{i18n.t('account.profile')} — MarineLaceSpace</title>
</svelte:head>

<div class="profile-page">
  <h1 class="page-heading">{i18n.t('account.profile')}</h1>

  {#if user}
    <div class="profile-card card">
      <div class="profile-header">
        <div class="avatar" aria-hidden="true">
          <span class="avatar-initials">{initials}</span>
        </div>
        <div class="profile-info">
          <h2 class="profile-name">{user.firstName} {user.lastName}</h2>
          <p class="profile-email">{user.email}</p>
          <div class="role-badges">
            {#each user.roles as role}
              <span class="role-badge">{role}</span>
            {/each}
          </div>
        </div>
      </div>

      {#if !isEditing}
        <div class="profile-details">
          <div class="detail-row">
            <span class="detail-label">{i18n.t('account.firstName')}</span>
            <span class="detail-value">{user.firstName}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">{i18n.t('account.lastName')}</span>
            <span class="detail-value">{user.lastName}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">{i18n.t('account.email')}</span>
            <span class="detail-value">{user.email}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">{i18n.t('account.phone')}</span>
            <span class="detail-value text-muted">{i18n.t('account.notSpecified')}</span>
          </div>
        </div>

        <button class="btn btn-outline edit-btn" onclick={startEditing}>
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
            <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/>
            <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/>
          </svg>
          {i18n.t('account.editProfile')}
        </button>
      {:else}
        <form class="edit-form" onsubmit={handleSave} novalidate>
          <div class="form-group">
            <label for="editFirstName" class="form-label">{i18n.t('account.firstName')}</label>
            <input
              id="editFirstName"
              type="text"
              class="input"
              class:input-error={firstNameError}
              bind:value={editFirstName}
              onblur={() => touched.firstName = true}
              aria-invalid={firstNameError ? 'true' : undefined}
            />
            {#if firstNameError}
              <p class="field-error" role="alert">{firstNameError}</p>
            {/if}
          </div>

          <div class="form-group">
            <label for="editLastName" class="form-label">{i18n.t('account.lastName')}</label>
            <input
              id="editLastName"
              type="text"
              class="input"
              class:input-error={lastNameError}
              bind:value={editLastName}
              onblur={() => touched.lastName = true}
              aria-invalid={lastNameError ? 'true' : undefined}
            />
            {#if lastNameError}
              <p class="field-error" role="alert">{lastNameError}</p>
            {/if}
          </div>

          <div class="form-group">
            <label for="editEmail" class="form-label">{i18n.t('account.email')}</label>
            <input
              id="editEmail"
              type="email"
              class="input"
              value={user.email}
              disabled
              aria-readonly="true"
            />
            <p class="field-hint">{i18n.t('account.emailReadonly')}</p>
          </div>

          <div class="form-group">
            <label for="editPhone" class="form-label">{i18n.t('account.phone')}</label>
            <input
              id="editPhone"
              type="tel"
              class="input"
              placeholder="+380 XX XXX XX XX"
              bind:value={editPhone}
            />
          </div>

          <div class="form-actions">
            <button type="button" class="btn btn-ghost" onclick={cancelEditing}>
              {i18n.t('common.cancel')}
            </button>
            <button type="submit" class="btn btn-primary" disabled={isSaving}>
              {#if isSaving}
                <span class="spinner" aria-hidden="true"></span>
                {i18n.t('account.saving')}
              {:else}
                {i18n.t('account.saveChanges')}
              {/if}
            </button>
          </div>
        </form>
      {/if}
    </div>

    <!-- Password Change -->
    <div class="password-section card">
      <button class="password-toggle" onclick={() => showPasswordForm = !showPasswordForm}>
        <h2 class="section-title">{i18n.t('account.changePassword')}</h2>
        <span class="toggle-icon" class:open={showPasswordForm}>▸</span>
      </button>

      {#if showPasswordForm}
        <form class="password-form" onsubmit={handleChangePassword}>
          <div class="form-group">
            <label for="currentPassword">{i18n.t('account.currentPassword')}</label>
            <input
              id="currentPassword"
              type="password"
              class="form-input"
              class:error={currentPasswordError}
              bind:value={currentPassword}
              onfocus={() => pwTouched.current = true}
            />
            {#if currentPasswordError}
              <span class="field-error">{currentPasswordError}</span>
            {/if}
          </div>

          <div class="form-group">
            <label for="newPassword">{i18n.t('account.newPassword')}</label>
            <input
              id="newPassword"
              type="password"
              class="form-input"
              class:error={newPasswordError}
              bind:value={newPassword}
              onfocus={() => pwTouched.newPw = true}
            />
            {#if newPasswordError}
              <span class="field-error">{newPasswordError}</span>
            {/if}
          </div>

          <div class="form-group">
            <label for="confirmPassword">{i18n.t('account.confirmNewPassword')}</label>
            <input
              id="confirmPassword"
              type="password"
              class="form-input"
              class:error={confirmPasswordError}
              bind:value={confirmPassword}
              onfocus={() => pwTouched.confirm = true}
            />
            {#if confirmPasswordError}
              <span class="field-error">{confirmPasswordError}</span>
            {/if}
          </div>

          <div class="form-actions">
            <button type="button" class="btn btn-secondary" onclick={() => { showPasswordForm = false; currentPassword = ''; newPassword = ''; confirmPassword = ''; pwTouched = { current: false, newPw: false, confirm: false }; }}>
              {i18n.t('common.cancel')}
            </button>
            <button type="submit" class="btn btn-primary" disabled={isChangingPassword}>
              {#if isChangingPassword}
                <span class="spinner" aria-hidden="true"></span>
                {i18n.t('account.changingPassword')}
              {:else}
                {i18n.t('account.changePassword')}
              {/if}
            </button>
          </div>
        </form>
      {/if}
    </div>
  {/if}
</div>

<style>
  .page-heading {
    font-family: var(--font-display);
    font-size: 1.5rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-6);
  }

  .profile-card {
    padding: var(--space-8);
  }

  .profile-header {
    display: flex;
    align-items: center;
    gap: var(--space-5);
    margin-bottom: var(--space-8);
    padding-bottom: var(--space-6);
    border-bottom: 1px solid var(--color-border);
  }

  .avatar {
    width: 72px;
    height: 72px;
    border-radius: 50%;
    background: linear-gradient(135deg, var(--color-primary), var(--color-primary-light));
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  .avatar-initials {
    font-family: var(--font-display);
    font-size: 1.5rem;
    font-weight: 600;
    color: #fff;
  }

  .profile-info {
    min-width: 0;
  }

  .profile-name {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-1);
  }

  .profile-email {
    font-size: 0.875rem;
    color: var(--color-text-light);
    margin-bottom: var(--space-2);
  }

  .role-badges {
    display: flex;
    gap: var(--space-2);
    flex-wrap: wrap;
  }

  .role-badge {
    display: inline-flex;
    align-items: center;
    padding: 2px 10px;
    font-size: 0.75rem;
    font-weight: 500;
    color: var(--color-primary-dark);
    background: rgba(139, 94, 107, 0.1);
    border-radius: var(--radius-full);
  }

  .profile-details {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    margin-bottom: var(--space-6);
  }

  .detail-row {
    display: flex;
    align-items: center;
    gap: var(--space-4);
  }

  .detail-label {
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text-light);
    min-width: 100px;
  }

  .detail-value {
    font-size: 0.9375rem;
    color: var(--color-text);
  }

  .edit-btn {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
  }

  /* Edit form */
  .edit-form {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .form-group {
    margin-bottom: var(--space-4);
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

  .field-hint {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
    margin-top: var(--space-1);
  }

  .form-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-4);
  }

  .form-actions .btn {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
  }

  .spinner {
    display: inline-block;
    width: 16px;
    height: 16px;
    border: 2px solid rgba(255, 255, 255, 0.3);
    border-top-color: #fff;
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }

  @media (max-width: 640px) {
    .profile-card {
      padding: var(--space-5);
    }

    .profile-header {
      flex-direction: column;
      text-align: center;
    }

    .role-badges {
      justify-content: center;
    }

    .detail-row {
      flex-direction: column;
      align-items: flex-start;
      gap: var(--space-1);
    }

    .detail-label {
      min-width: unset;
    }

    .form-actions {
      flex-direction: column-reverse;
    }

    .form-actions .btn {
      width: 100%;
      justify-content: center;
    }
  }

  /* Password Section */
  .password-section {
    margin-top: var(--space-6);
    padding: var(--space-6) var(--space-8);
  }

  .password-toggle {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    background: none;
    border: none;
    cursor: pointer;
    padding: 0;
  }

  .section-title {
    font-family: var(--font-display);
    font-size: 1.1rem;
    font-weight: 600;
    color: var(--color-text);
    margin: 0;
  }

  .toggle-icon {
    font-size: 1rem;
    color: var(--color-text-secondary);
    transition: transform 0.2s ease;
  }

  .toggle-icon.open {
    transform: rotate(90deg);
  }

  .password-form {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
    margin-top: var(--space-5);
    padding-top: var(--space-5);
    border-top: 1px solid var(--color-border);
  }
</style>
