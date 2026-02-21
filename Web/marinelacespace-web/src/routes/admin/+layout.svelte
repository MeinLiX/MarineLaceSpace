<script lang="ts">
  import type { Snippet } from 'svelte';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { authStore } from '$stores/auth.svelte';
  import { i18n } from '$i18n/index.svelte';

  let { children }: { children: Snippet } = $props();

  let sidebarOpen = $state(false);
  let currentPath = $derived($page.url.pathname);

  $effect(() => {
    if (!authStore.isLoading && !authStore.isAdmin && !authStore.isSeller) {
      goto('/');
    }
  });

  $effect(() => {
    currentPath;
    sidebarOpen = false;
  });

  let navItems = $derived.by(() => {
    const items = [
      { icon: '📊', label: i18n.t('admin.dashboard'), href: '/admin' },
      { icon: '📦', label: i18n.t('admin.products'), href: '/admin/products' },
      { icon: '📋', label: i18n.t('admin.orders'), href: '/admin/orders' },
      { icon: '🏪', label: i18n.t('admin.shops'), href: '/admin/shops' },
      { icon: '⭐', label: i18n.t('admin.reviews'), href: '/admin/reviews' },
    ];

    if (authStore.isAdmin) {
      items.splice(2, 0, { icon: '📂', label: i18n.t('admin.categories'), href: '/admin/categories' });
      items.push({ icon: '👥', label: i18n.t('admin.users'), href: '/admin/users' });
    }

    return items;
  });

  let dictionaryItems = $derived.by(() => {
    if (!authStore.isAdmin) return [];
    return [
      { icon: '📐', label: i18n.t('admin.sizes'), href: '/admin/dictionaries/sizes' },
      { icon: '🎨', label: i18n.t('admin.colors'), href: '/admin/dictionaries/colors' },
      { icon: '🧵', label: i18n.t('admin.materials'), href: '/admin/dictionaries/materials' },
    ];
  });

  function isLinkActive(href: string): boolean {
    if (href === '/admin') return currentPath === '/admin';
    return currentPath.startsWith(href);
  }
</script>

{#if authStore.isAdmin || authStore.isSeller}
  <div class="admin-wrapper">
    <button
      class="hamburger"
      onclick={() => (sidebarOpen = !sidebarOpen)}
      aria-label={i18n.t('admin.openMenu')}
    >
      <span class="hamburger-line"></span>
      <span class="hamburger-line"></span>
      <span class="hamburger-line"></span>
    </button>

    {#if sidebarOpen}
      <div
        class="sidebar-overlay"
        onclick={() => (sidebarOpen = false)}
        onkeydown={(e) => e.key === 'Escape' && (sidebarOpen = false)}
        role="presentation"
      ></div>
    {/if}

    <aside class="admin-sidebar" class:open={sidebarOpen}>
      <div class="sidebar-brand">
        <a href="/admin">MLS Admin</a>
      </div>

      <nav class="sidebar-nav">
        {#each navItems as item}
          <a
            href={item.href}
            class="nav-link"
            class:active={isLinkActive(item.href)}
          >
            <span class="nav-icon">{item.icon}</span>
            <span class="nav-label">{item.label}</span>
          </a>
        {/each}

        {#if dictionaryItems.length > 0}
          <div class="nav-separator"></div>

          {#each dictionaryItems as item}
            <a
              href={item.href}
              class="nav-link"
              class:active={isLinkActive(item.href)}
            >
              <span class="nav-icon">{item.icon}</span>
              <span class="nav-label">{item.label}</span>
            </a>
          {/each}
        {/if}
      </nav>

      <div class="sidebar-user">
        {#if authStore.currentUser}
          <div class="user-info">
            <span class="user-name">
              {authStore.currentUser.firstName} {authStore.currentUser.lastName}
            </span>
            <span class="user-role">{authStore.isAdmin ? i18n.t('admin.administrator') : i18n.t('admin.seller')}</span>
          </div>
          <button class="btn-logout" onclick={() => authStore.logout()}>
            {i18n.t('common.logout')}
          </button>
        {/if}
      </div>
    </aside>

    <main class="admin-content">
      {@render children()}
    </main>
  </div>
{/if}

<style>
  .admin-wrapper {
    display: flex;
    min-height: 100vh;
    background: var(--color-background);
  }

  .admin-sidebar {
    width: 240px;
    flex-shrink: 0;
    background: var(--color-accent);
    color: #fff;
    display: flex;
    flex-direction: column;
    overflow-y: auto;
  }

  .sidebar-brand {
    padding: var(--space-5) var(--space-4);
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  .sidebar-brand a {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 700;
    color: var(--color-secondary);
    text-decoration: none;
    letter-spacing: 0.05em;
  }

  .sidebar-nav {
    flex: 1;
    padding: var(--space-3) 0;
  }

  .nav-link {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    color: rgba(255, 255, 255, 0.7);
    text-decoration: none;
    font-size: 0.875rem;
    transition: all var(--transition-fast);
    border-left: 3px solid transparent;
  }

  .nav-link:hover {
    color: #fff;
    background: rgba(255, 255, 255, 0.08);
  }

  .nav-link.active {
    color: #fff;
    background: rgba(255, 255, 255, 0.12);
    border-left-color: var(--color-secondary);
  }

  .nav-icon {
    font-size: 1.125rem;
    width: 24px;
    text-align: center;
    flex-shrink: 0;
  }

  .nav-label {
    white-space: nowrap;
  }

  .nav-separator {
    height: 1px;
    background: rgba(255, 255, 255, 0.1);
    margin: var(--space-3) var(--space-4);
  }

  .sidebar-user {
    padding: var(--space-4);
    border-top: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .user-info {
    display: flex;
    flex-direction: column;
  }

  .user-name {
    font-size: 0.8125rem;
    font-weight: 600;
    color: #fff;
  }

  .user-role {
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.5);
  }

  .btn-logout {
    background: rgba(255, 255, 255, 0.1);
    color: rgba(255, 255, 255, 0.7);
    border: none;
    padding: var(--space-2) var(--space-3);
    border-radius: var(--radius-sm);
    font-size: 0.75rem;
    cursor: pointer;
    transition: all var(--transition-fast);
    text-align: center;
  }

  .btn-logout:hover {
    background: rgba(255, 255, 255, 0.2);
    color: #fff;
  }

  .admin-content {
    flex: 1;
    padding: var(--space-6);
    min-width: 0;
    overflow-x: hidden;
  }

  .hamburger {
    display: none;
    position: fixed;
    top: var(--space-3);
    left: var(--space-3);
    z-index: 200;
    background: var(--color-accent);
    border: none;
    border-radius: var(--radius-sm);
    padding: var(--space-2);
    cursor: pointer;
    flex-direction: column;
    gap: 4px;
  }

  .hamburger-line {
    display: block;
    width: 20px;
    height: 2px;
    background: #fff;
    border-radius: 1px;
  }

  .sidebar-overlay {
    display: none;
  }

  @media (max-width: 768px) {
    .hamburger {
      display: flex;
    }

    .admin-sidebar {
      position: fixed;
      top: 0;
      left: 0;
      height: 100vh;
      z-index: 150;
      transform: translateX(-100%);
      transition: transform var(--transition-base);
    }

    .admin-sidebar.open {
      transform: translateX(0);
    }

    .sidebar-overlay {
      display: block;
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.4);
      z-index: 140;
    }

    .admin-content {
      padding: var(--space-4);
      padding-top: var(--space-12);
    }
  }
</style>
