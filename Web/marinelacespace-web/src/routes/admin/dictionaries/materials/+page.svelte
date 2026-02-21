<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification';
  import type { Material } from '$types';

  let loading = $state(true);
  let materials = $state<Material[]>([]);

  // Modal state
  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let saving = $state(false);

  // Delete
  let showDeleteModal = $state(false);
  let deleteTarget = $state<Material | null>(null);

  $effect(() => {
    loadMaterials();
  });

  async function loadMaterials() {
    try {
      loading = true;
      materials = await catalogApi.getMaterials();
    } catch {
      notificationStore.error('Помилка завантаження матеріалів');
    } finally {
      loading = false;
    }
  }

  function openCreate() {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    showModal = true;
  }

  function openEdit(material: Material) {
    modalMode = 'edit';
    editingId = material.id;
    modalName = material.name;
    showModal = true;
  }

  function confirmDelete(material: Material) {
    deleteTarget = material;
    showDeleteModal = true;
  }

  async function saveMaterial() {
    if (!modalName.trim()) {
      notificationStore.warning('Введіть назву матеріалу');
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createMaterial({ name: modalName.trim() });
        notificationStore.success('Матеріал створено');
      } else if (editingId) {
        await catalogApi.updateMaterial(editingId, { name: modalName.trim() });
        notificationStore.success('Матеріал оновлено');
      }
      showModal = false;
      loadMaterials();
    } catch {
      notificationStore.error('Помилка збереження матеріалу');
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteMaterial(deleteTarget.id);
      notificationStore.success('Матеріал видалено');
      showDeleteModal = false;
      deleteTarget = null;
      loadMaterials();
    } catch {
      notificationStore.error('Помилка видалення матеріалу');
    }
  }
</script>

<div class="materials-page">
  <div class="page-header">
    <h1 class="page-title">Довідник матеріалів</h1>
    <button class="btn btn-primary" on:click={openCreate}>Додати матеріал</button>
  </div>

  {#if loading}
    <LoadingSpinner message="Завантаження матеріалів..." />
  {:else if materials.length === 0}
    <EmptyState
      title="Матеріалів ще немає"
      description="Додайте матеріали для товарів"
      icon="🧵"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Назва</th>
            <th>Дії</th>
          </tr>
        </thead>
        <tbody>
          {#each materials as material}
            <tr>
              <td class="cell-name">{material.name}</td>
              <td class="cell-actions">
                <button class="btn btn-sm btn-ghost" on:click={() => openEdit(material)}>
                  Редагувати
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  on:click={() => confirmDelete(material)}
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
  title={modalMode === 'create' ? 'Новий матеріал' : 'Редагувати матеріал'}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="materialName">Назва</label>
    <input
      id="materialName"
      class="input"
      type="text"
      bind:value={modalName}
      placeholder="Шовк, Мереживо, Бавовна..."
    />
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" on:click={() => (showModal = false)}>Скасувати</button>
    <button class="btn btn-primary" on:click={saveMaterial} disabled={saving}>
      {saving ? 'Збереження...' : 'Зберегти'}
    </button>
  </div>
</Modal>

<Modal open={showDeleteModal} title="Видалити матеріал?" onclose={() => (showDeleteModal = false)}>
  <p>Ви впевнені, що хочете видалити матеріал <strong>{deleteTarget?.name}</strong>?</p>
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
