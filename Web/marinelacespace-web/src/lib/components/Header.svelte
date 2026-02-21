<script lang="ts">
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  import { authStore } from '$stores/auth.svelte';
  import { basketStore } from '$stores/basket.svelte';
  import { i18n } from '$i18n/index.svelte';

  let { onbasketclick }: { onbasketclick?: () => void } = $props();

  let searchQuery = $state('');
  let mobileMenuOpen = $state(false);
  let userMenuOpen = $state(false);
  let scrolled = $state(false);
  let langMenuOpen = $state(false);

  const navLinks = $derived([
    { label: i18n.t('nav.home'), href: '/' },
    { label: i18n.t('nav.catalog'), href: '/catalog' },
    { label: i18n.t('nav.shops'), href: '/shops' }
  ]);

  function handleScroll() {
    scrolled = window.scrollY > 10;
  }

  function handleSearch(e: Event) {
    e.preventDefault();
    if (searchQuery.trim()) {
      goto(`/catalog?search=${encodeURIComponent(searchQuery.trim())}`);
      searchQuery = '';
      mobileMenuOpen = false;
    }
  }

  function toggleMobileMenu() {
    mobileMenuOpen = !mobileMenuOpen;
    if (mobileMenuOpen) userMenuOpen = false;
  }

  function toggleUserMenu() {
    userMenuOpen = !userMenuOpen;
  }

  function toggleLangMenu() {
    langMenuOpen = !langMenuOpen;
  }

  function closeMenus() {
    mobileMenuOpen = false;
    userMenuOpen = false;
    langMenuOpen = false;
  }

  function logout() {
    closeMenus();
    authStore.logout();
  }

  $effect(() => {
    authStore.loadUser();
    basketStore.loadBasket();
  });

  $effect(() => {
    if (typeof window !== 'undefined') {
      window.addEventListener('scroll', handleScroll, { passive: true });
      return () => window.removeEventListener('scroll', handleScroll);
    }
  });
</script>

<svelte:window onclick={closeMenus} />

<header class="header" class:scrolled>
  <div class="header-inner container">
    <!-- Brand -->
    <a href="/" class="brand" aria-label="MarineLaceSpace — Home">
      <span class="brand-name">MarineLaceSpace</span>
    </a>

    <!-- Desktop Navigation -->
    <nav class="nav-desktop" aria-label="Main navigation">
      {#each navLinks as link}
        <a
          href={link.href}
          class="nav-link"
          class:active={$page.url.pathname === link.href}
        >
          {link.label}
        </a>
      {/each}
    </nav>

    <!-- Search Bar -->
    <form class="search-bar" onsubmit={handleSearch} role="search">
      <svg class="search-icon" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
        <circle cx="11" cy="11" r="8" />
        <line x1="21" y1="21" x2="16.65" y2="16.65" />
      </svg>
      <input
        type="search"
        class="search-input"
        placeholder={i18n.t('nav.searchPlaceholder')}
        bind:value={searchQuery}
        aria-label={i18n.t('common.search')}
      />
    </form>

    <!-- Right Actions -->
    <div class="header-actions">
      <!-- Language Switcher -->
      <div class="lang-switcher-wrapper">
        <button
          class="action-btn lang-toggle"
          aria-label={i18n.t('nav.language')}
          aria-expanded={langMenuOpen}
          onclick={(e) => { e.stopPropagation(); toggleLangMenu(); }}
        >
          <span class="lang-current">{i18n.locales.find(l => l.code === i18n.locale)?.flag} {i18n.locale.toUpperCase()}</span>
        </button>
        {#if langMenuOpen}
          <!-- svelte-ignore a11y_no_static_element_interactions a11y_interactive_supports_focus -->
          <div class="lang-dropdown" onclick={(e) => e.stopPropagation()} onkeydown={() => {}} role="menu" tabindex="-1">
            {#each i18n.locales as loc}
              <button
                class="lang-option"
                class:active={i18n.locale === loc.code}
                role="menuitem"
                onclick={() => { i18n.setLocale(loc.code); langMenuOpen = false; }}
              >
                <span class="lang-flag">{loc.flag}</span>
                <span class="lang-label">{loc.label}</span>
              </button>
            {/each}
          </div>
        {/if}
      </div>

      <!-- Basket -->
      <button class="action-btn" aria-label="{i18n.t('nav.basket')} ({basketStore.itemCount})" onclick={onbasketclick}>
        <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
          <path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z" />
          <line x1="3" y1="6" x2="21" y2="6" />
          <path d="M16 10a4 4 0 01-8 0" />
        </svg>
        {#if basketStore.itemCount > 0}
          <span class="basket-badge">{basketStore.itemCount}</span>
        {/if}
      </button>

      <!-- User Menu -->
      <div class="user-menu-wrapper">
        <button
          class="action-btn"
          aria-label={i18n.t('nav.account')}
          aria-expanded={userMenuOpen}
          onclick={(e) => { e.stopPropagation(); toggleUserMenu(); }}
        >
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
            <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" />
            <circle cx="12" cy="7" r="4" />
          </svg>
        </button>
        {#if userMenuOpen}
          <!-- svelte-ignore a11y_no_static_element_interactions a11y_interactive_supports_focus -->
          <div class="user-dropdown" onclick={(e) => e.stopPropagation()} onkeydown={() => {}} role="menu" tabindex="-1">
            {#if authStore.isAuthenticated}
              <span class="dropdown-greeting">{i18n.t('nav.greeting', { name: authStore.currentUser?.firstName ?? 'User' })}</span>
              <hr class="dropdown-divider" />
              {#if authStore.isAdmin}
                <a href="/admin" class="dropdown-item dropdown-item--admin" role="menuitem">🛡️ {i18n.t('nav.admin')}</a>
                <hr class="dropdown-divider" />
              {/if}
              <a href="/account" class="dropdown-item" role="menuitem">{i18n.t('nav.profile')}</a>
              <a href="/account/orders" class="dropdown-item" role="menuitem">{i18n.t('nav.myOrders')}</a>
              <hr class="dropdown-divider" />
              <button class="dropdown-item" role="menuitem" onclick={logout}>{i18n.t('nav.logout')}</button>
            {:else}
              <a href="/auth/login" class="dropdown-item" role="menuitem">{i18n.t('nav.login')}</a>
              <a href="/auth/register" class="dropdown-item" role="menuitem">{i18n.t('nav.createAccount')}</a>
            {/if}
          </div>
        {/if}
      </div>

      <!-- Mobile Hamburger -->
      <button
        class="hamburger"
        aria-label={mobileMenuOpen ? i18n.t('nav.closeMenu') : i18n.t('nav.openMenu')}
        aria-expanded={mobileMenuOpen}
        onclick={(e) => { e.stopPropagation(); toggleMobileMenu(); }}
      >
        <span class="hamburger-line" class:open={mobileMenuOpen}></span>
        <span class="hamburger-line" class:open={mobileMenuOpen}></span>
        <span class="hamburger-line" class:open={mobileMenuOpen}></span>
      </button>
    </div>
  </div>

  <!-- Mobile Menu -->
  {#if mobileMenuOpen}
    <!-- svelte-ignore a11y_no_noninteractive_element_interactions -->
    <div class="mobile-menu" onclick={(e) => e.stopPropagation()} onkeydown={() => {}} role="navigation" aria-label="Mobile navigation">
      <form class="mobile-search" onsubmit={handleSearch} role="search">
        <input
          type="search"
          class="input"
          placeholder={i18n.t('nav.searchPlaceholder')}
          bind:value={searchQuery}
          aria-label={i18n.t('common.search')}
        />
      </form>
      <nav class="mobile-nav">
        {#each navLinks as link}
          <a
            href={link.href}
            class="mobile-nav-link"
            class:active={$page.url.pathname === link.href}
            onclick={closeMenus}
          >
            {link.label}
          </a>
        {/each}
      </nav>
      <!-- Mobile Language Switcher -->
      <div class="mobile-lang-switcher">
        {#each i18n.locales as loc}
          <button
            class="mobile-lang-btn"
            class:active={i18n.locale === loc.code}
            onclick={() => { i18n.setLocale(loc.code); }}
          >
            {loc.flag} {loc.code.toUpperCase()}
          </button>
        {/each}
      </div>
      <hr class="dropdown-divider" />
      {#if authStore.isAuthenticated}
        {#if authStore.isAdmin}
          <a href="/admin" class="mobile-nav-link mobile-nav-link--admin" onclick={closeMenus}>🛡️ {i18n.t('nav.admin')}</a>
          <hr class="dropdown-divider" />
        {/if}
        <a href="/account" class="mobile-nav-link" onclick={closeMenus}>{i18n.t('nav.profile')}</a>
        <a href="/account/orders" class="mobile-nav-link" onclick={closeMenus}>{i18n.t('nav.myOrders')}</a>
        <button class="mobile-nav-link" onclick={logout}>{i18n.t('nav.logout')}</button>
      {:else}
        <a href="/auth/login" class="mobile-nav-link" onclick={closeMenus}>{i18n.t('nav.login')}</a>
        <a href="/auth/register" class="mobile-nav-link" onclick={closeMenus}>{i18n.t('nav.createAccount')}</a>
      {/if}
    </div>
  {/if}
</header>

<style>
  .header {
    position: sticky;
    top: 0;
    z-index: var(--z-sticky);
    background-color: rgba(253, 251, 249, 0.95);
    backdrop-filter: blur(10px);
    -webkit-backdrop-filter: blur(10px);
    border-bottom: 1px solid transparent;
    transition: border-color var(--transition-base), box-shadow var(--transition-base);
  }

  .header.scrolled {
    border-bottom-color: var(--color-border-light);
    box-shadow: var(--shadow-sm);
  }

  .header-inner {
    display: flex;
    align-items: center;
    gap: var(--space-6);
    height: 72px;
  }

  /* Brand */
  .brand {
    flex-shrink: 0;
    text-decoration: none;
  }

  .brand-name {
    font-family: var(--font-display);
    font-size: 1.375rem;
    font-weight: 700;
    color: var(--color-primary);
    letter-spacing: -0.01em;
  }

  /* Desktop Nav */
  .nav-desktop {
    display: none;
    align-items: center;
    gap: var(--space-1);
  }

  @media (min-width: 768px) {
    .nav-desktop {
      display: flex;
    }
  }

  .nav-link {
    padding: var(--space-2) var(--space-3);
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text-light);
    border-radius: var(--radius-md);
    transition: color var(--transition-fast), background-color var(--transition-fast);
    text-decoration: none;
    letter-spacing: 0.01em;
  }

  .nav-link:hover {
    color: var(--color-primary);
    background-color: var(--color-surface-hover);
  }

  .nav-link.active {
    color: var(--color-primary);
    font-weight: 600;
  }

  /* Search */
  .search-bar {
    display: none;
    flex: 1;
    max-width: 360px;
    position: relative;
  }

  @media (min-width: 768px) {
    .search-bar {
      display: flex;
    }
  }

  .search-icon {
    position: absolute;
    left: var(--space-3);
    top: 50%;
    transform: translateY(-50%);
    color: var(--color-text-muted);
    pointer-events: none;
  }

  .search-input {
    width: 100%;
    padding: var(--space-2) var(--space-4) var(--space-2) var(--space-10);
    font-size: 0.875rem;
    background-color: var(--color-surface-hover);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-full);
    transition: border-color var(--transition-fast), background-color var(--transition-fast), box-shadow var(--transition-fast);
  }

  .search-input::placeholder {
    color: var(--color-text-muted);
  }

  .search-input:focus {
    outline: none;
    border-color: var(--color-primary-light);
    background-color: var(--color-surface);
    box-shadow: 0 0 0 3px rgba(139, 94, 107, 0.08);
  }

  /* Actions */
  .header-actions {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    margin-left: auto;
  }

  .action-btn {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    border-radius: var(--radius-full);
    color: var(--color-text);
    transition: background-color var(--transition-fast), color var(--transition-fast);
    text-decoration: none;
  }

  .action-btn:hover {
    background-color: var(--color-surface-hover);
    color: var(--color-primary);
  }

  .basket-badge {
    position: absolute;
    top: 2px;
    right: 2px;
    min-width: 18px;
    height: 18px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.625rem;
    font-weight: 700;
    color: #FFFFFF;
    background-color: var(--color-primary);
    border-radius: var(--radius-full);
    padding: 0 4px;
    line-height: 1;
  }

  /* User Dropdown */
  .user-menu-wrapper {
    position: relative;
  }

  .user-dropdown {
    position: absolute;
    top: calc(100% + var(--space-2));
    right: 0;
    min-width: 200px;
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-lg);
    padding: var(--space-2);
    animation: slideDown var(--transition-fast) ease both;
    z-index: var(--z-dropdown);
  }

  .dropdown-greeting {
    display: block;
    padding: var(--space-2) var(--space-3);
    font-size: 0.8125rem;
    color: var(--color-text-muted);
    font-weight: 500;
  }

  .dropdown-divider {
    border: none;
    border-top: 1px solid var(--color-border-light);
    margin: var(--space-1) 0;
  }

  .dropdown-item {
    display: block;
    width: 100%;
    padding: var(--space-2) var(--space-3);
    font-size: 0.875rem;
    color: var(--color-text);
    border-radius: var(--radius-md);
    transition: background-color var(--transition-fast);
    text-align: left;
    text-decoration: none;
    cursor: pointer;
    background: none;
    border: none;
    font-family: inherit;
  }

  .dropdown-item:hover {
    background-color: var(--color-surface-hover);
    color: var(--color-primary);
  }

  .dropdown-item--admin {
    color: var(--color-primary);
    font-weight: 600;
  }

  .mobile-nav-link--admin {
    color: var(--color-primary);
    font-weight: 600;
  }

  /* Hamburger */
  .hamburger {
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 5px;
    width: 40px;
    height: 40px;
    padding: 8px;
    border-radius: var(--radius-md);
    cursor: pointer;
    background: none;
    border: none;
  }

  @media (min-width: 768px) {
    .hamburger {
      display: none;
    }
  }

  .hamburger:hover {
    background-color: var(--color-surface-hover);
  }

  .hamburger-line {
    display: block;
    width: 100%;
    height: 2px;
    background-color: var(--color-text);
    border-radius: 1px;
    transition: all var(--transition-fast);
    transform-origin: center;
  }

  .hamburger-line.open:nth-child(1) {
    transform: translateY(7px) rotate(45deg);
  }

  .hamburger-line.open:nth-child(2) {
    opacity: 0;
  }

  .hamburger-line.open:nth-child(3) {
    transform: translateY(-7px) rotate(-45deg);
  }

  /* Mobile Menu */
  .mobile-menu {
    display: flex;
    flex-direction: column;
    padding: var(--space-4);
    border-top: 1px solid var(--color-border-light);
    background-color: var(--color-surface);
    animation: slideDown var(--transition-fast) ease both;
  }

  @media (min-width: 768px) {
    .mobile-menu {
      display: none;
    }
  }

  .mobile-search {
    margin-bottom: var(--space-4);
  }

  .mobile-nav {
    display: flex;
    flex-direction: column;
  }

  .mobile-nav-link {
    display: block;
    padding: var(--space-3) var(--space-4);
    font-size: 0.9375rem;
    font-weight: 500;
    color: var(--color-text);
    border-radius: var(--radius-md);
    transition: background-color var(--transition-fast), color var(--transition-fast);
    text-decoration: none;
  }

  .mobile-nav-link:hover,
  .mobile-nav-link.active {
    background-color: var(--color-surface-hover);
    color: var(--color-primary);
  }

  @keyframes slideDown {
    from {
      opacity: 0;
      transform: translateY(-8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  /* Language Switcher */
  .lang-switcher-wrapper {
    position: relative;
  }

  .lang-toggle {
    width: auto;
    padding: var(--space-1) var(--space-2);
    gap: var(--space-1);
  }

  .lang-current {
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text);
    white-space: nowrap;
  }

  .lang-dropdown {
    position: absolute;
    top: calc(100% + var(--space-2));
    right: 0;
    min-width: 160px;
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-lg);
    padding: var(--space-2);
    animation: slideDown var(--transition-fast) ease both;
    z-index: var(--z-dropdown);
  }

  .lang-option {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    width: 100%;
    padding: var(--space-2) var(--space-3);
    font-size: 0.875rem;
    color: var(--color-text);
    border-radius: var(--radius-md);
    transition: background-color var(--transition-fast);
    text-align: left;
    cursor: pointer;
    background: none;
    border: none;
    font-family: inherit;
  }

  .lang-option:hover {
    background-color: var(--color-surface-hover);
  }

  .lang-option.active {
    color: var(--color-primary);
    font-weight: 600;
    background-color: var(--color-surface-hover);
  }

  .lang-flag {
    font-size: 1.125rem;
    line-height: 1;
  }

  .lang-label {
    flex: 1;
  }

  /* Mobile Language Switcher */
  .mobile-lang-switcher {
    display: flex;
    gap: var(--space-2);
    padding: var(--space-3) var(--space-4);
  }

  .mobile-lang-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-1);
    padding: var(--space-2) var(--space-3);
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
    cursor: pointer;
    transition: all var(--transition-fast);
    font-family: inherit;
  }

  .mobile-lang-btn:hover {
    background-color: var(--color-surface-hover);
  }

  .mobile-lang-btn.active {
    color: var(--color-primary);
    border-color: var(--color-primary);
    background-color: var(--color-surface-hover);
    font-weight: 600;
  }
</style>
