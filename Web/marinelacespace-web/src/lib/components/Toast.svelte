<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  interface Notification {
    id: string;
    type: 'success' | 'error' | 'warning' | 'info';
    message: string;
    duration?: number;
  }

  // Standalone reactive store for notifications
  let notifications = $state<Notification[]>([]);
  let timers = new Map<string, ReturnType<typeof setTimeout>>();

  function addNotification(notification: Omit<Notification, 'id'>) {
    const id = crypto.randomUUID();
    const item: Notification = { id, ...notification };
    notifications = [...notifications, item];

    const duration = notification.duration ?? 5000;
    const timer = setTimeout(() => removeNotification(id), duration);
    timers.set(id, timer);
  }

  function removeNotification(id: string) {
    const timer = timers.get(id);
    if (timer) {
      clearTimeout(timer);
      timers.delete(id);
    }
    notifications = notifications.filter((n) => n.id !== id);
  }

  // Expose globally for other components via window (or import store)
  if (typeof window !== 'undefined') {
    (window as unknown as Record<string, unknown>).__mls_toast = addNotification;
  }

  const iconPaths: Record<string, string> = {
    success: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',
    error: 'M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z',
    warning: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
    info: 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
  };
</script>

{#if notifications.length > 0}
  <div class="toast-container" aria-live="polite" aria-label={i18n.t('common.notifications')}>
    {#each notifications as notification (notification.id)}
      <div class="toast toast-{notification.type}" role="alert">
        <svg class="toast-icon" viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
          <path d={iconPaths[notification.type]} />
        </svg>
        <p class="toast-message">{notification.message}</p>
        <button
          class="toast-close"
          aria-label={i18n.t('common.dismiss')}
          onclick={() => removeNotification(notification.id)}
        >
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </div>
    {/each}
  </div>
{/if}

<style>
  .toast-container {
    position: fixed;
    top: var(--space-6);
    right: var(--space-6);
    z-index: var(--z-toast);
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    max-width: 400px;
    width: calc(100% - var(--space-12));
    pointer-events: none;
  }

  .toast {
    display: flex;
    align-items: flex-start;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-lg);
    pointer-events: auto;
    animation: toastSlideIn var(--transition-base) ease both;
  }

  .toast-success {
    border-left: 3px solid var(--color-success);
  }

  .toast-error {
    border-left: 3px solid var(--color-error);
  }

  .toast-warning {
    border-left: 3px solid var(--color-warning);
  }

  .toast-info {
    border-left: 3px solid var(--color-info);
  }

  .toast-icon {
    flex-shrink: 0;
    margin-top: 2px;
  }

  .toast-success .toast-icon { color: var(--color-success); }
  .toast-error .toast-icon { color: var(--color-error); }
  .toast-warning .toast-icon { color: var(--color-warning); }
  .toast-info .toast-icon { color: var(--color-info); }

  .toast-message {
    flex: 1;
    font-size: 0.875rem;
    color: var(--color-text);
    line-height: 1.5;
  }

  .toast-close {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border-radius: var(--radius-sm);
    color: var(--color-text-muted);
    cursor: pointer;
    background: none;
    border: none;
    transition: color var(--transition-fast), background-color var(--transition-fast);
    padding: 0;
  }

  .toast-close:hover {
    color: var(--color-text);
    background-color: var(--color-surface-hover);
  }

  @keyframes toastSlideIn {
    from {
      opacity: 0;
      transform: translateX(20px);
    }
    to {
      opacity: 1;
      transform: translateX(0);
    }
  }
</style>
