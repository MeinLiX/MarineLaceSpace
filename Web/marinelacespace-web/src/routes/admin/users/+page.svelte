<script lang="ts">
  import * as authApi from '$api/auth';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { AuthUser } from '$types';

  let loading = $state(true);
  let users = $state<AuthUser[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let search = $state('');

  // Role modal
  let showRoleModal = $state(false);
  let roleTarget = $state<AuthUser | null>(null);
  let roleAdmin = $state(false);
  let roleSeller = $state(false);
  let roleCustomer = $state(true);
  let savingRoles = $state(false);

  $effect(() => {
    loadUsers(currentPage, search);
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

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA');
  }

  function roleBadgeClass(role: string): string {
    const map: Record<string, string> = {
      Admin: 'badge-error',
      Seller: 'badge-warning',
      Customer: 'badge-info',
    };
    return map[role] ?? 'badge-outline';
  }
</script>

<div class="users-page">
  <h1 class="page-title">{i18n.t('admin.users')}</h1>

  <div class="toolbar">
    <input
      class="input input-sm search-input"
      type="search"
      placeholder={i18n.t('admin.searchByNameOrEmail')}
      bind:value={search}
      oninput={() => { currentPage = 1; }}
    />
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
            <tr>
              <td class="cell-name">{user.firstName} {user.lastName}</td>
              <td>{user.email}</td>
              <td>
                <div class="roles-list">
                  {#each user.roles as role}
                    <span class="badge {roleBadgeClass(role)}">{role}</span>
                  {/each}
                </div>
              </td>
              <td class="cell-actions">
                <button class="btn btn-sm btn-ghost" onclick={() => openRoleModal(user)}>
                  {i18n.t('admin.roles')}
                </button>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>

    <Pagination {currentPage} {totalPages} onPageChange={handlePageChange} />
  {/if}
</div>

<Modal open={showRoleModal} title={i18n.t('admin.assignRoles')} onclose={() => (showRoleModal = false)}>
  {#if roleTarget}
    <p class="mb-4">
      {i18n.t('admin.user')}: <strong>{roleTarget.firstName} {roleTarget.lastName}</strong>
      ({roleTarget.email})
    </p>
    <div class="role-checkboxes">
      <label class="role-checkbox">
        <input type="checkbox" bind:checked={roleAdmin} />
        <span class="badge badge-error">Admin</span>
        <span class="role-desc">{i18n.t('admin.roleDescAdmin')}</span>
      </label>
      <label class="role-checkbox">
        <input type="checkbox" bind:checked={roleSeller} />
        <span class="badge badge-warning">Seller</span>
        <span class="role-desc">{i18n.t('admin.roleDescSeller')}</span>
      </label>
      <label class="role-checkbox">
        <input type="checkbox" bind:checked={roleCustomer} />
        <span class="badge badge-info">Customer</span>
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
  .page-title {
    font-size: 1.75rem;
    margin-bottom: var(--space-6);
  }

  .toolbar {
    margin-bottom: var(--space-4);
  }

  .search-input {
    max-width: 400px;
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

  .data-table tbody tr:hover {
    background: var(--color-surface-hover);
  }

  .cell-name {
    font-weight: 500;
  }

  .cell-actions {
    white-space: nowrap;
  }

  .roles-list {
    display: flex;
    gap: var(--space-1);
    flex-wrap: wrap;
  }

  .role-checkboxes {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
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
    transition: border-color var(--transition-fast);
  }

  .role-checkbox:hover {
    border-color: var(--color-primary-light);
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

  .mb-4 {
    margin-bottom: var(--space-4);
  }
</style>
