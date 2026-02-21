<script lang="ts">
  import * as authApi from '$api/auth';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { AuthUser } from '$types';

  let loading = $state(true);
  let users = $state<AuthUser[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');

  let showRoleModal = $state(false);
  let roleTarget = $state<AuthUser | null>(null);
  let roleAdmin = $state(false);
  let roleSeller = $state(false);
  let roleCustomer = $state(true);
  let savingRoles = $state(false);

  let accessDenied = $derived(!authStore.isAdmin);

  $effect(() => {
    if (!accessDenied) {
      loadUsers(currentPage, search);
    }
  });

  async function loadUsers(page: number, q: string) {
    try {
      loading = true;
      const result = await authApi.getUsers({
        page,
        pageSize: 20,
        search: q || undefined,
      });
      users = result.items;
      totalPages = result.totalPages;
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingUsers'));
    } finally {
      loading = false;
    }
  }

  function handlePageChange(page: number) {
    currentPage = page;
  }

  function openRoleModal(user: AuthUser) {
    roleTarget = user;
    roleAdmin = user.roles.includes('Admin');
    roleSeller = user.roles.includes('Seller');
    roleCustomer = user.roles.includes('Customer');
    showRoleModal = true;
  }

  async function saveRoles() {
    if (!roleTarget) return;
    try {
      savingRoles = true;
      const roles: string[] = [];
      if (roleAdmin) roles.push('Admin');
      if (roleSeller) roles.push('Seller');
      if (roleCustomer) roles.push('Customer');
      if (roles.length === 0) roles.push('Customer');

      await authApi.assignRoles(roleTarget.id, roles);
      notificationStore.success(i18n.t('admin.rolesUpdated'));
      showRoleModal = false;
      loadUsers(currentPage, search);
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingRoles'));
    } finally {
      savingRoles = false;
    }
  }

  function roleBadgeClass(role: string): string {
    const map: Record<string, string> = {
      Admin: 'role-admin',
      Seller: 'role-seller',
      Customer: 'role-customer',
    };
    return map[role] ?? 'role-default';
  }

  function roleIcon(role: string): string {
    const map: Record<string, string> = {
      Admin: '🛡️',
      Seller: '🏪',
      Customer: '👤',
    };
    return map[role] ?? '🔵';
  }
</script>

<div class="users-page">
  {#if accessDenied}
    <div class="access-denied">
      <div class="access-denied-icon">🚫</div>
      <h2>Access Denied</h2>
      <p>Only administrators can manage users.</p>
      <a href="/admin" class="btn btn-primary">Back to Dashboard</a>
    </div>
  {:else}
    <div class="page-header">
      <h1 class="page-title">{i18n.t('admin.users')}</h1>
      {#if !loading}
        <span class="user-count">{users.length} users on this page</span>
      {/if}
    </div>

    <div class="toolbar">
      <div class="search-wrapper">
        <span class="search-icon">🔍</span>
        <input
          class="input input-sm search-input"
          type="search"
          placeholder={i18n.t('admin.searchByNameOrEmail')}
          bind:value={search}
          oninput={() => { currentPage = 1; }}
        />
      </div>
    </div>

    {#if loading}
      <LoadingSpinner message={i18n.t('admin.loadingUsers')} />
    {:else if users.length === 0}
      <EmptyState title={i18n.t('admin.noUsersFound')} icon="👥" />
    {:else}
      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>{i18n.t('admin.name')}</th>
              <th>Email</th>
              <th>{i18n.t('admin.roles')}</th>
              <th>{i18n.t('admin.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {#each users as user}
              <tr class="user-row">
                <td class="cell-name">
                  <div class="user-avatar">{user.firstName.charAt(0)}{user.lastName.charAt(0)}</div>
                  <div class="user-name-group">
                    <span class="user-fullname">{user.firstName} {user.lastName}</span>
                    <span class="user-id">ID: {user.id.slice(0, 8)}</span>
                  </div>
                </td>
                <td>
                  <a href="mailto:{user.email}" class="email-link">{user.email}</a>
                </td>
                <td>
                  <div class="roles-list">
                    {#each user.roles as role}
                      <span class="role-badge {roleBadgeClass(role)}">
                        <span class="role-icon">{roleIcon(role)}</span>
                        {role}
                      </span>
                    {/each}
                  </div>
                </td>
                <td class="cell-actions">
                  <button class="btn btn-sm btn-outline action-btn" onclick={() => openRoleModal(user)}>
                    ✏️ {i18n.t('admin.roles')}
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>

      <Pagination {currentPage} {totalPages} onPageChange={handlePageChange} />
    {/if}
  {/if}
</div>

<Modal open={showRoleModal} title={i18n.t('admin.assignRoles')} onclose={() => (showRoleModal = false)}>
  {#if roleTarget}
    <div class="modal-user-info">
      <div class="modal-user-avatar">{roleTarget.firstName.charAt(0)}{roleTarget.lastName.charAt(0)}</div>
      <div>
        <p class="modal-user-name">{roleTarget.firstName} {roleTarget.lastName}</p>
        <p class="modal-user-email">{roleTarget.email}</p>
      </div>
    </div>
    <div class="role-checkboxes">
      <label class="role-checkbox" class:role-checkbox-active={roleAdmin}>
        <input type="checkbox" bind:checked={roleAdmin} />
        <span class="role-badge role-admin">🛡️ Admin</span>
        <span class="role-desc">{i18n.t('admin.roleDescAdmin')}</span>
      </label>
      <label class="role-checkbox" class:role-checkbox-active={roleSeller}>
        <input type="checkbox" bind:checked={roleSeller} />
        <span class="role-badge role-seller">🏪 Seller</span>
        <span class="role-desc">{i18n.t('admin.roleDescSeller')}</span>
      </label>
      <label class="role-checkbox" class:role-checkbox-active={roleCustomer}>
        <input type="checkbox" bind:checked={roleCustomer} />
        <span class="role-badge role-customer">👤 Customer</span>
        <span class="role-desc">{i18n.t('admin.roleDescCustomer')}</span>
      </label>
    </div>
    <div class="modal-actions">
      <button class="btn btn-outline" onclick={() => (showRoleModal = false)}>{i18n.t('common.cancel')}</button>
      <button class="btn btn-primary" onclick={saveRoles} disabled={savingRoles}>
        {savingRoles ? i18n.t('common.saving') : i18n.t('common.save')}
      </button>
    </div>
  {/if}
</Modal>

<style>
  .access-denied {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: var(--space-12) var(--space-6);
    text-align: center;
    gap: var(--space-3);
  }

  .access-denied-icon {
    font-size: 3rem;
  }

  .access-denied h2 {
    font-size: 1.5rem;
    margin: 0;
  }

  .access-denied p {
    color: var(--color-text-light);
    margin-bottom: var(--space-4);
  }

  .page-header {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    margin-bottom: var(--space-6);
  }

  .page-title {
    font-size: 1.75rem;
    margin: 0;
  }

  .user-count {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  .toolbar {
    margin-bottom: var(--space-4);
  }

  .search-wrapper {
    position: relative;
    max-width: 400px;
  }

  .search-icon {
    position: absolute;
    left: var(--space-3);
    top: 50%;
    transform: translateY(-50%);
    font-size: 0.875rem;
    pointer-events: none;
  }

  .search-input {
    max-width: 400px;
    padding-left: var(--space-8);
  }

  .table-wrapper {
    overflow-x: auto;
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    margin-bottom: var(--space-4);
  }

  .data-table {
    width: 100%;
    font-size: 0.875rem;
  }

  .data-table th {
    text-align: left;
    padding: var(--space-3) var(--space-4);
    font-weight: 600;
    color: var(--color-text-light);
    border-bottom: 2px solid var(--color-border);
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    white-space: nowrap;
  }

  .data-table td {
    padding: var(--space-3) var(--space-4);
    border-bottom: 1px solid var(--color-border-light);
    vertical-align: middle;
  }

  .user-row {
    transition: background var(--transition-fast);
  }

  .user-row:hover {
    background: var(--color-surface-hover);
  }

  .cell-name {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .user-avatar {
    width: 36px;
    height: 36px;
    border-radius: var(--radius-full);
    background: var(--color-primary);
    color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.75rem;
    font-weight: 700;
    flex-shrink: 0;
    text-transform: uppercase;
  }

  .user-name-group {
    display: flex;
    flex-direction: column;
  }

  .user-fullname {
    font-weight: 600;
    font-size: 0.875rem;
  }

  .user-id {
    font-size: 0.6875rem;
    color: var(--color-text-muted);
    font-family: monospace;
  }

  .email-link {
    color: var(--color-text-light);
    text-decoration: none;
    font-size: 0.8125rem;
  }

  .email-link:hover {
    color: var(--color-primary);
    text-decoration: underline;
  }

  .cell-actions {
    white-space: nowrap;
  }

  .action-btn {
    font-size: 0.75rem;
  }

  .roles-list {
    display: flex;
    gap: var(--space-1);
    flex-wrap: wrap;
  }

  .role-badge {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    padding: 2px var(--space-2);
    border-radius: var(--radius-full);
    font-size: 0.6875rem;
    font-weight: 600;
    border: 1px solid transparent;
  }

  .role-icon {
    font-size: 0.625rem;
  }

  .role-admin {
    background: color-mix(in srgb, var(--color-error) 12%, transparent);
    color: var(--color-error);
    border-color: color-mix(in srgb, var(--color-error) 30%, transparent);
  }

  .role-seller {
    background: color-mix(in srgb, var(--color-warning) 12%, transparent);
    color: var(--color-warning);
    border-color: color-mix(in srgb, var(--color-warning) 30%, transparent);
  }

  .role-customer {
    background: color-mix(in srgb, var(--color-info) 12%, transparent);
    color: var(--color-info);
    border-color: color-mix(in srgb, var(--color-info) 30%, transparent);
  }

  .role-default {
    background: var(--color-border-light);
    color: var(--color-text-light);
  }

  /* Modal styles */
  .modal-user-info {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-bottom: var(--space-6);
    padding: var(--space-3);
    background: var(--color-surface-hover);
    border-radius: var(--radius-md);
  }

  .modal-user-avatar {
    width: 44px;
    height: 44px;
    border-radius: var(--radius-full);
    background: var(--color-primary);
    color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.875rem;
    font-weight: 700;
    flex-shrink: 0;
    text-transform: uppercase;
  }

  .modal-user-name {
    font-weight: 600;
    margin: 0;
  }

  .modal-user-email {
    font-size: 0.8125rem;
    color: var(--color-text-light);
    margin: 0;
  }

  .role-checkboxes {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    margin-bottom: var(--space-4);
  }

  .role-checkbox {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    cursor: pointer;
    padding: var(--space-3);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
    transition: all var(--transition-fast);
  }

  .role-checkbox:hover {
    border-color: var(--color-primary-light);
    background: var(--color-surface-hover);
  }

  .role-checkbox-active {
    border-color: var(--color-primary);
    background: color-mix(in srgb, var(--color-primary) 5%, transparent);
  }

  .role-desc {
    font-size: 0.8125rem;
    color: var(--color-text-light);
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }
</style>
