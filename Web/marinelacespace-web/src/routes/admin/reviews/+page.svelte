<script lang="ts">
  import * as catalogApi from '$api/catalog';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import Pagination from '$components/Pagination.svelte';
  import EmptyState from '$components/EmptyState.svelte';
  import Modal from '$components/Modal.svelte';
  import ReviewStars from '$components/ReviewStars.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { authStore } from '$lib/stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';
  import type { ProductReview } from '$types';

  // TODO: For sellers, reviews should be filtered server-side by shopId/products.
  // The current getReviews() endpoint doesn't support a shopId filter, so for now
  // all reviews are shown. Backend filtering is needed to scope reviews to seller's products.

  let loading = $state(true);
  let reviews = $state<(ProductReview & { productName?: string })[]>([]);
  let totalPages = $state(1);
  let currentPage = $state(1);
  let ratingFilter = $state<number | null>(null);

  let showViewModal = $state(false);
  let viewTarget = $state<ProductReview | null>(null);

  let showDeleteModal = $state(false);
  let deleteTarget = $state<ProductReview | null>(null);

  $effect(() => {
    loadReviews(currentPage, ratingFilter);
  });

  async function loadReviews(page: number, rating: number | null) {
    try {
      loading = true;
      const result = await catalogApi.getReviews({
        page,
        pageSize: 20,
        rating: rating ?? undefined,
      });
      reviews = result.items;
      totalPages = result.totalPages;
    } catch {
      notificationStore.error(i18n.t('admin.errorLoadingReviews'));
    } finally {
      loading = false;
    }
  }

  function handlePageChange(page: number) {
    currentPage = page;
  }

  function viewReview(review: ProductReview) {
    viewTarget = review;
    showViewModal = true;
  }

  function confirmDelete(review: ProductReview) {
    deleteTarget = review;
    showDeleteModal = true;
  }

  async function executeDelete() {
    if (!deleteTarget) return;
    try {
      await catalogApi.deleteReview(deleteTarget.productId, deleteTarget.id);
      notificationStore.success(i18n.t('admin.reviewDeleted'));
      showDeleteModal = false;
      deleteTarget = null;
      loadReviews(currentPage, ratingFilter);
    } catch {
      notificationStore.error(i18n.t('admin.errorDeletingReview'));
    }
  }

  function formatDate(date: string): string {
    return new Date(date).toLocaleDateString('uk-UA');
  }

  function truncate(text: string, maxLength = 80): string {
    if (text.length <= maxLength) return text;
    return text.slice(0, maxLength) + '…';
  }
</script>

<div class="reviews-page">
  <h1 class="page-title">{i18n.t('admin.reviewModeration')}</h1>

  <div class="toolbar">
    <select
      class="input input-sm filter-select"
      value={ratingFilter ?? ''}
      onchange={(e) => {
        const v = e.currentTarget.value;
        ratingFilter = v ? parseInt(v) : null;
        currentPage = 1;
      }}
    >
      <option value="">{i18n.t('admin.allRatings')}</option>
      <option value="5">⭐⭐⭐⭐⭐ (5)</option>
      <option value="4">⭐⭐⭐⭐ (4)</option>
      <option value="3">⭐⭐⭐ (3)</option>
      <option value="2">⭐⭐ (2)</option>
      <option value="1">⭐ (1)</option>
    </select>
  </div>

  {#if loading}
    <LoadingSpinner message={i18n.t('admin.loadingReviews')} />
  {:else if reviews.length === 0}
    <EmptyState title={i18n.t('admin.noReviewsFound')} icon="⭐" />
  {:else}
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>{i18n.t('admin.product')}</th>
            <th>{i18n.t('admin.author')}</th>
            <th>{i18n.t('admin.rating')}</th>
            <th>{i18n.t('admin.text')}</th>
            <th>{i18n.t('admin.date')}</th>
            <th>{i18n.t('admin.actions')}</th>
          </tr>
        </thead>
        <tbody>
          {#each reviews as review}
            <tr>
              <td>
                <a href="/admin/products/{review.productId}" class="product-link">
                  {(review as any).productName ?? review.productId.slice(0, 8)}
                </a>
              </td>
              <td>{review.guestName ?? i18n.t('admin.userDefault')}</td>
              <td>
                <ReviewStars rating={review.rating} size="sm" />
              </td>
              <td class="cell-text">{truncate(review.text)}</td>
              <td class="cell-nowrap">{formatDate(review.createdAt)}</td>
              <td class="cell-actions">
                <button class="btn btn-sm btn-ghost" onclick={() => viewReview(review)}>
                  {i18n.t('admin.view')}
                </button>
                <button
                  class="btn btn-sm btn-ghost btn-danger-text"
                  onclick={() => confirmDelete(review)}
                >
                  {i18n.t('common.delete')}
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

<Modal open={showViewModal} title={i18n.t('admin.review')} onclose={() => (showViewModal = false)}>
  {#if viewTarget}
    <div class="review-detail">
      <div class="review-meta">
        <ReviewStars rating={viewTarget.rating} />
        <span class="text-sm text-muted">{formatDate(viewTarget.createdAt)}</span>
      </div>
      {#if viewTarget.title}
        <h4 class="review-title">{viewTarget.title}</h4>
      {/if}
      <p class="review-text">{viewTarget.text}</p>
      <div class="review-footer">
        <span class="text-sm">
          {i18n.t('admin.author')}: <strong>{viewTarget.guestName ?? i18n.t('admin.userDefault')}</strong>
        </span>
        {#if viewTarget.isVerifiedPurchase}
          <span class="badge badge-success">{i18n.t('admin.verifiedPurchase')}</span>
        {/if}
      </div>
    </div>
  {/if}
</Modal>

<Modal open={showDeleteModal} title={i18n.t('admin.deleteReviewQuestion')} onclose={() => (showDeleteModal = false)}>
  <p>{i18n.t('admin.confirmDeleteReview')}</p>
  <p class="text-sm text-muted mt-2">{i18n.t('admin.actionIrreversible')}</p>
  <div class="modal-actions">
    <button class="btn btn-outline" onclick={() => (showDeleteModal = false)}>{i18n.t('common.cancel')}</button>
    <button class="btn btn-danger" onclick={executeDelete}>{i18n.t('common.delete')}</button>
  </div>
</Modal>

<style>
  .page-title {
    font-size: 1.75rem;
    margin-bottom: var(--space-6);
  }

  .toolbar {
    margin-bottom: var(--space-4);
  }

  .filter-select {
    width: auto;
    min-width: 200px;
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

  .product-link {
    color: var(--color-primary);
    text-decoration: none;
  }

  .product-link:hover {
    text-decoration: underline;
  }

  .cell-text {
    max-width: 300px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: var(--color-text-light);
    font-size: 0.8125rem;
  }

  .cell-nowrap {
    white-space: nowrap;
  }

  .cell-actions {
    white-space: nowrap;
  }

  .review-detail {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .review-meta {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .review-title {
    font-size: 1rem;
    font-weight: 600;
    font-family: var(--font-body);
  }

  .review-text {
    font-size: 0.9375rem;
    line-height: 1.6;
    color: var(--color-text);
    white-space: pre-wrap;
  }

  .review-footer {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding-top: var(--space-3);
    border-top: 1px solid var(--color-border-light);
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
