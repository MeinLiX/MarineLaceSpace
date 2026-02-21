<script lang="ts">
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { ShippingAddress } from '$types';

  interface SavedAddress extends ShippingAddress {
    id: string;
    label: string;
    isDefault: boolean;
  }

  const STORAGE_KEY = 'mls-saved-addresses';

  let addresses = $state<SavedAddress[]>([]);
  let showFormModal = $state(false);
  let showDeleteModal = $state(false);
  let editingAddress = $state<SavedAddress | null>(null);
  let deleteTarget = $state<SavedAddress | null>(null);

  let formLabel = $state('');
  let formFullName = $state('');
  let formAddressLine1 = $state('');
  let formAddressLine2 = $state('');
  let formCity = $state('');
  let formState = $state('');
  let formPostalCode = $state('');
  let formCountry = $state('');
  let formPhone = $state('');
  let formIsDefault = $state(false);

  $effect(() => {
    loadAddresses();
  });

  function loadAddresses() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      addresses = raw ? JSON.parse(raw) : [];
    } catch {
      addresses = [];
    }
  }

  function saveAddresses() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(addresses));
  }

  function generateId(): string {
    return crypto.randomUUID();
  }

  function resetForm() {
    formLabel = '';
    formFullName = '';
    formAddressLine1 = '';
    formAddressLine2 = '';
    formCity = '';
    formState = '';
    formPostalCode = '';
    formCountry = '';
    formPhone = '';
    formIsDefault = false;
  }

  function openAddModal() {
    editingAddress = null;
    resetForm();
    if (addresses.length === 0) {
      formIsDefault = true;
    }
    showFormModal = true;
  }

  function openEditModal(address: SavedAddress) {
    editingAddress = address;
    formLabel = address.label;
    formFullName = address.fullName;
    formAddressLine1 = address.addressLine1;
    formAddressLine2 = address.addressLine2 ?? '';
    formCity = address.city;
    formState = address.state;
    formPostalCode = address.postalCode;
    formCountry = address.country;
    formPhone = address.phone;
    formIsDefault = address.isDefault;
    showFormModal = true;
  }

  function saveAddress() {
    if (!formFullName.trim() || !formAddressLine1.trim() || !formCity.trim() || !formPostalCode.trim() || !formCountry.trim()) {
      return;
    }

    const entry: SavedAddress = {
      id: editingAddress?.id ?? generateId(),
      label: formLabel.trim() || 'Home',
      fullName: formFullName.trim(),
      addressLine1: formAddressLine1.trim(),
      addressLine2: formAddressLine2.trim() || undefined,
      city: formCity.trim(),
      state: formState.trim(),
      postalCode: formPostalCode.trim(),
      country: formCountry.trim(),
      phone: formPhone.trim(),
      isDefault: formIsDefault,
    };

    if (entry.isDefault) {
      addresses = addresses.map((a) => ({ ...a, isDefault: false }));
    }

    if (editingAddress) {
      addresses = addresses.map((a) => (a.id === entry.id ? entry : a));
    } else {
      addresses = [...addresses, entry];
    }

    saveAddresses();
    notificationStore.success(i18n.t('account.addressSaved'));
    showFormModal = false;
  }

  function confirmDelete(address: SavedAddress) {
    deleteTarget = address;
    showDeleteModal = true;
  }

  function executeDelete() {
    if (!deleteTarget) return;
    const wasDefault = deleteTarget.isDefault;
    addresses = addresses.filter((a) => a.id !== deleteTarget!.id);
    if (wasDefault && addresses.length > 0) {
      addresses[0].isDefault = true;
    }
    saveAddresses();
    notificationStore.success(i18n.t('account.addressDeleted'));
    showDeleteModal = false;
    deleteTarget = null;
  }

  function setAsDefault(address: SavedAddress) {
    addresses = addresses.map((a) => ({
      ...a,
      isDefault: a.id === address.id,
    }));
    saveAddresses();
    notificationStore.success(i18n.t('account.addressSaved'));
  }

  let modalTitle = $derived(
    editingAddress ? i18n.t('account.editAddress') : i18n.t('account.addAddress')
  );
</script>

<svelte:head>
  <title>{i18n.t('account.myAddresses')} — MarineLaceSpace</title>
</svelte:head>

<div class="addresses-page">
  <div class="page-header">
    <h1 class="page-heading">{i18n.t('account.myAddresses')}</h1>
    <button class="btn btn-primary" onclick={openAddModal}>
      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <path d="M12 5v14M5 12h14" />
      </svg>
      {i18n.t('account.addAddress')}
    </button>
  </div>

  {#if addresses.length === 0}
    <EmptyState
      title={i18n.t('account.noAddresses')}
      description={i18n.t('account.noAddressesDescription')}
      icon="📍"
    />
  {:else}
    <div class="address-grid">
      {#each addresses as address (address.id)}
        <div class="address-card card" class:is-default={address.isDefault}>
          <div class="card-top">
            <div class="card-label-row">
              <span class="address-label">{address.label}</span>
              {#if address.isDefault}
                <span class="default-badge">{i18n.t('account.defaultAddress')}</span>
              {/if}
            </div>
          </div>

          <div class="card-body">
            <p class="address-name">{address.fullName}</p>
            <p class="address-line">{address.addressLine1}</p>
            {#if address.addressLine2}
              <p class="address-line">{address.addressLine2}</p>
            {/if}
            <p class="address-line">{address.city}, {address.state} {address.postalCode}</p>
            <p class="address-line">{address.country}</p>
            {#if address.phone}
              <p class="address-phone">{address.phone}</p>
            {/if}
          </div>

          <div class="card-actions">
            {#if !address.isDefault}
              <button class="btn btn-sm btn-ghost" onclick={() => setAsDefault(address)}>
                {i18n.t('account.setAsDefault')}
              </button>
            {/if}
            <button class="btn btn-sm btn-ghost" onclick={() => openEditModal(address)}>
              {i18n.t('common.edit')}
            </button>
            <button class="btn btn-sm btn-ghost btn-danger-text" onclick={() => confirmDelete(address)}>
              {i18n.t('common.delete')}
            </button>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>

<!-- Add / Edit Modal -->
<Modal open={showFormModal} title={modalTitle} onclose={() => (showFormModal = false)}>
  <div class="address-form">
    <div class="form-group">
      <label for="addr-label" class="form-label">{i18n.t('account.addressLabel')}</label>
      <input id="addr-label" type="text" class="input" placeholder="Home, Work…" bind:value={formLabel} />
    </div>

    <div class="form-group">
      <label for="addr-fullName" class="form-label">{i18n.t('checkout.fullName')}</label>
      <input id="addr-fullName" type="text" class="input" bind:value={formFullName} />
    </div>

    <div class="form-group">
      <label for="addr-line1" class="form-label">{i18n.t('checkout.addressPlaceholder')}</label>
      <input id="addr-line1" type="text" class="input" bind:value={formAddressLine1} />
    </div>

    <div class="form-group">
      <label for="addr-line2" class="form-label">{i18n.t('checkout.addressLine2')}</label>
      <input id="addr-line2" type="text" class="input" bind:value={formAddressLine2} />
    </div>

    <div class="form-row">
      <div class="form-group flex-1">
        <label for="addr-city" class="form-label">{i18n.t('checkout.city')}</label>
        <input id="addr-city" type="text" class="input" bind:value={formCity} />
      </div>
      <div class="form-group flex-1">
        <label for="addr-state" class="form-label">{i18n.t('checkout.state')}</label>
        <input id="addr-state" type="text" class="input" bind:value={formState} />
      </div>
    </div>

    <div class="form-row">
      <div class="form-group flex-1">
        <label for="addr-postal" class="form-label">{i18n.t('checkout.postalCode')}</label>
        <input id="addr-postal" type="text" class="input" bind:value={formPostalCode} />
      </div>
      <div class="form-group flex-1">
        <label for="addr-country" class="form-label">{i18n.t('checkout.country')}</label>
        <input id="addr-country" type="text" class="input" bind:value={formCountry} />
      </div>
    </div>

    <div class="form-group">
      <label for="addr-phone" class="form-label">{i18n.t('checkout.phone')}</label>
      <input id="addr-phone" type="tel" class="input" bind:value={formPhone} />
    </div>

    <div class="form-group checkbox-group">
      <label class="checkbox-label">
        <input type="checkbox" bind:checked={formIsDefault} />
        <span>{i18n.t('account.setAsDefault')}</span>
      </label>
    </div>

    <div class="modal-actions">
      <button class="btn btn-outline" onclick={() => (showFormModal = false)}>
        {i18n.t('common.cancel')}
      </button>
      <button class="btn btn-primary" onclick={saveAddress}>
        {editingAddress ? i18n.t('common.save') : i18n.t('account.addAddress')}
      </button>
    </div>
  </div>
</Modal>

<!-- Delete Confirmation Modal -->
<Modal open={showDeleteModal} title={i18n.t('account.deleteAddress')} onclose={() => (showDeleteModal = false)}>
  <p>{i18n.t('account.confirmDeleteAddress')}</p>
  <p class="text-sm text-muted mt-2">{i18n.t('account.deleteAddressWarning')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>
      {i18n.t('common.cancel')}
    </button>
    <button class="btn btn-danger" onclick={executeDelete}>
      {i18n.t('common.delete')}
    </button>
  </div>
</Modal>

<style>
  .addresses-page {
    max-width: 960px;
  }

  .page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
    gap: var(--space-3);
  }

  .page-heading {
    font-family: var(--font-display);
    font-size: 1.5rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .page-header .btn-primary {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
  }

  /* Address grid */
  .address-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: var(--space-4);
  }

  .address-card {
    display: flex;
    flex-direction: column;
    padding: var(--space-5);
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    transition: border-color 0.15s ease;
  }

  .address-card.is-default {
    border-color: var(--color-primary);
  }

  .card-top {
    margin-bottom: var(--space-3);
  }

  .card-label-row {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }

  .address-label {
    font-family: var(--font-display);
    font-size: 1rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .default-badge {
    display: inline-flex;
    align-items: center;
    padding: 2px 10px;
    font-size: 0.7rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-primary-dark);
    background: rgba(139, 94, 107, 0.1);
    border-radius: var(--radius-full);
  }

  .card-body {
    flex: 1;
    margin-bottom: var(--space-4);
  }

  .address-name {
    font-weight: 500;
    color: var(--color-text);
    margin-bottom: var(--space-1);
  }

  .address-line {
    font-size: 0.875rem;
    color: var(--color-text-light);
    line-height: 1.5;
  }

  .address-phone {
    font-size: 0.875rem;
    color: var(--color-text-light);
    margin-top: var(--space-2);
  }

  .card-actions {
    display: flex;
    gap: var(--space-2);
    border-top: 1px solid var(--color-border-light);
    padding-top: var(--space-3);
    flex-wrap: wrap;
  }

  /* Form */
  .address-form {
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

  .form-row {
    display: flex;
    gap: var(--space-4);
  }

  .flex-1 {
    flex: 1;
  }

  .checkbox-group {
    margin-bottom: var(--space-2);
  }

  .checkbox-label {
    display: inline-flex;
    align-items: center;
    gap: var(--space-2);
    font-size: 0.875rem;
    color: var(--color-text);
    cursor: pointer;
  }

  .checkbox-label input[type='checkbox'] {
    accent-color: var(--color-primary);
    width: 16px;
    height: 16px;
  }

  /* Shared modal / button styles */
  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: var(--space-3);
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

  .text-sm {
    font-size: 0.875rem;
  }

  .text-muted {
    color: var(--color-text-muted);
  }

  .mt-2 {
    margin-top: var(--space-2);
  }

  @media (max-width: 640px) {
    .address-grid {
      grid-template-columns: 1fr;
    }

    .form-row {
      flex-direction: column;
      gap: 0;
    }

    .page-header {
      flex-direction: column;
      align-items: flex-start;
    }
  }
</style>
