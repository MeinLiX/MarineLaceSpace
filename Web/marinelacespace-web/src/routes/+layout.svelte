<script lang="ts">
  import type { Snippet } from 'svelte';
  import { onNavigate } from '$app/navigation';
  import { page } from '$app/stores';
  import '../app.css';
  import Header from '$lib/components/Header.svelte';
  import Footer from '$lib/components/Footer.svelte';
  import Toast from '$lib/components/Toast.svelte';
  import BasketModal from '$lib/components/BasketModal.svelte';

  let { children }: { children: Snippet } = $props();
  let basketModalOpen = $state(false);
  let isAdminPage = $derived($page.url.pathname.startsWith('/admin'));

  function openBasketModal() {
    basketModalOpen = true;
  }
  function closeBasketModal() {
    basketModalOpen = false;
  }

  onNavigate((navigation) => {
    if (!document.startViewTransition) return;
    return new Promise((resolve) => {
      document.startViewTransition!(async () => {
        resolve();
        await navigation.complete;
      });
    });
  });
</script>

<div class="app-layout">
  {#if !isAdminPage}
    <Header onbasketclick={openBasketModal} />
  {/if}

  <Toast />

  <main class="app-main">
    {@render children()}
  </main>

  {#if !isAdminPage}
    <Footer />
  {/if}
</div>
<BasketModal open={basketModalOpen} onclose={closeBasketModal} />

<style>
  .app-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }

  .app-main {
    flex: 1;
  }

  @keyframes fade-in {
    from { opacity: 0; }
  }

  @keyframes fade-out {
    to { opacity: 0; }
  }

  @keyframes slide-from-right {
    from { transform: translateX(24px); }
  }

  @keyframes slide-to-left {
    to { transform: translateX(-24px); }
  }

  :root::view-transition-old(root) {
    animation: 200ms cubic-bezier(0.4, 0, 1, 1) both fade-out,
               300ms cubic-bezier(0.4, 0, 0.2, 1) both slide-to-left;
  }

  :root::view-transition-new(root) {
    animation: 300ms cubic-bezier(0, 0, 0.2, 1) 90ms both fade-in,
               300ms cubic-bezier(0.4, 0, 0.2, 1) both slide-from-right;
  }
</style>
