<script lang="ts">
  import { goto } from '$app/navigation';
  import { authStore } from '$lib/stores/auth.svelte';
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Size, Gender } from '$types';

  $effect(() => {
    if (!authStore.isLoading && !authStore.isAdmin) {
      goto('/admin');
    }
  });

  let loading = $state(true);
  let sizes = $state<Size[]>([]);

  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let modalGender = $state<Gender>('Unisex');
  let saving = $state(false);

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
      notificationStore.error(i18n.t('admin.errorLoadingSizes'));
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
      notificationStore.warning(i18n.t('admin.enterSizeName'));
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createSize({ name: modalName.trim(), gender: modalGender });
        notificationStore.success(i18n.t('admin.sizeCreated'));
      } else if (editingId) {
        await catalogApi.updateSize(editingId, { name: modalName.trim(), gender: modalGender });
        notificationStore.success(i18n.t('admin.sizeUpdated'));
      }
      showModal = false;
      loadSizes();
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingSize'));
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteSize(deleteTarget.id);
      notificationStore.success(i18n.t('admin.sizeDeleted'));
      showDeleteModal = false;
      deleteTarget = null;
      loadSizes();
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingSize'));
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
      Male: i18n.t('admin.genderMale'),
      Female: i18n.t('admin.genderFemale'),
      Unisex: i18n.t('admin.genderUnisex'),
    };
    return map[gender];
  }
</script>

{#if authStore.isAdmin}
<div class="sizes-page">
  <div class="page-header">
    <h1 class="page-title">{i18n.t('admin.sizesDictionary')}</h1>
    <button class="btn btn-primary" onclick={openCreate}>{i18n.t('admin.addSize')}</button>
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingSizes')} />
  {:else if sizes.length === 0}
    <EmptyState
      title={i18n.t('admin.noSizesYet')}
      description={i18n.t('admin.addSizesForProducts')}
      icon="📐"
    />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>{i18n.t('admin.name')}</th>
            <th>{i18n.t('admin.gender')}</th>
            <th>{i18n.t('admin.actions')}</th>
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
                <button class="btn btn-sm btn-ghost" onclick={() => openEdit(size)}>
                  {i18n.t('common.edit')}
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  onclick={() => confirmDelete(size)}
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
  title={modalMode === 'create' ? i18n.t('admin.newSize') : i18n.t('admin.editSize')}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="sizeName">{i18n.t('admin.name')}</label>
    <input id="sizeName" class="input" type="text" bind:value={modalName} placeholder="XS, S, M, L..." />
  </div>
  <div class="form-group mt-4">
    <label class="form-label" for="sizeGender">{i18n.t('admin.gender')}</label>
    <select id="sizeGender" class="input" bind:value={modalGender}>
      <option value="Unisex">{i18n.t('admin.genderUnisex')}</option>
      <option value="Female">{i18n.t('admin.genderFemale')}</option>
      <option value="Male">{i18n.t('admin.genderMale')}</option>
    </select>
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-primary" onclick={saveSize} disabled={saving}>
      {saving ? i18n.t('common.saving') : i18n.t('common.save')}
    </button>
  </div>
</Modal>

<Modal open={showDeleteModal} title={i18n.t('admin.deleteSizeQuestion')} onclose={() => (showDeleteModal = false)}>
  <p>{i18n.t('admin.confirmDeleteSize', { name: deleteTarget?.name ?? '' })}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-danger" onclick={executeDelete}>{i18n.t('common.delete')}</button>
  </div>
</Modal>
{:else}
  <div style="padding: 2rem; text-align: center;">
    <p>Access denied. Admin only.</p>
    <a href="/admin">← Back to Dashboard</a>
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
