<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Size, Gender } from '$types';

  let loading = $state(true);
  let sizes = $state<Size[]>([]);

  // Modal state
  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let modalGender = $state<Gender>('Unisex');
  let saving = $state(false);

  // Delete
  let showDeleteModal = $state(false);
  let deleteTarget = $state<Size | null>(null);

  $effect(() => {
    loadSizes();
  });

  async function loadSizes() {
    try {
      loading = true;
      sizes = await catalogApi.getSizes();
    } catch {
      notificationStore.error('Помилка завантаження розмірів');
    } finally {
      loading = false;
    }
  }

  function openCreate() {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    modalGender = 'Unisex';
    showModal = true;
  }

  function openEdit(size: Size) {
    modalMode = 'edit';
    editingId = size.id;
    modalName = size.name;
    modalGender = size.gender;
    showModal = true;
  }

  function confirmDelete(size: Size) {
    deleteTarget = size;
    showDeleteModal = true;
  }

  async function saveSize() {
    if (!modalName.trim()) {
      notificationStore.warning('Введіть назву розміру');
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createSize({ name: modalName.trim(), gender: modalGender });
        notificationStore.success('Розмір створено');
      } else if (editingId) {
        await catalogApi.updateSize(editingId, { name: modalName.trim(), gender: modalGender });
        notificationStore.success('Розмір оновлено');
      }
      showModal = false;
      loadSizes();
    } catch {
      notificationStore.error('Помилка збереження розміру');
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteSize(deleteTarget.id);
      notificationStore.success('Розмір видалено');
      showDeleteModal = false;
      deleteTarget = null;
      loadSizes();
    } catch {
      notificationStore.error('Помилка видалення розміру');
    }
  }

  function genderBadge(gender: Gender): string {
    const map: Record<Gender, string> = {
      Male: 'badge-info',
      Female: 'badge-primary',
      Unisex: 'badge-outline',
    };
    return map[gender];
  }

  function genderLabel(gender: Gender): string {
    const map: Record<Gender, string> = {
      Male: 'Чоловічий',
      Female: 'Жіночий',
      Unisex: 'Унісекс',
    };
    return map[gender];
  }
</script>

<div class="sizes-page">
  <div class="page-header">
    <h1 class="page-title">Довідник розмірів</h1>
    <button class="btn btn-primary" on:click={openCreate}>Додати розмір</button>
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження розмірів..." />
  {:else if sizes.length === 0}
    <EmptyState
      title="Розмірів ще немає"
      description="Додайте розміри для товарів"
      icon="📐"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Назва</th>
            <th>Стать</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          {#each sizes as size}
            <tr>
              <td class="cell-name">{size.name}</td>
              <td>
                <span class="badge {genderBadge(size.gender)}">
                  {genderLabel(size.gender)}
                </span>
              </td>
              <td class="cell-actions">
                <button class="btn btn-sm btn-ghost" on:click={() => openEdit(size)}>
                  Редагувати
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  on:click={() => confirmDelete(size)}
                >
                  Видалити
                </button>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  {/if}
</div>

<Modal
  open={showModal}
  title={modalMode === 'create' ? 'Новий розмір' : 'Редагувати розмір'}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="sizeName">Назва</label>
    <input id="sizeName" class="input" type="text" bind:value={modalName} placeholder="XS, S, M, L..." />
  </div>
  <div class="form-group mt-4">
    <label class="form-label" for="sizeGender">Стать</label>
    <select id="sizeGender" class="input" bind:value={modalGender}>
      <option value="Unisex">Унісекс</option>
      <option value="Female">Жіночий</option>
      <option value="Male">Чоловічий</option>
    </select>
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showModal = false)}>Скасувати</button>
    <button class="btn btn-primary" on:click={saveSize} disabled={saving}>
      {saving ? 'Збереження...' : 'Зберегти'}
    </button>
  </div>
</Modal>

<Modal open={showDeleteModal} title="Видалити розмір?" onclose={() => (showDeleteModal = false)}>
  <p>Ви впевнені, що хочете видалити розмір <strong>{deleteTarget?.name}</strong>?</p>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showDeleteModal = false)}>Скасувати</button>
    <button class="btn btn-danger" on:click={executeDelete}>Видалити</button>
  </div>
</Modal>

<style>
  .page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
  }

  .page-title {
    font-size: 1.75rem;
  }

  .table-wrapper {
    overflow-x: auto;
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
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
    font-weight: 600;
    font-size: 0.9375rem;
  }

  .cell-actions {
    white-space: nowrap;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .form-label {
    font-size: 0.8125rem;
    font-weight: 600;
  }

  .mt-4 {
    margin-top: var(--space-4);
  }

  .btn-danger {
    background: var(--color-error);
    color: #fff;
    border: none;
  }

  .btn-danger-text {
    color: var(--color-error);
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
    margin-top: var(--space-6);
  }
</style>
