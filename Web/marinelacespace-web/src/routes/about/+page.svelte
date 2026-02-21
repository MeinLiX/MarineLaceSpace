<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let values = $derived([
    {
      icon: '✋',
      title: i18n.t('about.handmade'),
      description: i18n.t('about.handmadeText'),
    },
    {
      icon: '🌿',
      title: i18n.t('about.natural'),
      description: i18n.t('about.naturalText'),
    },
    {
      icon: '🇺🇦',
      title: i18n.t('about.ukrainian'),
      description: i18n.t('about.ukrainianText'),
    },
    {
      icon: '💎',
      title: i18n.t('about.quality'),
      description: i18n.t('about.qualityText'),
    },
    {
      icon: '🎨',
      title: i18n.t('about.design'),
      description: i18n.t('about.designText'),
    },
    {
      icon: '♻️',
      title: i18n.t('about.sustainable'),
      description: i18n.t('about.sustainableText'),
    },
  ]);
</script>

<svelte:head>
  <title>{i18n.t('about.pageTitle')}</title>
  <meta name="description" content={i18n.t('about.metaDescription')} />
</svelte:head>

<div class="container">
  <div class="about-page">
    <!-- Hero -->
    <section class="hero">
      <div class="hero-content">
        <p class="hero-tagline">{i18n.t('about.subtitle')}</p>
        <h1>MarineLaceSpace</h1>
        <p class="hero-subtitle">
          {i18n.t('about.heroSubtitle')}
        </p>
      </div>
    </section>

    <!-- Story -->
    <section class="story-section">
      <div class="story-layout">
        <div class="story-image-placeholder">
          <div class="image-inner">
            <span class="image-emoji">🧵</span>
            <p>{i18n.t('about.storyImageLabel')}</p>
          </div>
        </div>

        <div class="story-content">
          <h2>{i18n.t('about.story')}</h2>
          <p>{i18n.t('about.storyText1')}</p>
          <p>{i18n.t('about.storyText2')}</p>
          <p>{i18n.t('about.storyText3')}</p>
          <a
            href="https://www.etsy.com/shop/MarineLaceSpace"
            target="_blank"
            rel="noopener noreferrer"
            class="btn btn-outline etsy-link"
          >
            {i18n.t('about.visitEtsy')}
          </a>
        </div>
      </div>
    </section>

    <!-- Mission -->
    <section class="mission-section">
      <div class="mission-inner">
        <h2>{i18n.t('about.mission')}</h2>
        <blockquote>
          {i18n.t('about.missionText')}
        </blockquote>
      </div>
    </section>

    <!-- Values -->
    <section class="values-section">
      <h2>{i18n.t('about.values')}</h2>
      <div class="values-grid">
        {#each values as value (value.title)}
          <div class="value-card">
            <span class="value-icon">{value.icon}</span>
            <h3>{value.title}</h3>
            <p>{value.description}</p>
          </div>
        {/each}
      </div>
    </section>

    <!-- Team Placeholder -->
    <section class="team-section">
      <h2>{i18n.t('about.team')}</h2>
      <p class="team-intro">
        {i18n.t('about.teamText')}
      </p>
      <div class="team-grid">
        <div class="team-card">
          <div class="team-avatar">
            <span>👩‍🎨</span>
          </div>
          <h4>{i18n.t('about.teamFounder')}</h4>
          <p>{i18n.t('about.teamFounderText')}</p>
        </div>
        <div class="team-card">
          <div class="team-avatar">
            <span>🧶</span>
          </div>
          <h4>{i18n.t('about.teamSewers')}</h4>
          <p>{i18n.t('about.teamSewersText')}</p>
        </div>
        <div class="team-card">
          <div class="team-avatar">
            <span>📸</span>
          </div>
          <h4>{i18n.t('about.teamCreative')}</h4>
          <p>{i18n.t('about.teamCreativeText')}</p>
        </div>
      </div>
    </section>

    <!-- CTA -->
    <section class="about-cta">
      <h2>{i18n.t('about.ctaTitle')}</h2>
      <p>{i18n.t('about.ctaText')}</p>
      <div class="cta-actions">
        <a href="/catalog" class="btn btn-primary btn-lg">{i18n.t('about.ctaCatalog')}</a>
        <a href="/contact" class="btn btn-outline btn-lg">{i18n.t('about.ctaContact')}</a>
      </div>
    </section>
  </div>
</div>

<style>
  .about-page {
    max-width: 1000px;
    margin: 0 auto;
    padding-bottom: var(--space-16);
  }

  /* ─── Hero ──────────────────────────────────── */

  .hero {
    text-align: center;
    padding: var(--space-12) var(--space-4);
    margin-bottom: var(--space-14);
    background: linear-gradient(135deg, rgba(139, 94, 107, 0.06) 0%, rgba(212, 165, 116, 0.08) 100%);
    border-radius: var(--radius-xl);
  }

  .hero-tagline {
    font-size: 0.8125rem;
    text-transform: uppercase;
    letter-spacing: 0.15em;
    color: var(--color-secondary);
    font-weight: 600;
    margin-bottom: var(--space-4);
  }

  .hero h1 {
    font-family: var(--font-display);
    font-size: 3rem;
    color: var(--color-primary);
    margin-bottom: var(--space-5);
    letter-spacing: -0.02em;
  }

  .hero-subtitle {
    font-size: 1.125rem;
    line-height: 1.75;
    color: var(--color-text-light);
    max-width: 580px;
    margin: 0 auto;
  }

  /* ─── Story ─────────────────────────────────── */

  .story-section {
    margin-bottom: var(--space-14);
  }

  .story-layout {
    display: grid;
    grid-template-columns: 1fr 1.2fr;
    gap: var(--space-10);
    align-items: center;
  }

  .story-image-placeholder {
    border-radius: var(--radius-xl);
    overflow: hidden;
    border: 1px solid var(--color-border-light);
    aspect-ratio: 4 / 5;
  }

  .image-inner {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    background: linear-gradient(135deg, var(--color-surface-hover) 0%, var(--color-background) 100%);
    gap: var(--space-3);
  }

  .image-emoji {
    font-size: 3rem;
  }

  .image-inner p {
    font-size: 0.875rem;
    color: var(--color-text-muted);
    font-style: italic;
  }

  .story-content h2 {
    font-family: var(--font-display);
    font-size: 1.75rem;
    color: var(--color-primary);
    margin-bottom: var(--space-5);
  }

  .story-content p {
    font-size: 0.9375rem;
    line-height: 1.8;
    color: var(--color-text-light);
    margin-bottom: var(--space-4);
  }

  .etsy-link {
    margin-top: var(--space-2);
  }

  /* ─── Mission ───────────────────────────────── */

  .mission-section {
    margin-bottom: var(--space-14);
  }

  .mission-inner {
    text-align: center;
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    padding: var(--space-12) var(--space-8);
  }

  .mission-inner h2 {
    font-family: var(--font-display);
    font-size: 1.5rem;
    color: var(--color-accent);
    margin-bottom: var(--space-6);
  }

  .mission-inner blockquote {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-style: italic;
    line-height: 1.8;
    color: var(--color-primary);
    max-width: 700px;
    margin: 0 auto;
    position: relative;
    padding: 0 var(--space-6);
  }

  .mission-inner blockquote::before {
    content: '"';
    font-size: 3rem;
    color: var(--color-secondary-light);
    position: absolute;
    left: -8px;
    top: -12px;
    font-family: var(--font-display);
    line-height: 1;
  }

  /* ─── Values ────────────────────────────────── */

  .values-section {
    margin-bottom: var(--space-14);
  }

  .values-section h2 {
    font-family: var(--font-display);
    font-size: 1.75rem;
    color: var(--color-accent);
    text-align: center;
    margin-bottom: var(--space-8);
  }

  .values-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-6);
  }

  .value-card {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    padding: var(--space-6);
    text-align: center;
    transition: transform var(--transition-base), box-shadow var(--transition-base);
  }

  .value-card:hover {
    transform: translateY(-3px);
    box-shadow: var(--shadow-lg);
  }

  .value-icon {
    font-size: 2rem;
    display: block;
    margin-bottom: var(--space-4);
  }

  .value-card h3 {
    font-family: var(--font-display);
    font-size: 1.0625rem;
    color: var(--color-text);
    margin-bottom: var(--space-3);
  }

  .value-card p {
    font-size: 0.8125rem;
    line-height: 1.7;
    color: var(--color-text-light);
  }

  /* ─── Team ──────────────────────────────────── */

  .team-section {
    margin-bottom: var(--space-14);
    text-align: center;
  }

  .team-section h2 {
    font-family: var(--font-display);
    font-size: 1.75rem;
    color: var(--color-accent);
    margin-bottom: var(--space-3);
  }

  .team-intro {
    font-size: 0.9375rem;
    color: var(--color-text-muted);
    max-width: 560px;
    margin: 0 auto var(--space-8);
    line-height: 1.7;
  }

  .team-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: var(--space-6);
  }

  .team-card {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    padding: var(--space-8) var(--space-5);
    transition: box-shadow var(--transition-base);
  }

  .team-card:hover {
    box-shadow: var(--shadow-md);
  }

  .team-avatar {
    width: 72px;
    height: 72px;
    border-radius: 50%;
    background: linear-gradient(135deg, var(--color-secondary-light) 0%, #f5e6d0 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto var(--space-4);
    font-size: 1.75rem;
  }

  .team-card h4 {
    font-family: var(--font-display);
    font-size: 1rem;
    color: var(--color-text);
    margin-bottom: var(--space-2);
  }

  .team-card p {
    font-size: 0.8125rem;
    line-height: 1.65;
    color: var(--color-text-light);
  }

  /* ─── CTA ───────────────────────────────────── */

  .about-cta {
    text-align: center;
    padding: var(--space-12) var(--space-6);
    background: linear-gradient(135deg, rgba(139, 94, 107, 0.06) 0%, rgba(212, 165, 116, 0.08) 100%);
    border-radius: var(--radius-xl);
  }

  .about-cta h2 {
    font-family: var(--font-display);
    font-size: 1.5rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .about-cta p {
    font-size: 0.9375rem;
    color: var(--color-text-muted);
    margin-bottom: var(--space-6);
  }

  .cta-actions {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-4);
    flex-wrap: wrap;
  }

  /* ─── Responsive ────────────────────────────── */

  @media (max-width: 860px) {
    .story-layout {
      grid-template-columns: 1fr;
    }

    .story-image-placeholder {
      max-width: 400px;
      margin: 0 auto;
    }

    .values-grid,
    .team-grid {
      grid-template-columns: 1fr 1fr;
    }
  }

  @media (max-width: 640px) {
    .hero h1 {
      font-size: 2.25rem;
    }

    .values-grid,
    .team-grid {
      grid-template-columns: 1fr;
    }

    .mission-inner {
      padding: var(--space-8) var(--space-4);
    }

    .mission-inner blockquote {
      font-size: 1.0625rem;
    }
  }
</style>
