<script lang="ts">
  import { getProducts, getCategoryTree } from '$api/catalog';
  import type { Product } from '$types';
  import ProductCard from '$components/ProductCard.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';

  // ─── State ───────────────────────────────────────────────────────────────────

  let products = $state<Product[]>([]);
  let productsLoading = $state(true);
  let productsError = $state<string | null>(null);

  let newsletterEmail = $state('');
  let newsletterSubmitted = $state(false);
  let newsletterSubmitting = $state(false);

  // ─── Categories (static with emoji placeholders) ─────────────────────────────

  const categories = [
    { emoji: '👙', name: 'Бюстгальтери', slug: 'biusthaltery' },
    { emoji: '🩲', name: 'Трусики', slug: 'trusyky' },
    { emoji: '💝', name: 'Комплекти', slug: 'komplekty' },
    { emoji: '🌙', name: 'Піжами & Халати', slug: 'pizhamy-khalaty' },
    { emoji: '🦵', name: 'Панчохи & Колготки', slug: 'panchokhy-kolhotky' },
    { emoji: '🧔', name: 'Чоловіча білизна', slug: 'cholovicha-bilyzna' },
  ];

  // ─── Features ────────────────────────────────────────────────────────────────

  const features = [
    {
      icon: '✋',
      title: 'Ручна робота',
      description: 'Кожен виріб створений з любов\u2019ю та увагою до деталей',
    },
    {
      icon: '🌿',
      title: 'Натуральні тканини',
      description: 'Використовуємо лише найякісніші матеріали',
    },
    {
      icon: '🎨',
      title: 'Індивідуальний підхід',
      description: 'Можливість персоналізації кожного замовлення',
    },
    {
      icon: '🔒',
      title: 'Безпечна оплата',
      description: 'Захищені платежі та гарантія повернення',
    },
  ];

  // ─── Product mapping for ProductCard ─────────────────────────────────────────

  function mapProduct(p: Product) {
    return {
      id: p.id,
      title: p.title,
      shopName: p.shopName,
      imageUrl: p.mainImageUrl ?? undefined,
      imageAlt: p.title,
      basePrice: p.minPrice,
      minPrice: p.minPrice,
      maxPrice: p.maxPrice,
      rating: p.averageRating,
      reviewCount: p.reviewCount,
    };
  }

  // ─── Fetch products on mount ─────────────────────────────────────────────────

  $effect(() => {
    let cancelled = false;

    async function loadProducts() {
      try {
        productsLoading = true;
        productsError = null;
        const response = await getProducts({ page: 1, pageSize: 8 });
        if (!cancelled) {
          products = response.items;
        }
      } catch (err) {
        if (!cancelled) {
          productsError = err instanceof Error ? err.message : 'Не вдалося завантажити товари';
        }
      } finally {
        if (!cancelled) {
          productsLoading = false;
        }
      }
    }

    loadProducts();
    return () => { cancelled = true; };
  });

  // ─── Scroll-reveal IntersectionObserver ──────────────────────────────────────

  let revealSections = $state<HTMLElement[]>([]);

  function addRevealRef(el: HTMLElement) {
    revealSections.push(el);
  }

  $effect(() => {
    if (revealSections.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add('revealed');
            observer.unobserve(entry.target);
          }
        }
      },
      { threshold: 0.1, rootMargin: '0px 0px -40px 0px' }
    );

    for (const el of revealSections) {
      observer.observe(el);
    }

    return () => observer.disconnect();
  });

  // ─── Newsletter ──────────────────────────────────────────────────────────────

  function handleNewsletterSubmit(e: Event) {
    e.preventDefault();
    if (!newsletterEmail || newsletterSubmitting) return;
    newsletterSubmitting = true;
    // Simulated submit
    setTimeout(() => {
      newsletterSubmitted = true;
      newsletterSubmitting = false;
    }, 800);
  }
</script>

<svelte:head>
  <title>MarineLaceSpace — Елегантність у кожній деталі</title>
  <meta name="description" content="Відкрийте світ вишуканої білизни від найкращих українських майстрів. Ручна робота, натуральні тканини, індивідуальний підхід." />
</svelte:head>

<!-- Skip to content -->
<a href="#main-content" class="skip-link">Перейти до основного вмісту</a>

<main id="main-content">
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- HERO SECTION                                                          -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="hero" aria-label="Головна секція">
    <div class="hero-pattern" aria-hidden="true"></div>
    <div class="hero-overlay" aria-hidden="true"></div>
    <div class="hero-content container">
      <h1 class="hero-title">Елегантність у кожній деталі</h1>
      <p class="hero-subtitle">
        Відкрийте світ вишуканої білизни від найкращих українських майстрів
      </p>
      <div class="hero-actions">
        <a href="/catalog" class="btn btn-primary btn-lg">Переглянути каталог</a>
        <a href="/shops" class="btn btn-outline btn-lg hero-btn-outline">Наші магазини</a>
      </div>
      <div class="hero-scroll-hint" aria-hidden="true">
        <svg viewBox="0 0 24 24" width="28" height="28" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round">
          <path d="M12 5v14M5 12l7 7 7-7"/>
        </svg>
      </div>
    </div>
  </section>

  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- CATEGORIES SECTION                                                    -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="categories reveal-section" use:addRevealRef aria-labelledby="categories-heading">
    <div class="container">
      <div class="section-header">
        <h2 id="categories-heading" class="section-title">Категорії</h2>
        <p class="section-subtitle">Знайдіть ідеальну білизну для кожного випадку</p>
      </div>
      <div class="categories-grid">
        {#each categories as cat (cat.slug)}
          <a href="/catalog?category={cat.slug}" class="category-card" aria-label="Категорія: {cat.name}">
            <span class="category-icon" aria-hidden="true">{cat.emoji}</span>
            <h3 class="category-name">{cat.name}</h3>
            <span class="category-link">Переглянути →</span>
          </a>
        {/each}
      </div>
    </div>
  </section>

  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- FEATURED PRODUCTS SECTION                                             -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="products reveal-section" use:addRevealRef aria-labelledby="products-heading">
    <div class="container">
      <div class="section-header">
        <h2 id="products-heading" class="section-title">Популярні товари</h2>
        <p class="section-subtitle">Найбільш бажані вироби від наших майстрів</p>
      </div>

      {#if productsLoading}
        <div class="products-loading">
          <LoadingSpinner size="lg" message="Завантаження товарів..." />
        </div>
      {:else if productsError}
        <div class="products-error" role="alert">
          <p class="error-icon" aria-hidden="true">⚠️</p>
          <p class="error-message">{productsError}</p>
          <button class="btn btn-outline" onclick={() => location.reload()}>
            Спробувати знову
          </button>
        </div>
      {:else if products.length > 0}
        <div class="products-grid">
          {#each products as product (product.id)}
            <ProductCard product={mapProduct(product)} />
          {/each}
        </div>
        <div class="products-footer">
          <a href="/catalog" class="btn btn-secondary btn-lg">Дивитись все</a>
        </div>
      {:else}
        <div class="products-empty">
          <p>Наразі немає доступних товарів. Поверніться пізніше!</p>
        </div>
      {/if}
    </div>
  </section>

  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- WHY CHOOSE US SECTION                                                 -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="features reveal-section" use:addRevealRef aria-labelledby="features-heading">
    <div class="container">
      <div class="section-header">
        <h2 id="features-heading" class="section-title">Чому обирають нас</h2>
        <p class="section-subtitle">Ми створюємо не просто білизну — ми створюємо впевненість</p>
      </div>
      <div class="features-grid">
        {#each features as feature (feature.title)}
          <article class="feature-card">
            <span class="feature-icon" aria-hidden="true">{feature.icon}</span>
            <h3 class="feature-title">{feature.title}</h3>
            <p class="feature-description">{feature.description}</p>
          </article>
        {/each}
      </div>
    </div>
  </section>

  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- NEWSLETTER CTA SECTION                                                -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="newsletter reveal-section" use:addRevealRef aria-labelledby="newsletter-heading">
    <div class="newsletter-bg" aria-hidden="true"></div>
    <div class="container newsletter-content">
      <h2 id="newsletter-heading" class="newsletter-title">Підпишіться на новини</h2>
      <p class="newsletter-subtitle">
        Отримуйте першими інформацію про нові колекції, акції та ексклюзивні пропозиції
      </p>

      {#if newsletterSubmitted}
        <div class="newsletter-success" role="status">
          <span aria-hidden="true">✉️</span>
          <p>Дякуємо! Ви успішно підписались на наші новини.</p>
        </div>
      {:else}
        <form class="newsletter-form" onsubmit={handleNewsletterSubmit} aria-label="Підписка на новини">
          <div class="newsletter-input-group">
            <label for="newsletter-email" class="sr-only">Електронна пошта</label>
            <input
              id="newsletter-email"
              type="email"
              placeholder="Ваш email"
              required
              autocomplete="email"
              bind:value={newsletterEmail}
              disabled={newsletterSubmitting}
            />
            <button type="submit" class="btn btn-primary" disabled={newsletterSubmitting}>
              {#if newsletterSubmitting}
                Відправка...
              {:else}
                Підписатись
              {/if}
            </button>
          </div>
        </form>
        <p class="newsletter-privacy">
          Натискаючи «Підписатись», ви погоджуєтесь з нашою
          <a href="/privacy">політикою конфіденційності</a>. Відписатись можна у будь-який момент.
        </p>
      {/if}
    </div>
  </section>
</main>

<style>
  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* SKIP LINK                                                                 */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  :global(.skip-link) {
    position: absolute;
    top: -100%;
    left: var(--space-4);
    z-index: 9999;
    padding: var(--space-3) var(--space-6);
    background: var(--color-primary);
    color: #fff;
    border-radius: 0 0 8px 8px;
    font-family: var(--font-body);
    font-size: 0.875rem;
    text-decoration: none;
    transition: top 0.2s;
  }

  :global(.skip-link:focus) {
    top: 0;
  }

  /* Screen reader only utility (scoped) */
  .sr-only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* REVEAL ON SCROLL                                                          */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .reveal-section {
    opacity: 0;
    transform: translateY(32px);
    transition: opacity 0.7s cubic-bezier(0.22, 1, 0.36, 1),
                transform 0.7s cubic-bezier(0.22, 1, 0.36, 1);
  }

  :global(.reveal-section.revealed) {
    opacity: 1;
    transform: translateY(0);
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* SECTION COMMON                                                            */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .section-header {
    text-align: center;
    margin-bottom: var(--space-12);
  }

  .section-title {
    font-family: var(--font-display);
    font-size: clamp(1.75rem, 4vw, 2.5rem);
    font-weight: 700;
    color: var(--color-text);
    margin-bottom: var(--space-3);
    letter-spacing: -0.01em;
  }

  .section-subtitle {
    font-size: clamp(0.95rem, 1.8vw, 1.125rem);
    color: var(--color-text-light);
    max-width: 520px;
    margin-inline: auto;
    line-height: 1.6;
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* HERO                                                                      */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .hero {
    position: relative;
    min-height: 100svh;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    background: linear-gradient(
      135deg,
      #8B5E6B 0%,
      #A87586 25%,
      #D4A574 50%,
      #C49068 75%,
      #8B5E6B 100%
    );
    background-size: 400% 400%;
    animation: heroGradientShift 12s ease infinite;
  }

  @keyframes heroGradientShift {
    0%,100% { background-position: 0% 50%; }
    50% { background-position: 100% 50%; }
  }

  /* Lace-like pattern overlay */
  .hero-pattern {
    position: absolute;
    inset: 0;
    opacity: 0.07;
    background:
      repeating-linear-gradient(
        45deg,
        transparent,
        transparent 8px,
        rgba(255,255,255,0.5) 8px,
        rgba(255,255,255,0.5) 9px
      ),
      repeating-linear-gradient(
        -45deg,
        transparent,
        transparent 8px,
        rgba(255,255,255,0.5) 8px,
        rgba(255,255,255,0.5) 9px
      ),
      repeating-radial-gradient(
        circle at 50% 50%,
        transparent 0,
        transparent 12px,
        rgba(255,255,255,0.3) 12px,
        rgba(255,255,255,0.3) 13px
      );
    pointer-events: none;
  }

  .hero-overlay {
    position: absolute;
    inset: 0;
    background: radial-gradient(
      ellipse 80% 60% at 50% 40%,
      rgba(0,0,0,0) 0%,
      rgba(0,0,0,0.15) 100%
    );
    pointer-events: none;
  }

  .hero-content {
    position: relative;
    z-index: 1;
    text-align: center;
    padding: var(--space-16) var(--space-4);
    max-width: 720px;
    animation: heroFadeIn 1s ease-out;
  }

  @keyframes heroFadeIn {
    from { opacity: 0; transform: translateY(24px); }
    to { opacity: 1; transform: translateY(0); }
  }

  .hero-title {
    font-family: var(--font-display);
    font-size: clamp(2.2rem, 6vw, 4rem);
    font-weight: 700;
    color: #FFFFFF;
    line-height: 1.15;
    margin-bottom: var(--space-6);
    text-shadow: 0 2px 20px rgba(0,0,0,0.15);
    letter-spacing: -0.02em;
  }

  .hero-subtitle {
    font-family: var(--font-body);
    font-size: clamp(1rem, 2.2vw, 1.25rem);
    color: rgba(255,255,255,0.9);
    line-height: 1.7;
    margin-bottom: var(--space-10);
    max-width: 520px;
    margin-inline: auto;
  }

  .hero-actions {
    display: flex;
    gap: var(--space-4);
    justify-content: center;
    flex-wrap: wrap;
  }

  .hero-btn-outline {
    border-color: rgba(255,255,255,0.6) !important;
    color: #fff !important;
    background: rgba(255,255,255,0.08) !important;
    backdrop-filter: blur(4px);
  }

  .hero-btn-outline:hover {
    background: rgba(255,255,255,0.18) !important;
    border-color: #fff !important;
  }

  .hero-scroll-hint {
    margin-top: var(--space-14);
    color: rgba(255,255,255,0.5);
    animation: bounceDown 2s ease-in-out infinite;
  }

  @keyframes bounceDown {
    0%,100% { transform: translateY(0); }
    50% { transform: translateY(8px); }
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* CATEGORIES                                                                */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .categories {
    padding: var(--space-16) 0;
    background: var(--color-background);
  }

  .categories-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
    gap: var(--space-6);
  }

  .category-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    text-decoration: none;
    color: var(--color-text);
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: 16px;
    padding: var(--space-8) var(--space-4);
    transition: transform 0.3s cubic-bezier(0.22, 1, 0.36, 1),
                box-shadow 0.3s cubic-bezier(0.22, 1, 0.36, 1);
    cursor: pointer;
  }

  .category-card:hover {
    transform: translateY(-6px);
    box-shadow:
      0 12px 32px rgba(139, 94, 107, 0.12),
      0 4px 12px rgba(0,0,0,0.06);
  }

  .category-icon {
    font-size: 2.5rem;
    margin-bottom: var(--space-4);
    display: block;
    transition: transform 0.3s ease;
  }

  .category-card:hover .category-icon {
    transform: scale(1.15);
  }

  .category-name {
    font-family: var(--font-body);
    font-size: 1rem;
    font-weight: 600;
    margin-bottom: var(--space-2);
    color: var(--color-text);
  }

  .category-link {
    font-size: 0.8125rem;
    color: var(--color-primary);
    font-weight: 500;
    opacity: 0;
    transform: translateY(4px);
    transition: opacity 0.25s, transform 0.25s;
  }

  .category-card:hover .category-link {
    opacity: 1;
    transform: translateY(0);
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* PRODUCTS                                                                  */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .products {
    padding: var(--space-16) 0;
    background: var(--color-surface);
  }

  .products-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    gap: var(--space-6);
  }

  .products-loading,
  .products-error,
  .products-empty {
    text-align: center;
    padding: var(--space-12) 0;
  }

  .products-error {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-4);
  }

  .error-icon {
    font-size: 2rem;
  }

  .error-message {
    color: var(--color-error);
    font-size: 1rem;
  }

  .products-empty {
    color: var(--color-text-muted);
    font-size: 1.05rem;
  }

  .products-footer {
    text-align: center;
    margin-top: var(--space-12);
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* FEATURES                                                                  */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .features {
    padding: var(--space-16) 0;
    background: var(--color-background);
  }

  .features-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: var(--space-6);
  }

  .feature-card {
    text-align: center;
    padding: var(--space-8) var(--space-6);
    border-radius: 16px;
    background: var(--color-surface);
    border: 1px solid var(--color-border-light);
    transition: transform 0.3s cubic-bezier(0.22, 1, 0.36, 1),
                box-shadow 0.3s cubic-bezier(0.22, 1, 0.36, 1);
  }

  .feature-card:hover {
    transform: translateY(-4px);
    box-shadow:
      0 8px 24px rgba(139, 94, 107, 0.1),
      0 2px 8px rgba(0,0,0,0.04);
  }

  .feature-icon {
    font-size: 2.5rem;
    display: block;
    margin-bottom: var(--space-4);
  }

  .feature-title {
    font-family: var(--font-display);
    font-size: 1.2rem;
    font-weight: 600;
    color: var(--color-text);
    margin-bottom: var(--space-3);
  }

  .feature-description {
    font-size: 0.925rem;
    color: var(--color-text-light);
    line-height: 1.65;
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* NEWSLETTER                                                                */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .newsletter {
    position: relative;
    padding: var(--space-16) 0;
    overflow: hidden;
  }

  .newsletter-bg {
    position: absolute;
    inset: 0;
    background: linear-gradient(
      135deg,
      var(--color-primary) 0%,
      #A87586 40%,
      var(--color-secondary) 100%
    );
    z-index: 0;
  }

  .newsletter-bg::after {
    content: '';
    position: absolute;
    inset: 0;
    opacity: 0.05;
    background:
      repeating-linear-gradient(
        60deg,
        transparent, transparent 6px,
        rgba(255,255,255,0.5) 6px,
        rgba(255,255,255,0.5) 7px
      ),
      repeating-linear-gradient(
        -60deg,
        transparent, transparent 6px,
        rgba(255,255,255,0.5) 6px,
        rgba(255,255,255,0.5) 7px
      );
  }

  .newsletter-content {
    position: relative;
    z-index: 1;
    text-align: center;
    max-width: 560px;
  }

  .newsletter-title {
    font-family: var(--font-display);
    font-size: clamp(1.6rem, 3.5vw, 2.25rem);
    font-weight: 700;
    color: #fff;
    margin-bottom: var(--space-4);
  }

  .newsletter-subtitle {
    font-size: clamp(0.925rem, 1.6vw, 1.05rem);
    color: rgba(255,255,255,0.85);
    line-height: 1.6;
    margin-bottom: var(--space-8);
  }

  .newsletter-form {
    width: 100%;
  }

  .newsletter-input-group {
    display: flex;
    gap: var(--space-3);
    max-width: 440px;
    margin-inline: auto;
  }

  .newsletter-input-group input {
    flex: 1;
    padding: var(--space-3) var(--space-4);
    border: 2px solid rgba(255,255,255,0.3);
    border-radius: 8px;
    background: rgba(255,255,255,0.12);
    color: #fff;
    font-family: var(--font-body);
    font-size: 0.95rem;
    backdrop-filter: blur(4px);
    transition: border-color 0.2s, background-color 0.2s;
    outline: none;
  }

  .newsletter-input-group input::placeholder {
    color: rgba(255,255,255,0.55);
  }

  .newsletter-input-group input:focus {
    border-color: #fff;
    background: rgba(255,255,255,0.2);
  }

  .newsletter-input-group .btn {
    white-space: nowrap;
    background: #fff;
    color: var(--color-primary);
    font-weight: 600;
    border: none;
  }

  .newsletter-input-group .btn:hover {
    background: rgba(255,255,255,0.9);
  }

  .newsletter-privacy {
    margin-top: var(--space-4);
    font-size: 0.8rem;
    color: rgba(255,255,255,0.55);
    line-height: 1.5;
  }

  .newsletter-privacy a {
    color: rgba(255,255,255,0.8);
    text-decoration: underline;
    text-underline-offset: 2px;
  }

  .newsletter-privacy a:hover {
    color: #fff;
  }

  .newsletter-success {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-3);
    color: #fff;
    font-size: 1.05rem;
    animation: fadeInUp 0.5s ease-out;
  }

  .newsletter-success span {
    font-size: 2.5rem;
  }

  @keyframes fadeInUp {
    from { opacity: 0; transform: translateY(12px); }
    to { opacity: 1; transform: translateY(0); }
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* RESPONSIVE                                                                */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  @media (max-width: 768px) {
    .hero-content {
      padding: var(--space-12) var(--space-4);
    }

    .hero-actions {
      flex-direction: column;
      align-items: center;
    }

    .hero-actions a {
      width: 100%;
      max-width: 280px;
      text-align: center;
    }

    .categories-grid {
      grid-template-columns: repeat(2, 1fr);
      gap: var(--space-4);
    }

    .category-card {
      padding: var(--space-6) var(--space-3);
    }

    .category-link {
      opacity: 1;
      transform: translateY(0);
    }

    .products-grid {
      grid-template-columns: repeat(2, 1fr);
      gap: var(--space-4);
    }

    .features-grid {
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
    }

    .newsletter-input-group {
      flex-direction: column;
    }

    .newsletter-input-group .btn {
      width: 100%;
    }
  }

  @media (max-width: 480px) {
    .products-grid {
      grid-template-columns: 1fr;
    }

    .features-grid {
      grid-template-columns: 1fr;
    }

    .categories-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }

  @media (min-width: 1200px) {
    .categories-grid {
      grid-template-columns: repeat(6, 1fr);
    }

    .products-grid {
      grid-template-columns: repeat(4, 1fr);
    }

    .features-grid {
      grid-template-columns: repeat(4, 1fr);
    }
  }
</style>
