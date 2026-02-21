<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { Color } from '$types';

  let loading = $state(true);
  let colors = $state<Color[]>([]);

  // Modal state
  let showModal = $state(false);
  let modalMode = $state<'create' | 'edit'>('create');
  let editingId = $state<string | null>(null);
  let modalName = $state('');
  let modalHex = $state('#000000');
  let saving = $state(false);

  // Delete
  let showDeleteModal = $state(false);
  let deleteTarget = $state<Color | null>(null);

  $effect(() => {
    loadColors();
  });

  async function loadColors() {
    try {
      loading = true;
      colors = await catalogApi.getColors();
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingColors'));
    } finally {
      loading = false;
    }
  }

  function openCreate() {
    modalMode = 'create';
    editingId = null;
    modalName = '';
    modalHex = '#000000';
    showModal = true;
  }

  function openEdit(color: Color) {
    modalMode = 'edit';
    editingId = color.id;
    modalName = color.name;
    modalHex = color.hexCode;
    showModal = true;
  }

  function confirmDelete(color: Color) {
    deleteTarget = color;
    showDeleteModal = true;
  }

  async function saveColor() {
    if (!modalName.trim()) {
      notificationStore.warning(i18n.t('admin.enterColorName'));
      return;
    }
    if (!/^#[0-9a-fA-F]{6}$/.test(modalHex)) {
      notificationStore.warning(i18n.t('admin.enterValidHex'));
      return;
    }
    try {
      saving = true;
      if (modalMode === 'create') {
        await catalogApi.createColor({ name: modalName.trim(), hexCode: modalHex });
        notificationStore.success(i18n.t('admin.colorCreated'));
      } else if (editingId) {
        await catalogApi.updateColor(editingId, { name: modalName.trim(), hexCode: modalHex });
        notificationStore.success(i18n.t('admin.colorUpdated'));
      }
      showModal = false;
      loadColors();
    } catch {
      notificationStore.error(i18n.t('admin.errorSavingColor'));
    } finally {
      saving = false;
    }
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteColor(deleteTarget.id);
      notificationStore.success(i18n.t('admin.colorDeleted'));
      showDeleteModal = false;
      deleteTarget = null;
      loadColors();
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingColor'));
    }
  }
</script>

<div class="colors-page">
  <div class="page-header">
    <h1 class="page-title">{i18n.t('admin.colorsDictionary')}</h1>
    <button class="btn btn-primary" onclick={openCreate}>{i18n.t('admin.addColor')}</button>
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingColors')} />
  {:else if colors.length === 0}
    <EmptyState
      title={i18n.t('admin.noColorsYet')}
      description={i18n.t('admin.addColorsForProducts')}
      icon="🎨"
    />
  {:else}
    <div class="colors-grid">
      {#each colors as color}
        <div class="color-card card">
          <div class="color-swatch" style="background: {color.hexCode}"></div>
          <div class="color-info">
            <span class="color-name">{color.name}</span>
            <span class="color-hex">{color.hexCode}</span>
          </div>
          <div class="color-actions">
            <button class="btn btn-sm btn-ghost" onclick={() => openEdit(color)}>
              {i18n.t('common.edit')}
            </button>
            <button
              class="btn btn-sm btn-ghost btn-danger-text"
              onclick={() => confirmDelete(color)}
            >
              {i18n.t('common.delete')}
            </button>
          </div>
        </div>
      {/each}
    </div>

    <!-- Alternative table view -->
    <details class="table-toggle mt-6">
      <summary class="text-sm text-muted">{i18n.t('admin.tableView')}</summary>
      <div class="table-wrapper mt-2">
        <table class="data-table">
          <thead>
            <tr>
              <th>{i18n.t('admin.color')}</th>
              <th>{i18n.t('admin.name')}</th>
              <th>{i18n.t('admin.hexCode')}</th>
              <th>{i18n.t('admin.actions')}</th>
            </tr>
          </thead>
          <tbody>
            {#each colors as color}
              <tr>
                <td>
                  <span
                    class="swatch-circle"
                    style="background: {color.hexCode}"
                  ></span>
                </td>
                <td class="cell-name">{color.name}</td>
                <td class="cell-mono">{color.hexCode}</td>
                <td class="cell-actions">
                  <button class="btn btn-sm btn-ghost" onclick={() => openEdit(color)}>
                    {i18n.t('common.edit')}
                  </button>
                  <button
                    class="btn btn-sm btn-ghost btn-danger-text"
                    onclick={() => confirmDelete(color)}
                  >
                    {i18n.t('common.delete')}
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>
    </details>
  {/if}
</div>

<Modal
  open={showModal}
  title={modalMode === 'create' ? i18n.t('admin.newColor') : i18n.t('admin.editColor')}
  onclose={() => (showModal = false)}
>
  <div class="form-group">
    <label class="form-label" for="colorName">{i18n.t('admin.name')}</label>
    <input id="colorName" class="input" type="text" bind:value={modalName} placeholder={i18n.t('admin.colorNamePlaceholder')} />
  </div>
  <div class="form-group mt-4">
    <label class="form-label" for="colorHex">{i18n.t('admin.hexCode')}</label>
    <div class="hex-input-row">
      <input
        id="colorHex"
        class="input"
        type="text"
        bind:value={modalHex}
        placeholder="#FF0000"
        maxlength="7"
      />
      <input
        type="color"
        class="color-picker"
        bind:value={modalHex}
        aria-label={i18n.t('admin.pickColor')}
      />
    </div>
    <div class="color-preview" style="background: {modalHex}"></div>
  </div>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-primary" onclick={saveColor} disabled={saving}>
      {saving ? i18n.t('common.saving') : i18n.t('common.save')}
    </button>
  </div>
</Modal>

<Modal open={showDeleteModal} title={i18n.t('admin.deleteColorQuestion')} onclose={() => (showDeleteModal = false)}>
  <p>{i18n.t('admin.confirmDeleteColor', { name: deleteTarget?.name ?? '' })}</p>
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

  .colors-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: var(--space-4);
  }

  .color-card {
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .color-swatch {
    height: 80px;
    width: 100%;
  }

  .color-info {
    padding: var(--space-3) var(--space-4);
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .color-name {
    font-weight: 600;
    font-size: 0.9375rem;
  }

  .color-hex {
    font-family: monospace;
    font-size: 0.8125rem;
    color: var(--color-text-light);
  }

  .color-actions {
    padding: var(--space-2) var(--space-4) var(--space-3);
    display: flex;
    gap: var(--space-2);
  }

  .swatch-circle {
    display: inline-block;
    width: 24px;
    height: 24px;
    border-radius: var(--radius-full);
    border: 2px solid var(--color-border);
    vertical-align: middle;
  }

  .hex-input-row {
    display: flex;
    gap: var(--space-2);
    align-items: center;
  }

  .color-picker {
    width: 40px;
    height: 40px;
    padding: 0;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-sm);
    cursor: pointer;
    flex-shrink: 0;
  }

  .color-preview {
    width: 100%;
    height: 40px;
    border-radius: var(--radius-md);
    border: 1px solid var(--color-border);
    margin-top: var(--space-2);
  }

  .table-toggle {
    cursor: pointer;
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
  }

  .cell-mono {
    font-family: monospace;
    font-size: 0.8125rem;
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

  .mt-2 {
    margin-top: var(--space-2);
  }

  .mt-4 {
    margin-top: var(--space-4);
  }

  .mt-6 {
    margin-top: var(--space-6);
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
