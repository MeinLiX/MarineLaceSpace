<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Material } from '$types';

  let loading = $state(true);
  let materials = $state<Material[]>([]);

  // Modal state
  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let modalImageUrl = $state('');
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
      notificationStore.error(i18n.t('admin.errorLoadingMaterials'));
    } finally {
      loading = false;
    }
  }

  function openCreate() {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    modalImageUrl = '';
    showModal = true;
  }

  function openEdit(material: Material) {
    modalMode = 'edit';
    editingId = material.id;
    modalName = material.name;
    modalImageUrl = material.imageUrl ?? '';
    showModal = true;
  }

  function confirmDelete(material: Material) {
    deleteTarget = material;
    showDeleteModal = true;
  }

  async function saveMaterial() {
    if (!modalName.trim()) {
      notificationStore.warning(i18n.t('admin.enterMaterialName'));
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createMaterial({ name: modalName.trim(), imageUrl: modalImageUrl.trim() || undefined });
        notificationStore.success(i18n.t('admin.materialCreated'));
      } else if (editingId) {
        await catalogApi.updateMaterial(editingId, { name: modalName.trim(), imageUrl: modalImageUrl.trim() || undefined });
        notificationStore.success(i18n.t('admin.materialUpdated'));
      }
      showModal = false;
      loadMaterials();
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingMaterial'));
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteMaterial(deleteTarget.id);
      notificationStore.success(i18n.t('admin.materialDeleted'));
      showDeleteModal = false;
      deleteTarget = null;
      loadMaterials();
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingMaterial'));
    }
  }
</script>

<div class="materials-page">
  <div class="page-header">
    <h1 class="page-title">{i18n.t('admin.materialsDictionary')}</h1>
    <button class="btn btn-primary" onclick={openCreate}>{i18n.t('admin.addMaterial')}</button>
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingMaterials')} />
  {:else if materials.length === 0}
    <EmptyState
      title={i18n.t('admin.noMaterialsYet')}
      description={i18n.t('admin.addMaterialsForProducts')}
      icon="🧵"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>{i18n.t('admin.image')}</th>
            <th>{i18n.t('admin.name')}</th>
            <th>{i18n.t('admin.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {#each materials as material}
            <tr>
              <td class="cell-image">
                {#if material.imageUrl}
                  <img src={material.imageUrl} alt={material.name} class="material-thumb" />
                {:else}
                  <span class="material-placeholder">🧵</span>
                {/if}
              </td>
              <td class="cell-name">{material.name}</td>
              <td class="cell-actions">
                <button class="btn btn-sm btn-ghost" onclick={() => openEdit(material)}>
                  {i18n.t('common.edit')}
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  onclick={() => confirmDelete(material)}
                >
                  {i18n.t('common.delete')}
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
  title={modalMode === 'create' ? i18n.t('admin.newMaterial') : i18n.t('admin.editMaterial')}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="materialName">{i18n.t('admin.name')}</label>
    <input
      id="materialName"
      class="input"
      type="text"
      bind:value={modalName}
      placeholder={i18n.t('admin.materialPlaceholder')}
    />
  </div>
  <div class="form-group">
    <label class="form-label" for="materialImageUrl">{i18n.t('admin.imageUrl')}</label>
    <input
      id="materialImageUrl"
      class="input"
      type="url"
      bind:value={modalImageUrl}
      placeholder="https://..."
    />
    {#if modalImageUrl}
      <img src={modalImageUrl} alt="Preview" class="material-preview" />
    {/if}
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-primary" onclick={saveMaterial} disabled={saving}>
      {saving ? i18n.t('common.saving') : i18n.t('common.save')}
    </button>
  </div>
</Modal>

<Modal open={showDeleteModal} title={i18n.t('admin.deleteMaterialQuestion')} onclose={() => (showDeleteModal = false)}>
  <p>{i18n.t('admin.confirmDeleteMaterial', { name: deleteTarget?.name ?? '' })}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-danger" onclick={executeDelete}>{i18n.t('common.delete')}</button>
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

  .cell-image {
    width: 48px;
  }

  .material-thumb {
    width: 36px;
    height: 36px;
    border-radius: var(--radius-sm);
    object-fit: cover;
  }

  .material-placeholder {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    background: var(--color-border-light);
    border-radius: var(--radius-sm);
    font-size: 1rem;
  }

  .material-preview {
    width: 64px;
    height: 64px;
    border-radius: var(--radius-sm);
    object-fit: cover;
    margin-top: var(--space-2);
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
