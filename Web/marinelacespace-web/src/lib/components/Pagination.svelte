<script lang="ts">
  let {
    currentPage,
    totalPages,
    onPageChange
  }: {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
  } = $props();

  let pages = $derived(computePages(currentPage, totalPages));

  function computePages(current: number, total: number): (number | '...')[] {
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const result: (number | '...')[] = [1];

    if (current > 3) {
      result.push('...');
    }

    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    for (let i = start; i <= end; i++) {
      result.push(i);
    }

    if (current < total - 2) {
      result.push('...');
    }

    result.push(total);

    return result;
  }
</script>

{#if totalPages > 1}
  <nav class="pagination" aria-label="Pagination">
    <button
      class="page-btn prev"
      disabled={currentPage <= 1}
      on:click={() => onPageChange(currentPage - 1)}
      aria-label="Previous page"
    >
      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <polyline points="15 18 9 12 15 6" />
      </svg>
      <span class="btn-label">Previous</span>
    </button>

    <div class="page-numbers">
      {#each pages as page, i (i)}
        {#if page === '...'}
          <span class="page-ellipsis" aria-hidden="true">&hellip;</span>
        {:else}
          <button
            class="page-num"
            class:active={page === currentPage}
            on:click={() => onPageChange(page as number)}
            aria-label="Page {page}"
            aria-current={page === currentPage ? 'page' : undefined}
          >
            {page}
          </button>
        {/if}
      {/each}
    </div>

    <button
      class="page-btn next"
      disabled={currentPage >= totalPages}
      on:click={() => onPageChange(currentPage + 1)}
      aria-label="Next page"
    >
      <span class="btn-label">Next</span>
      <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <polyline points="9 18 15 12 9 6" />
      </svg>
    </button>
  </nav>
{/if}

<style>
  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-2);
    flex-wrap: wrap;
  }

  .page-btn {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    padding: var(--space-2) var(--space-3);
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
    background: none;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    cursor: pointer;
    transition: all var(--transition-fast);
    font-family: inherit;
  }

  .page-btn:hover:not(:disabled) {
    border-color: var(--color-primary-light);
    color: var(--color-primary);
    background-color: var(--color-surface-hover);
  }

  .page-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  .btn-label {
    display: none;
  }

  @media (min-width: 640px) {
    .btn-label {
      display: inline;
    }
  }

  .page-numbers {
    display: flex;
    align-items: center;
    gap: var(--space-1);
  }

  .page-num {
    display: flex;
    align-items: center;
    justify-content: center;
    min-width: 36px;
    height: 36px;
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
    background: none;
    border: 1px solid transparent;
    border-radius: var(--radius-md);
    cursor: pointer;
    transition: all var(--transition-fast);
    font-family: inherit;
    padding: 0 var(--space-2);
  }

  .page-num:hover:not(.active) {
    background-color: var(--color-surface-hover);
    border-color: var(--color-border-light);
  }

  .page-num.active {
    background-color: var(--color-primary);
    color: #FFFFFF;
    border-color: var(--color-primary);
    font-weight: 600;
  }

  .page-ellipsis {
    display: flex;
    align-items: center;
    justify-content: center;
    min-width: 36px;
    height: 36px;
    font-size: 0.875rem;
    color: var(--color-text-muted);
    user-select: none;
  }
</style>
