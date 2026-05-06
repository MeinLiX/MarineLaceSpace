<script lang="ts">
  import { goto } from '$app/navigation';
  import { authStore } from '$lib/stores/auth.svelte';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import FileUploadDropzone from '$components/FileUploadDropzone.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Material, Shop } from '$types';

  $effect(() => {
    if (!authStore.isLoading && !authStore.isAdmin && !authStore.isSeller) {
      goto('/admin');
    }
  });

  let loading = $state(true);
  let materials = $state<Material[]>([]);
  let myShop = $state<Shop | null>(null);

  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let modalImageFile = $state<File | null>(null);
  let modalImagePreview = $state<string | null>(null);
  let existingImageUrl = $state<string | null>(null);
  let saving = $state(false);
  let fileInput = $state<HTMLInputElement | null>(null);

  let showDeleteModal = $state(false);
  let deleteTarget = $state<Material | null>(null);

  $effect(() => {
    loadData();
  });

  async function loadData() {
    try {
      loading = true;
      if (authStore.isSeller && !authStore.isAdmin) {
        const shops = await catalogApi.getMyShops();
        myShop = shops.length > 0 ? shops[0] : null;
      }
      materials = await catalogApi.getMaterials(myShop?.id);
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingMaterials'));
    } finally {
      loading = false;
    }
  }

  function canEditEntry(entry: Material): boolean {
    if (authStore.isAdmin) return true;
    return !!entry.shopId && entry.shopId === myShop?.id;
  }

  function openCreate() {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    modalImageFile = null;
    modalImagePreview = null;
    existingImageUrl = null;
    showModal = true;
  }

  function openEdit(material: Material) {
    modalMode = 'edit';
    editingId = material.id;
    modalName = material.name;
    modalImageFile = null;
    modalImagePreview = null;
    existingImageUrl = material.imageUrl ?? null;
    showModal = true;
  }

  function handleFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      modalImageFile = file;
      const reader = new FileReader();
      reader.onload = (e) => {
        modalImagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  function handleDropzoneFile(file: File) {
    modalImageFile = file;
    const reader = new FileReader();
    reader.onload = (e) => {
      modalImagePreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  function clearSelectedFile() {
    modalImageFile = null;
    modalImagePreview = null;
    if (fileInput) fileInput.value = '';
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
        await catalogApi.createMaterial({ name: modalName.trim(), imageFile: modalImageFile ?? undefined, shopId: myShop?.id });
        notificationStore.success(i18n.t('admin.materialCreated'));
      } else if (editingId) {
        await catalogApi.updateMaterial(editingId, { name: modalName.trim(), imageFile: modalImageFile ?? undefined });
        notificationStore.success(i18n.t('admin.materialUpdated'));
      }
      showModal = false;
      loadData();
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
      loadData();
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingMaterial'));
    }
  }
</script>

{#if authStore.isAdmin || authStore.isSeller}
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
            <th>Scope</th>
            <th>{i18n.t('admin.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {#each materials as material}
            <tr>
              <td class="cell-image">
                {#if material.imageUrl}
                  <img src={material.imageUrl} alt={material.name} class="material-thumb" loading="lazy" />
                {:else}
                  <span class="material-placeholder">🧵</span>
                {/if}
              </td>
              <td class="cell-name">{material.name}</td>
              <td>
                <span class="badge {material.shopId ? 'badge-shop' : 'badge-global'}">
                  {material.shopId ? '🏪 Shop' : '🌐 Global'}
                </span>
              </td>
              <td class="cell-actions">
                {#if canEditEntry(material)}
                  <button class="btn btn-sm btn-ghost" onclick={() => openEdit(material)}>
                    {i18n.t('common.edit')}
                  </button>
                  <button
                    class="btn btn-sm btn-ghost btn-danger-text"
                    onclick={() => confirmDelete(material)}
                  >
                    {i18n.t('common.delete')}
                  </button>
                {/if}
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
    <span class="form-label">{i18n.t('admin.image')}</span>
    <FileUploadDropzone
      accept="image/*"
      preview={modalImagePreview}
      existingUrl={existingImageUrl}
      compact={true}
      onfile={handleDropzoneFile}
      onclear={clearSelectedFile}
    />
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
{:else}
  <div style="padding: 2rem; text-align: center;">
    <p>{i18n.t('admin.accessDenied')}</p>
    <a href="/admin">← {i18n.t('admin.backToDashboard')}</a>
  </div>
{/if}

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

  .badge-global {
    background: #e0f2fe;
    color: #0369a1;
    padding: 2px 8px;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
  }

  .badge-shop {
    background: #fef3c7;
    color: #92400e;
    padding: 2px 8px;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
  }
</style>
