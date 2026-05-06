<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let {
    accept = 'image/*',
    maxSizeMB = 10,
    uploading = false,
    preview = undefined,
    existingUrl = undefined,
    compact = false,
    label = undefined,
    onfile,
    onclear,
  }: {
    accept?: string;
    maxSizeMB?: number;
    uploading?: boolean;
    preview?: string | null;
    existingUrl?: string | null;
    compact?: boolean;
    label?: string;
    onfile: (file: File) => void;
    onclear?: () => void;
  } = $props();

  let dragOver = $state(false);
  let fileInput = $state<HTMLInputElement | null>(null);
  let error = $state('');

  function validateAndEmit(file: File) {
    error = '';
    if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
      error = i18n.t('upload.fileTooLarge', { max: `${maxSizeMB}MB` });
      return;
    }
    if (accept && accept !== '*') {
      const acceptedTypes = accept.split(',').map((t) => t.trim());
      const isValid = acceptedTypes.some((type) => {
        if (type.endsWith('/*')) {
          return file.type.startsWith(type.replace('/*', '/'));
        }
        return file.type === type || file.name.endsWith(type);
      });
      if (!isValid) {
        error = i18n.t('upload.invalidFileType');
        return;
      }
    }
    onfile(file);
  }

  function handleDrop(event: DragEvent) {
    event.preventDefault();
    dragOver = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) validateAndEmit(file);
  }

  function handleDragOver(event: DragEvent) {
    event.preventDefault();
    dragOver = true;
  }

  function handleDragLeave() {
    dragOver = false;
  }

  function handleClick() {
    fileInput?.click();
  }

  function handleFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) validateAndEmit(file);
    if (input) input.value = '';
  }

  function handleClear() {
    error = '';
    onclear?.();
  }

  let displayUrl = $derived(preview || existingUrl);
  let hasImage = $derived(!!displayUrl);
</script>

<div class="dropzone-wrapper" class:compact>
  {#if hasImage}
    <div class="preview-container" class:compact>
      <img src={displayUrl} alt="Preview" class="preview-image" class:compact />
      {#if !uploading}
        <button type="button" class="clear-button" onclick={handleClear} title={i18n.t('common.delete')}>
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      {/if}
      {#if uploading}
        <div class="upload-overlay">
          <div class="upload-spinner"></div>
        </div>
      {/if}
    </div>
  {/if}

  <!-- svelte-ignore a11y_no_static_element_interactions -->
  <div
    class="dropzone"
    class:drag-over={dragOver}
    class:has-image={hasImage}
    class:compact
    class:uploading
    ondrop={handleDrop}
    ondragover={handleDragOver}
    ondragleave={handleDragLeave}
    onclick={handleClick}
    onkeydown={(e) => e.key === 'Enter' && handleClick()}
    role="button"
    tabindex="0"
    aria-label={label || i18n.t('upload.dropzoneLabel')}
  >
    <input
      bind:this={fileInput}
      type="file"
      {accept}
      class="hidden-input"
      onchange={handleFileChange}
    />

    {#if uploading}
      <div class="dropzone-content">
        <div class="upload-indicator">
          <div class="upload-spinner-inline"></div>
        </div>
        <span class="dropzone-text">{i18n.t('common.uploading')}</span>
      </div>
    {:else}
      <div class="dropzone-content">
        <div class="dropzone-icon">
          <svg viewBox="0 0 24 24" width={compact ? 20 : 28} height={compact ? 20 : 28} fill="none" stroke="currentColor" stroke-width="1.5">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
            <polyline points="17 8 12 3 7 8" />
            <line x1="12" y1="3" x2="12" y2="15" />
          </svg>
        </div>
        {#if !compact}
          <span class="dropzone-text">
            {hasImage ? i18n.t('upload.replaceFile') : i18n.t('upload.dropOrClick')}
          </span>
          <span class="dropzone-hint">
            {i18n.t('upload.maxSize', { max: `${maxSizeMB}MB` })}
          </span>
        {:else}
          <span class="dropzone-text-compact">
            {hasImage ? i18n.t('upload.replace') : i18n.t('upload.choose')}
          </span>
        {/if}
      </div>
    {/if}
  </div>

  {#if error}
    <p class="dropzone-error">{error}</p>
  {/if}
</div>

<style>
  .dropzone-wrapper {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .dropzone-wrapper.compact {
    flex-direction: row;
    align-items: flex-start;
    gap: var(--space-3);
  }

  .preview-container {
    position: relative;
    display: inline-block;
    border-radius: var(--radius-md);
    overflow: hidden;
    border: 1px solid var(--color-border);
  }

  .preview-container.compact {
    max-width: 120px;
    flex-shrink: 0;
  }

  .preview-image {
    display: block;
    max-width: 200px;
    max-height: 200px;
    object-fit: cover;
    border-radius: var(--radius-md);
  }

  .preview-image.compact {
    max-width: 120px;
    max-height: 80px;
  }

  .clear-button {
    position: absolute;
    top: 6px;
    right: 6px;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    background: rgba(0, 0, 0, 0.6);
    border: none;
    color: white;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    transition: background var(--transition-fast);
    z-index: 2;
  }

  .clear-button:hover {
    background: rgba(196, 85, 90, 0.9);
  }

  .upload-overlay {
    position: absolute;
    inset: 0;
    background: rgba(255, 255, 255, 0.7);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1;
  }

  .hidden-input {
    display: none;
  }

  .dropzone {
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px dashed var(--color-border);
    border-radius: var(--radius-md);
    padding: var(--space-6) var(--space-4);
    cursor: pointer;
    transition: all var(--transition-fast);
    background: var(--color-surface);
    min-height: 100px;
  }

  .dropzone.compact {
    padding: var(--space-3) var(--space-4);
    min-height: 60px;
    flex: 1;
  }

  .dropzone.has-image {
    border-style: solid;
    border-color: var(--color-border-light);
    min-height: 60px;
    padding: var(--space-3) var(--space-4);
  }

  .dropzone:hover:not(.uploading) {
    border-color: var(--color-primary);
    background: var(--color-surface-hover);
  }

  .dropzone.drag-over {
    border-color: var(--color-primary);
    background: rgba(139, 94, 107, 0.06);
    box-shadow: 0 0 0 3px rgba(139, 94, 107, 0.1);
  }

  .dropzone.uploading {
    cursor: not-allowed;
    opacity: 0.7;
  }

  .dropzone-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-2);
    text-align: center;
  }

  .dropzone-icon {
    color: var(--color-text-muted);
  }

  .drag-over .dropzone-icon {
    color: var(--color-primary);
  }

  .dropzone-text {
    font-size: 0.875rem;
    color: var(--color-text);
    font-weight: 500;
  }

  .dropzone-text-compact {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  .dropzone-hint {
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  .dropzone-error {
    font-size: 0.75rem;
    color: var(--color-error, #c4555a);
    margin: 0;
  }

  .upload-spinner,
  .upload-spinner-inline {
    width: 24px;
    height: 24px;
    border: 3px solid var(--color-border-light);
    border-top-color: var(--color-primary);
    border-radius: 50%;
    animation: spin 0.7s linear infinite;
  }

  .upload-indicator {
    display: flex;
    align-items: center;
    justify-content: center;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
