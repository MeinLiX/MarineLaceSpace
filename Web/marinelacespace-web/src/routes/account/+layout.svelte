<script lang="ts">
  import type { Snippet } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { authStore } from '$lib/stores/auth';

  let { children }: { children: Snippet } = $props();

  let isMobileMenuOpen = $state(false);

  let currentPath = $derived($page.url.pathname);

  const navItems = [
    { label: 'Мій профіль', href: '/account', icon: 'user' },
    { label: 'Мої замовлення', href: '/account/orders', icon: 'orders' },
    { label: 'Мої адреси', href: '/account/addresses', icon: 'address' },
  ] as const;

  $effect(() => {
    if (!authStore.isLoading && !authStore.isAuthenticated) {
      goto('/auth/login');
    }
  });

  function handleLogout() {
    authStore.logout();
  }

  function isActive(href: string): boolean {
    if (href === '/account') return currentPath === '/account';
    return currentPath.startsWith(href);
  }
</script>

<svelte:head>
  <title>Мій акаунт — MarineLaceSpace</title>
</svelte:head>

{#if authStore.isAuthenticated}
  <div class="account-layout container">
    <nav class="account-sidebar" aria-label="Навігація акаунту">
      <button
        class="mobile-menu-toggle"
        onclick={() => isMobileMenuOpen = !isMobileMenuOpen}
        aria-expanded={isMobileMenuOpen}
        aria-controls="account-nav"
      >
        <span class="mobile-menu-label">Навігація</span>
        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"
          class:rotate={isMobileMenuOpen}
        >
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </button>

      <ul id="account-nav" class="nav-list" class:nav-open={isMobileMenuOpen} role="list">
        {#each navItems as item}
          <li>
            <a
              href={item.href}
              class="nav-link"
              class:active={isActive(item.href)}
              aria-current={isActive(item.href) ? 'page' : undefined}
              onclick={() => isMobileMenuOpen = false}
            >
              {#if item.icon === 'user'}
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
                  <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
                  <circle cx="12" cy="7" r="4"/>
                </svg>
              {:else if item.icon === 'orders'}
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
                  <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/>
                  <rect x="8" y="2" width="8" height="4" rx="1" ry="1"/>
                </svg>
              {:else if item.icon === 'address'}
                <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
                  <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 1 1 18 0z"/>
                  <circle cx="12" cy="10" r="3"/>
                </svg>
              {/if}
              <span>{item.label}</span>
            </a>
          </li>
        {/each}
        <li class="nav-divider"></li>
        <li>
          <button class="nav-link logout-btn" onclick={handleLogout}>
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.5" aria-hidden="true">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16 17 21 12 16 7"/>
              <line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
            <span>Вийти</span>
          </button>
        </li>
      </ul>
    </nav>

    <section class="account-content">
      {@render children()}
    </section>
  </div>
{/if}

<style>
  .account-layout {
    display: grid;
    grid-template-columns: 260px 1fr;
    gap: var(--space-8);
    padding-top: var(--space-8);
    padding-bottom: var(--space-16);
    min-height: calc(100vh - 200px);
  }

  .account-sidebar {
    position: sticky;
    top: var(--space-8);
    align-self: start;
  }

  .mobile-menu-toggle {
    display: none;
  }

  .nav-list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .nav-link {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    font-size: 0.9375rem;
    color: var(--color-text-light);
    text-decoration: none;
    border-radius: var(--radius-md);
    transition: all var(--transition-fast);
    border: none;
    background: none;
    cursor: pointer;
    width: 100%;
    text-align: left;
    font-family: inherit;
  }

  .nav-link:hover {
    color: var(--color-text);
    background: var(--color-surface-hover);
  }

  .nav-link.active {
    color: var(--color-primary);
    background: rgba(139, 94, 107, 0.08);
    font-weight: 500;
  }

  .nav-divider {
    height: 1px;
    background: var(--color-border);
    margin: var(--space-2) var(--space-4);
  }

  .logout-btn {
    color: var(--color-error);
  }

  .logout-btn:hover {
    color: var(--color-error);
    background: rgba(196, 85, 90, 0.06);
  }

  .account-content {
    min-width: 0;
  }

  .rotate {
    transform: rotate(180deg);
  }

  @media (max-width: 768px) {
    .account-layout {
      grid-template-columns: 1fr;
      gap: var(--space-4);
      padding-top: var(--space-4);
    }

    .account-sidebar {
      position: static;
    }

    .mobile-menu-toggle {
      display: flex;
      align-items: center;
      justify-content: space-between;
      width: 100%;
      padding: var(--space-3) var(--space-4);
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      cursor: pointer;
      font-family: inherit;
      font-size: 0.9375rem;
      color: var(--color-text);
    }

    .mobile-menu-toggle svg {
      transition: transform var(--transition-fast);
    }

    .nav-list {
      display: none;
      margin-top: var(--space-2);
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      padding: var(--space-2);
    }

    .nav-list.nav-open {
      display: flex;
    }
  }
</style>
