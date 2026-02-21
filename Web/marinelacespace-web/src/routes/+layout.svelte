<script lang="ts">
  import type { Snippet } from 'svelte';
  import '../app.css';
  import Header from '$lib/components/Header.svelte';
  import Footer from '$lib/components/Footer.svelte';
  import Toast from '$lib/components/Toast.svelte';

  let { children }: { children: Snippet } = $props();
</script>

<div class="app-layout">
  <Header />

  <Toast />

  <main class="app-main">
    {@render children()}
  </main>

  <Footer />
</div>

<style>
  .app-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }

  .app-main {
    flex: 1;
  }
</style>
