<script lang="ts">
  import * as authApi from '$api/auth';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
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
      notificationStore.error('Помилка завантаження користувачів');
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
      notificationStore.success('Ролі оновлено');
      showRoleModal = false;
      loadUsers(currentPage, search);
    } catch {
      notificationStore.error('Помилка збереження ролей');
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
  <h1 class="page-title">Користувачі</h1>

  <div class="toolbar">
    <input
      class="input input-sm search-input"
      type="search"
      placeholder="Пошук за ім'ям або email..."
      bind:value={search}
      on:input={() => { currentPage = 1; }}
    />
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження користувачів..." />
  {:else if users.length === 0}
    <EmptyState title="Користувачів не знайдено" icon="👥" />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Ім'я</th>
            <th>Email</th>
            <th>Ролі</th>
            <th>Дії</th>
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
                <button class="btn btn-sm btn-ghost" on:click={() => openRoleModal(user)}>
                  Ролі
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

<Modal open={showRoleModal} title="Призначити ролі" onclose={() => (showRoleModal = false)}>
  {#if roleTarget}
    <p class="mb-4">
      Користувач: <strong>{roleTarget.firstName} {roleTarget.lastName}</strong>
      ({roleTarget.email})
    </p>
    <div class="role-checkboxes">
      <label class="role-checkbox">
        <input type="checkbox" bind:checked={roleAdmin} />
        <span class="badge badge-error">Admin</span>
        <span class="role-desc">Повний доступ до адмін-панелі</span>
      </label>
      <label class="role-checkbox">
        <input type="checkbox" bind:checked={roleSeller} />
        <span class="badge badge-warning">Seller</span>
        <span class="role-desc">Може створювати та керувати магазинами</span>
      </label>
      <label class="role-checkbox">
        <input type="checkbox" bind:checked={roleCustomer} />
        <span class="badge badge-info">Customer</span>
        <span class="role-desc">Базова роль покупця</span>
      </label>
    </div>
    <div class="modal-actions">
      <button class="btn btn-outline" on:click={() => (showRoleModal = false)}>Скасувати</button>
      <button class="btn btn-primary" on:click={saveRoles} disabled={savingRoles}>
        {savingRoles ? 'Збереження...' : 'Зберегти'}
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
