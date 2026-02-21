<script lang="ts">
  import { getProducts, getCategoryTree } from '$api/catalog';
  import type { Product } from '$types';
  import ProductCard from '$components/ProductCard.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';
  import { i18n } from '$i18n/index.svelte';

  // ─── State ───────────────────────────────────────────────────────────────────

  let products = $state<Product[]>([]);
  let productsLoading = $state(true);
  let productsError = $state<string | null>(null);

  let newsletterEmail = $state('');
  let newsletterSubmitted = $state(false);
  let newsletterSubmitting = $state(false);

  // ─── Categories (static with emoji placeholders) ─────────────────────────────

  const categories = $derived([
    { emoji: '👙', name: i18n.t('home.catBras'), slug: 'biusthaltery' },
    { emoji: '🩲', name: i18n.t('home.catPanties'), slug: 'trusyky' },
    { emoji: '💝', name: i18n.t('home.catSets'), slug: 'komplekty' },
    { emoji: '🌙', name: i18n.t('home.catSleepwear'), slug: 'pizhamy-khalaty' },
    { emoji: '🦵', name: i18n.t('home.catHosiery'), slug: 'panchokhy-kolhotky' },
    { emoji: '🧔', name: i18n.t('home.catMens'), slug: 'cholovicha-bilyzna' },
  ]);

  // ─── Features ────────────────────────────────────────────────────────────────

  const features = $derived([
    {
      icon: '✋',
      title: i18n.t('home.featureHandmade'),
      description: i18n.t('home.featureHandmadeDesc'),
    },
    {
      icon: '🌿',
      title: i18n.t('home.featureNatural'),
      description: i18n.t('home.featureNaturalDesc'),
    },
    {
      icon: '🎨',
      title: i18n.t('home.featureCustom'),
      description: i18n.t('home.featureCustomDesc'),
    },
    {
      icon: '🔒',
      title: i18n.t('home.featureSecure'),
      description: i18n.t('home.featureSecureDesc'),
    },
  ]);

  // ─── Product mapping for ProductCard ─────────────────────────────────────────

  function mapProduct(p: Product) {
    return {
      id: p.id,
      title: p.name,
      shopName: p.shopName,
      imageUrl: p.mainImageUrl ?? undefined,
      imageAlt: p.name,
      basePrice: p.price,
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
          productsError = err instanceof Error ? err.message : i18n.t('home.productsError');
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

  // ─── Mannequin scroll animation ─────────────────────────────────────────────

  let mannequinSection: HTMLElement | undefined = $state();
  let scrollProgress = $state(0);

  $effect(() => {
    const section = mannequinSection;
    if (!section) return;

    function onScroll() {
      const rect = section!.getBoundingClientRect();
      const sectionHeight = section!.offsetHeight - window.innerHeight;
      const scrolled = -rect.top;
      scrollProgress = Math.max(0, Math.min(1, scrolled / sectionHeight));
    }

    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    return () => window.removeEventListener('scroll', onScroll);
  });

  const mannequinScale = $derived(0.4 + Math.min(scrollProgress / 0.35, 1) * 0.6);
  const mannequinOpacity = $derived(Math.min(scrollProgress / 0.15, 1));
  const lingerieReveal = $derived(Math.max(0, Math.min((scrollProgress - 0.3) / 0.35, 1)));
  const ctaOpacity = $derived(Math.max(0, (scrollProgress - 0.75) / 0.25));
  const pantyReveal = $derived(Math.max(0, Math.min((scrollProgress - 0.3) / 0.2, 1)));
  const braReveal = $derived(Math.max(0, Math.min((scrollProgress - 0.45) / 0.2, 1)));

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

  // ─── Scroll-to-top button ──────────────────────────────────────────────────

  let showScrollTop = $state(false);

  $effect(() => {
    function onScroll() {
      showScrollTop = window.scrollY > 400;
    }

    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    return () => window.removeEventListener('scroll', onScroll);
  });

  function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
</script>

<svelte:head>
  <title>MarineLaceSpace — {i18n.t('home.heroTitle')}</title>
  <meta name="description" content={i18n.t('home.heroSubtitle')} />
</svelte:head>

<!-- Skip to content -->
<a href="#main-content" class="skip-link">{i18n.t('common.skipToContent')}</a>

<main id="main-content">
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- HERO SECTION                                                          -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="hero" aria-label="Головна секція">
    <div class="hero-pattern" aria-hidden="true"></div>
    <div class="hero-overlay" aria-hidden="true"></div>
    <div class="hero-content container">
      <h1 class="hero-title">{i18n.t('home.heroTitle')}</h1>
      <p class="hero-subtitle">
        {i18n.t('home.heroSubtitle')}
      </p>
      <div class="hero-actions">
        <a href="/catalog" class="btn btn-primary btn-lg">{i18n.t('home.heroCatalog')}</a>
        <a href="/shops" class="btn btn-outline btn-lg hero-btn-outline">{i18n.t('home.heroShops')}</a>
      </div>
      <div class="hero-scroll-hint" aria-hidden="true">
        <svg viewBox="0 0 24 24" width="28" height="28" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round">
          <path d="M12 5v14M5 12l7 7 7-7"/>
        </svg>
      </div>
    </div>
  </section>

  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- MANNEQUIN SCROLL SHOWCASE                                             -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="mannequin-scroll" bind:this={mannequinSection} aria-label="Демонстрація колекції">
    <div class="mannequin-sticky">
      <div class="mannequin-canvas">
        
        <svg
          class="mannequin-svg floating-breath"
          viewBox="0 0 200 520"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          style="transform: scale({mannequinScale}); opacity: {mannequinOpacity}; filter: drop-shadow(0px 10px 20px rgba(0,0,0,0.1));"
          aria-hidden="true"
        >
          <defs>
            <!-- Silk fabric gradient -->
            <linearGradient id="silkGradient" x1="0%" y1="0%" x2="100%" y2="100%">
              <stop offset="0%" style="stop-color:#C8788C;stop-opacity:1" />
              <stop offset="30%" style="stop-color:#D4A574;stop-opacity:1" />
              <stop offset="60%" style="stop-color:#C8788C;stop-opacity:1" />
              <stop offset="100%" style="stop-color:#B0687A;stop-opacity:1" />
            </linearGradient>
            <!-- Deeper silk for shadows -->
            <linearGradient id="silkShadow" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" style="stop-color:#A05A6C;stop-opacity:0.5" />
              <stop offset="100%" style="stop-color:#C8788C;stop-opacity:0" />
            </linearGradient>
            <!-- Highlight for fabric sheen -->
            <linearGradient id="silkHighlight" x1="30%" y1="0%" x2="70%" y2="100%">
              <stop offset="0%" style="stop-color:#fff;stop-opacity:0.3" />
              <stop offset="50%" style="stop-color:#fff;stop-opacity:0" />
              <stop offset="100%" style="stop-color:#fff;stop-opacity:0.1" />
            </linearGradient>
            <!-- Skin gradient -->
            <linearGradient id="skinGrad" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" style="stop-color:#F5DCC8;stop-opacity:0.35" />
              <stop offset="100%" style="stop-color:#E8C9B0;stop-opacity:0.25" />
            </linearGradient>
            <!-- Lace mesh pattern -->
            <pattern id="laceMesh" x="0" y="0" width="8" height="8" patternUnits="userSpaceOnUse">
              <circle cx="4" cy="4" r="2.5" fill="none" stroke="#A05A6C" stroke-width="0.4" opacity="0.6"/>
              <circle cx="0" cy="0" r="1.5" fill="none" stroke="#A05A6C" stroke-width="0.3" opacity="0.4"/>
              <circle cx="8" cy="0" r="1.5" fill="none" stroke="#A05A6C" stroke-width="0.3" opacity="0.4"/>
              <circle cx="0" cy="8" r="1.5" fill="none" stroke="#A05A6C" stroke-width="0.3" opacity="0.4"/>
              <circle cx="8" cy="8" r="1.5" fill="none" stroke="#A05A6C" stroke-width="0.3" opacity="0.4"/>
            </pattern>
            <!-- Floral lace overlay -->
            <pattern id="floralLace" x="0" y="0" width="16" height="16" patternUnits="userSpaceOnUse">
              <path d="M8,2 C10,4 12,4 12,6 C12,8 10,10 8,8 C6,10 4,8 4,6 C4,4 6,4 8,2Z" fill="none" stroke="#A05A6C" stroke-width="0.35" opacity="0.5"/>
              <circle cx="8" cy="6" r="0.8" fill="#A05A6C" opacity="0.25"/>
              <path d="M0,10 C2,12 4,12 4,14 C4,16 2,18 0,16" fill="none" stroke="#A05A6C" stroke-width="0.3" opacity="0.3"/>
              <path d="M16,10 C14,12 12,12 12,14 C12,16 14,18 16,16" fill="none" stroke="#A05A6C" stroke-width="0.3" opacity="0.3"/>
            </pattern>
            <!-- Clip path for bra cups -->
            <clipPath id="leftCup">
              <path d="M100,122 C90,122 78,115 74,98 C70,110 74,137 86,142 C96,146 100,136 100,122Z"/>
            </clipPath>
            <clipPath id="rightCup">
              <path d="M100,122 C110,122 122,115 126,98 C130,110 126,137 114,142 C104,146 100,136 100,122Z"/>
            </clipPath>
            <!-- Clip for panty -->
            <clipPath id="pantyClip">
              <path d="M64,244 C64,244 80,266 100,266 C120,266 136,244 136,244 L133,272 C131,288 116,308 100,308 C84,308 69,288 67,272Z"/>
            </clipPath>
          </defs>
  
          <!-- Body silhouette outline -->
          <path
            class="mannequin-body"
            d="M100,18 
               C116,18 126,28 126,44 
               C126,56 120,66 114,72
               C136,77 146,98 140,118 
               C137,130 128,155 118,165 
               C113,170 116,185 124,195 
               C134,210 142,235 142,264 
               C142,300 136,352 131,422 
               L126,500
               M100,18
               C84,18 74,28 74,44 
               C74,56 80,66 86,72
               C64,77 54,98 60,118
               C63,130 72,155 82,165
               C87,170 84,185 76,195
               C66,210 58,235 58,264
               C58,300 64,352 69,422
               L74,500"
            stroke="var(--color-primary)"
            stroke-width="0.8"
            stroke-linecap="round"
            stroke-linejoin="round"
            opacity="0.5"
          />
          
          <!-- Collar detail -->
          <path 
            d="M86,56 Q100,64 114,56" 
            stroke="var(--color-primary)" 
            stroke-width="0.4" 
            opacity="0.25"
          />
  
          <!-- Body fill (skin tone) -->
          <path
            d="M86,72 C64,77 54,98 60,118
               C63,130 72,155 82,165
               C87,170 84,185 76,195
               C66,210 58,235 58,264
               C58,284 60,304 63,316
               L137,316
               C140,304 142,284 142,264
               C142,235 134,210 124,195
               C116,185 113,170 118,165
               C128,155 137,130 140,118
               C146,98 136,77 114,72"
            fill="url(#skinGrad)"
          />
          <!-- Subtle body contour lines -->
          <path d="M95,164 Q100,168 105,164" stroke="var(--color-primary)" stroke-width="0.3" opacity="0.15" fill="none"/>
          <path d="M88,200 Q100,210 112,200" stroke="var(--color-primary)" stroke-width="0.25" opacity="0.12" fill="none"/>
  
          <!-- ─── PANTIES ─── -->
          <g style="opacity: {pantyReveal}; transform: translateY({(1 - pantyReveal) * 25}px)">
            <!-- Shadow behind panty -->
            <path
              d="M69,248 Q100,272 131,248 L128,276 Q126,292 100,310 Q74,292 72,276Z"
              fill="#A05A6C"
              opacity="0.12"
              transform="translate(0, 2)"
            />
            <!-- Main panty shape -->
            <path
              d="M64,244 
                 C64,244 80,266 100,266 
                 C120,266 136,244 136,244
                 L133,272
                 C131,288 116,308 100,308
                 C84,308 69,288 67,272
                 Z"
              fill="url(#silkGradient)"
              filter="drop-shadow(0px 2px 4px rgba(0,0,0,0.15))"
            />
            <!-- Fabric highlight sheen -->
            <path
              d="M64,244 
                 C64,244 80,266 100,266 
                 C120,266 136,244 136,244
                 L133,272
                 C131,288 116,308 100,308
                 C84,308 69,288 67,272
                 Z"
              fill="url(#silkHighlight)"
              clip-path="url(#pantyClip)"
            />
            <!-- Lace overlay on panty -->
            <path
              d="M64,244 
                 C64,244 80,266 100,266 
                 C120,266 136,244 136,244
                 L133,272
                 C131,288 116,308 100,308
                 C84,308 69,288 67,272
                 Z"
              fill="url(#floralLace)"
              clip-path="url(#pantyClip)"
              opacity="0.7"
            />
            <!-- Lace scalloped waistband -->
            <path
              d="M63,244 Q66,240 69,244 T75,244 T81,244 T87,244 T93,244 T99,244 T105,244 T111,244 T117,244 T123,244 T129,244 T135,244 T137,244"
              stroke="#A05A6C"
              stroke-width="0.8"
              fill="none"
              opacity="0.7"
            />
            <!-- Second scallop row -->
            <path
              d="M65,247 Q68,243 71,247 T77,247 T83,247 T89,247 T95,247 T101,247 T107,247 T113,247 T119,247 T125,247 T131,247 T135,247"
              stroke="#A05A6C"
              stroke-width="0.4"
              fill="none"
              opacity="0.4"
            />
            <!-- Center seam -->
            <path d="M100,250 L100,302" stroke="#A05A6C" stroke-width="0.3" opacity="0.2"/>
            <!-- Fabric fold lines -->
            <path d="M85,260 Q90,262 95,258" stroke="#A05A6C" stroke-width="0.3" opacity="0.15" fill="none"/>
            <path d="M105,260 Q110,262 115,258" stroke="#A05A6C" stroke-width="0.3" opacity="0.15" fill="none"/>
            <!-- Lace trim at leg openings -->
            <path d="M67,272 Q72,280 80,288 Q85,293 90,296" stroke="#A05A6C" stroke-width="0.5" fill="none" opacity="0.5" stroke-dasharray="2 2"/>
            <path d="M133,272 Q128,280 120,288 Q115,293 110,296" stroke="#A05A6C" stroke-width="0.5" fill="none" opacity="0.5" stroke-dasharray="2 2"/>
            <!-- Side ties / straps -->
            <path d="M64,244 Q57,240 59,233" stroke="#C8788C" stroke-width="1.2" stroke-linecap="round"/>
            <path d="M136,244 Q143,240 141,233" stroke="#C8788C" stroke-width="1.2" stroke-linecap="round"/>
            <!-- Small bow at center waist -->
            <path d="M97,244 Q100,240 103,244" stroke="#D4A574" stroke-width="0.8" fill="none"/>
            <circle cx="100" cy="243" r="1" fill="#D4A574" opacity="0.8"/>
          </g>
  
          <!-- ─── BRA ─── -->
          <g style="opacity: {braReveal}; transform: translateY({(1 - braReveal) * 25}px)">
            <!-- Shadow under cups -->
            <ellipse cx="86" cy="138" rx="16" ry="5" fill="#A05A6C" opacity="0.08"/>
            <ellipse cx="114" cy="138" rx="16" ry="5" fill="#A05A6C" opacity="0.08"/>
            
            <!-- Left cup -->
            <path
              d="M100,122 
                 C90,122 78,115 74,98
                 C70,110 74,137 86,142
                 C96,146 100,136 100,122Z"
              fill="url(#silkGradient)"
            />
            <!-- Left cup highlight -->
            <path
              d="M100,122 
                 C90,122 78,115 74,98
                 C70,110 74,137 86,142
                 C96,146 100,136 100,122Z"
              fill="url(#silkHighlight)"
              clip-path="url(#leftCup)"
            />
            <!-- Left cup lace overlay -->
            <path
              d="M100,122 
                 C90,122 78,115 74,98
                 C70,110 74,137 86,142
                 C96,146 100,136 100,122Z"
              fill="url(#laceMesh)"
              clip-path="url(#leftCup)"
              opacity="0.6"
            />
            <!-- Floral lace on left cup upper -->
            <path
              d="M74,98 C78,96 82,98 86,96 C90,94 94,96 98,98"
              fill="url(#floralLace)"
              clip-path="url(#leftCup)"
              opacity="0.5"
            />
  
            <!-- Right cup -->
            <path
              d="M100,122 
                 C110,122 122,115 126,98
                 C130,110 126,137 114,142
                 C104,146 100,136 100,122Z"
              fill="url(#silkGradient)"
            />
            <!-- Right cup highlight -->
            <path
              d="M100,122 
                 C110,122 122,115 126,98
                 C130,110 126,137 114,142
                 C104,146 100,136 100,122Z"
              fill="url(#silkHighlight)"
              clip-path="url(#rightCup)"
            />
            <!-- Right cup lace overlay -->
            <path
              d="M100,122 
                 C110,122 122,115 126,98
                 C130,110 126,137 114,142
                 C104,146 100,136 100,122Z"
              fill="url(#laceMesh)"
              clip-path="url(#rightCup)"
              opacity="0.6"
            />
  
            <!-- Band / underwire line -->
            <path
              d="M74,125 C74,140 84,146 98,141 M102,141 C116,146 126,140 126,125"
              stroke="#A05A6C"
              stroke-width="0.7"
              stroke-linecap="round"
              fill="none"
              opacity="0.4"
            />
            
            <!-- Scalloped lace trim on cup edges -->
            <path d="M74,98 Q77,95 80,98 T86,98 T92,98 T98,100" stroke="#A05A6C" stroke-width="0.5" fill="none" opacity="0.5"/>
            <path d="M126,98 Q123,95 120,98 T114,98 T108,98 T102,100" stroke="#A05A6C" stroke-width="0.5" fill="none" opacity="0.5"/>
            
            <!-- Straps -->
            <path
              d="M80,103 L81,64"
              stroke="#C8788C"
              stroke-width="1"
              stroke-linecap="round"
            />
            <path
              d="M120,103 L119,64"
              stroke="#C8788C"
              stroke-width="1"
              stroke-linecap="round"
            />
            <!-- Strap adjuster detail -->
            <rect x="79.5" y="78" width="3" height="5" rx="0.5" fill="none" stroke="#D4A574" stroke-width="0.5" opacity="0.6"/>
            <rect x="117.5" y="78" width="3" height="5" rx="0.5" fill="none" stroke="#D4A574" stroke-width="0.5" opacity="0.6"/>
            
            <!-- Center gore / golden charm -->
            <circle cx="100" cy="124" r="2.5" fill="none" stroke="#D4AF37" stroke-width="0.6" opacity="0.8"/>
            <circle cx="100" cy="124" r="1.2" fill="#D4AF37" opacity="0.6"/>
            <!-- Small bow at center -->
            <path d="M97,120 Q100,116 103,120" stroke="#D4A574" stroke-width="0.6" fill="none" opacity="0.7"/>
          </g>
        </svg>
  
        <div class="mannequin-detail mannequin-detail-left" style="opacity: {braReveal}; transform: translateX({(1 - braReveal) * -30}px)">
          <span class="detail-line"></span>
          <span class="detail-label">{i18n.t('home.mannequinLace')}</span>
        </div>
        <div class="mannequin-detail mannequin-detail-right" style="opacity: {pantyReveal}; transform: translateX({(1 - pantyReveal) * 30}px)">
          <span class="detail-label">{i18n.t('home.mannequinSilk')}</span>
          <span class="detail-line"></span>
        </div>
      </div>
  
      <div class="mannequin-cta" style="opacity: {ctaOpacity}; transform: translate(-50%, {(1 - ctaOpacity) * 20}px)">
        <h2 class="mannequin-cta-title">{i18n.t('home.mannequinCta')}</h2>
        <p class="mannequin-cta-subtitle">{i18n.t('home.mannequinSubtitle')}</p>
        <a href="/catalog" class="btn btn-primary btn-lg">{i18n.t('home.mannequinCollection')}</a>
      </div>
  
      <div class="mannequin-progress" aria-hidden="true">
        <div class="mannequin-progress-bar" style="height: {scrollProgress * 100}%"></div>
      </div>
    </div>
    
    <style>
      /* Додатковий стиль для "живого" дихання */
      @keyframes breathe {
        0%, 100% { transform: scale(var(--scale, 1)) translateY(0); }
        50% { transform: scale(var(--scale, 1.005)) translateY(-2px); }
      }
      
      .floating-breath {
        /* Використовуємо змінні з інлайн-стилів Svelte як базу */
        animation: breathe 4s ease-in-out infinite;
      }
    </style>
  </section>

  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <!-- CATEGORIES SECTION                                                    -->
  <!-- ═══════════════════════════════════════════════════════════════════════ -->
  <section class="categories reveal-section" use:addRevealRef aria-labelledby="categories-heading">
    <div class="container">
      <div class="section-header">
        <h2 id="categories-heading" class="section-title">{i18n.t('home.categoriesTitle')}</h2>
        <p class="section-subtitle">{i18n.t('home.categoriesSubtitle')}</p>
      </div>
      <div class="categories-grid">
        {#each categories as cat (cat.slug)}
          <a href="/catalog?category={cat.slug}" class="category-card" aria-label="Категорія: {cat.name}">
            <span class="category-icon" aria-hidden="true">{cat.emoji}</span>
            <h3 class="category-name">{cat.name}</h3>
            <span class="category-link">{i18n.t('home.categoriesView')}</span>
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
        <h2 id="products-heading" class="section-title">{i18n.t('home.productsTitle')}</h2>
        <p class="section-subtitle">{i18n.t('home.productsSubtitle')}</p>
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
            {i18n.t('common.tryAgain')}
          </button>
        </div>
      {:else if products.length > 0}
        <div class="products-grid">
          {#each products as product (product.id)}
            <ProductCard product={mapProduct(product)} />
          {/each}
        </div>
        <div class="products-footer">
          <a href="/catalog" class="btn btn-secondary btn-lg">{i18n.t('home.productsViewAll')}</a>
        </div>
      {:else}
        <div class="products-empty">
          <p>{i18n.t('home.productsEmpty')}</p>
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
        <h2 id="features-heading" class="section-title">{i18n.t('home.featuresTitle')}</h2>
        <p class="section-subtitle">{i18n.t('home.featuresSubtitle')}</p>
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
      <h2 id="newsletter-heading" class="newsletter-title">{i18n.t('home.newsletterTitle')}</h2>
      <p class="newsletter-subtitle">
        {i18n.t('home.newsletterSubtitle')}
      </p>

      {#if newsletterSubmitted}
        <div class="newsletter-success" role="status">
          <span aria-hidden="true">✉️</span>
          <p>{i18n.t('home.newsletterSuccess')}</p>
        </div>
      {:else}
        <form class="newsletter-form" onsubmit={handleNewsletterSubmit} aria-label="Підписка на новини">
          <div class="newsletter-input-group">
            <label for="newsletter-email" class="sr-only">Електронна пошта</label>
            <input
              id="newsletter-email"
              type="email"
              placeholder={i18n.t('home.newsletterPlaceholder')}
              required
              autocomplete="email"
              bind:value={newsletterEmail}
              disabled={newsletterSubmitting}
            />
            <button type="submit" class="btn btn-primary" disabled={newsletterSubmitting}>
              {#if newsletterSubmitting}
                {i18n.t('home.newsletterSubmitting')}
              {:else}
                {i18n.t('home.newsletterSubmit')}
              {/if}
            </button>
          </div>
        </form>
        <p class="newsletter-privacy">
          {i18n.t('home.newsletterPrivacy')}
          <a href="/privacy">{i18n.t('home.newsletterPrivacyLink')}</a>{i18n.t('home.newsletterPrivacyEnd')}
        </p>
      {/if}
    </div>
  </section>

  <!-- Scroll-to-top button -->
  <button
    class="scroll-to-top"
    class:visible={showScrollTop}
    onclick={scrollToTop}
    aria-label={i18n.t('common.scrollToTop')}
    title={i18n.t('common.scrollToTop')}
  >
    <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M18 15l-6-6-6 6"/>
    </svg>
  </button>
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

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* SCROLL-TO-TOP BUTTON                                                      */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .scroll-to-top {
    position: fixed;
    bottom: var(--space-8);
    right: var(--space-8);
    z-index: 100;
    width: 48px;
    height: 48px;
    border-radius: 50%;
    border: 1px solid var(--color-border-light);
    background: var(--color-surface);
    color: var(--color-primary);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 16px rgba(139, 94, 107, 0.12), 0 2px 6px rgba(0, 0, 0, 0.06);
    opacity: 0;
    visibility: hidden;
    transform: translateY(12px);
    transition: opacity 0.3s ease, visibility 0.3s ease, transform 0.3s ease,
                background-color 0.2s ease, box-shadow 0.2s ease;
  }

  .scroll-to-top.visible {
    opacity: 1;
    visibility: visible;
    transform: translateY(0);
  }

  .scroll-to-top:hover {
    background: var(--color-primary);
    color: #fff;
    border-color: var(--color-primary);
    box-shadow: 0 6px 24px rgba(139, 94, 107, 0.25), 0 2px 8px rgba(0, 0, 0, 0.08);
  }

  .scroll-to-top:active {
    transform: translateY(2px);
  }

  @media (max-width: 768px) {
    .scroll-to-top {
      bottom: var(--space-4);
      right: var(--space-4);
      width: 42px;
      height: 42px;
    }
  }

  /* ═══════════════════════════════════════════════════════════════════════════ */
  /* MANNEQUIN SCROLL SHOWCASE                                                 */
  /* ═══════════════════════════════════════════════════════════════════════════ */

  .mannequin-scroll {
    position: relative;
    height: 400vh;
    background: var(--color-background);
  }

  .mannequin-sticky {
    position: sticky;
    top: 0;
    height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
  }

  .mannequin-canvas {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
  }

  .mannequin-svg {
    width: auto;
    height: 70vh;
    max-height: 600px;
    transition: none;
    will-change: transform, opacity;
    filter: drop-shadow(0 4px 30px rgba(139, 94, 107, 0.15));
  }

  .mannequin-body {
    transition: none;
  }

  .mannequin-detail {
    position: absolute;
    display: flex;
    align-items: center;
    gap: var(--space-3);
    will-change: transform, opacity;
    pointer-events: none;
  }

  .mannequin-detail-left {
    left: 8%;
    top: 30%;
  }

  .mannequin-detail-right {
    right: 8%;
    top: 55%;
  }

  .detail-label {
    font-family: var(--font-display);
    font-size: clamp(0.8rem, 1.5vw, 1rem);
    color: var(--color-primary);
    font-weight: 500;
    white-space: nowrap;
    letter-spacing: 0.04em;
  }

  .detail-line {
    display: block;
    width: 60px;
    height: 1px;
    background: var(--color-primary-light);
  }

  .mannequin-cta {
    position: absolute;
    bottom: 8vh;
    left: 50%;
    transform: translateX(-50%);
    text-align: center;
    will-change: transform, opacity;
    pointer-events: auto;
  }

  .mannequin-cta-title {
    font-family: var(--font-display);
    font-size: clamp(1.5rem, 3.5vw, 2.5rem);
    font-weight: 700;
    color: var(--color-text);
    margin-bottom: var(--space-3);
  }

  .mannequin-cta-subtitle {
    font-size: clamp(0.9rem, 1.5vw, 1.1rem);
    color: var(--color-text-light);
    margin-bottom: var(--space-6);
  }

  .mannequin-progress {
    position: absolute;
    right: var(--space-6);
    top: 50%;
    transform: translateY(-50%);
    width: 2px;
    height: 120px;
    background: var(--color-border-light);
    border-radius: 1px;
  }

  .mannequin-progress-bar {
    width: 100%;
    background: var(--color-primary);
    border-radius: 1px;
    transition: none;
  }

  @media (max-width: 768px) {
    .mannequin-scroll {
      height: 300vh;
    }

    .mannequin-svg {
      height: 55vh;
    }

    .mannequin-detail {
      display: none;
    }

    .mannequin-cta {
      bottom: 5vh;
    }

    .mannequin-progress {
      right: var(--space-3);
    }
  }
</style>
