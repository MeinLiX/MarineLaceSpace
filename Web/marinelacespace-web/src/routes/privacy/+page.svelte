<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
</script>

<svelte:head>
  <title>{i18n.t('privacy.pageTitle')}</title>
  <meta name="description" content={i18n.t('privacy.metaDescription')} />
</svelte:head>

<div class="container">
  <article class="policy">
    <header class="policy-header">
      <h1>{i18n.t('privacy.title')}</h1>
      <p class="updated">{i18n.t('privacy.lastUpdated')}</p>
    </header>

    <section class="policy-section">
      <p>{@html i18n.t('privacy.intro')}</p>
      <p>{i18n.t('privacy.introConsent')}</p>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.collection')}</h2>
      <p>{i18n.t('privacy.collectionText')}</p>
      <ul>
        <li>{@html i18n.t('privacy.collectionItem1')}</li>
        <li>{@html i18n.t('privacy.collectionItem2')}</li>
        <li>{@html i18n.t('privacy.collectionItem3')}</li>
        <li>{@html i18n.t('privacy.collectionItem4')}</li>
        <li>{@html i18n.t('privacy.collectionItem5')}</li>
        <li>{@html i18n.t('privacy.collectionItem6')}</li>
        <li>{@html i18n.t('privacy.collectionItem7')}</li>
      </ul>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.usage')}</h2>
      <p>{i18n.t('privacy.usageText')}</p>
      <ul>
        <li>{i18n.t('privacy.usageItem1')}</li>
        <li>{i18n.t('privacy.usageItem2')}</li>
        <li>{i18n.t('privacy.usageItem3')}</li>
        <li>{i18n.t('privacy.usageItem4')}</li>
        <li>{i18n.t('privacy.usageItem5')}</li>
        <li>{i18n.t('privacy.usageItem6')}</li>
        <li>{i18n.t('privacy.usageItem7')}</li>
        <li>{i18n.t('privacy.usageItem8')}</li>
        <li>{i18n.t('privacy.usageItem9')}</li>
      </ul>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.cookies')}</h2>
      <p>{i18n.t('privacy.cookiesText')}</p>
      <h3>{i18n.t('privacy.cookiesSubtitle')}</h3>
      <ul>
        <li>{@html i18n.t('privacy.cookiesItem1')}</li>
        <li>{@html i18n.t('privacy.cookiesItem2')}</li>
        <li>{@html i18n.t('privacy.cookiesItem3')}</li>
        <li>{@html i18n.t('privacy.cookiesItem4')}</li>
      </ul>
      <p>{i18n.t('privacy.cookiesOutro')}</p>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.sharing')}</h2>
      <p>{i18n.t('privacy.sharingText')}</p>
      <ul>
        <li>{@html i18n.t('privacy.sharingItem1')}</li>
        <li>{@html i18n.t('privacy.sharingItem2')}</li>
        <li>{@html i18n.t('privacy.sharingItem3')}</li>
        <li>{@html i18n.t('privacy.sharingItem4')}</li>
        <li>{@html i18n.t('privacy.sharingItem5')}</li>
      </ul>
      <p>{i18n.t('privacy.sharingOutro')}</p>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.retention')}</h2>
      <p>{i18n.t('privacy.retentionText')}</p>
      <ul>
        <li>{i18n.t('privacy.retentionItem1')}</li>
        <li>{i18n.t('privacy.retentionItem2')}</li>
        <li>{i18n.t('privacy.retentionItem3')}</li>
        <li>{i18n.t('privacy.retentionItem4')}</li>
      </ul>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.rights')}</h2>
      <p>{i18n.t('privacy.rightsText')}</p>
      <ul>
        <li>{i18n.t('privacy.rightsItem1')}</li>
        <li>{i18n.t('privacy.rightsItem2')}</li>
        <li>{i18n.t('privacy.rightsItem3')}</li>
        <li>{i18n.t('privacy.rightsItem4')}</li>
        <li>{i18n.t('privacy.rightsItem5')}</li>
        <li>{i18n.t('privacy.rightsItem6')}</li>
        <li>{i18n.t('privacy.rightsItem7')}</li>
        <li>{i18n.t('privacy.rightsItem8')}</li>
      </ul>
      <p>{i18n.t('privacy.rightsOutro')}</p>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.security')}</h2>
      <p>{i18n.t('privacy.securityText')}</p>
    </section>

    <section class="policy-section">
      <h2>{i18n.t('privacy.contact')}</h2>
      <p>{i18n.t('privacy.contactText')}</p>
      <div class="contact-block">
        <p><strong>MarineLaceSpace</strong></p>
        <p>📧 Email: <a href="mailto:privacy@marinelacespace.com">privacy@marinelacespace.com</a></p>
        <p>📞 {i18n.t('contact.phone')}: <a href="tel:+380501234567">+38 (050) 123-45-67</a></p>
        <p>📍 {i18n.t('contact.address')}: {i18n.t('contact.addressValue')}</p>
      </div>
    </section>
  </article>
</div>

<style>
  .policy {
    max-width: 800px;
    margin: 0 auto;
    padding-bottom: var(--space-16);
  }

  .policy-header {
    text-align: center;
    margin-bottom: var(--space-12);
    padding-bottom: var(--space-8);
    border-bottom: 1px solid var(--color-border-light);
  }

  .policy-header h1 {
    font-family: var(--font-display);
    font-size: 2.25rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .updated {
    font-size: 0.875rem;
    color: var(--color-text-muted);
  }

  .policy-section {
    margin-bottom: var(--space-10);
  }

  .policy-section h2 {
    font-family: var(--font-display);
    font-size: 1.375rem;
    color: var(--color-accent);
    margin-bottom: var(--space-4);
    padding-bottom: var(--space-2);
    border-bottom: 2px solid var(--color-secondary-light);
  }

  .policy-section h3 {
    font-family: var(--font-display);
    font-size: 1.125rem;
    color: var(--color-text);
    margin-top: var(--space-4);
    margin-bottom: var(--space-3);
  }

  .policy-section p {
    font-size: 0.9375rem;
    line-height: 1.75;
    color: var(--color-text-light);
    margin-bottom: var(--space-4);
  }

  .policy-section ul {
    list-style: none;
    padding: 0;
    margin-bottom: var(--space-4);
  }

  .policy-section ul li {
    position: relative;
    padding-left: var(--space-6);
    margin-bottom: var(--space-3);
    font-size: 0.9375rem;
    line-height: 1.7;
    color: var(--color-text-light);
  }

  .policy-section ul li::before {
    content: '';
    position: absolute;
    left: 0;
    top: 0.55em;
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background-color: var(--color-secondary);
  }

  .contact-block {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    padding: var(--space-6);
    margin-top: var(--space-4);
  }

  .contact-block p {
    margin-bottom: var(--space-2);
    color: var(--color-text);
  }

  .contact-block a {
    color: var(--color-primary);
    text-decoration: underline;
    text-underline-offset: 2px;
  }

  .contact-block a:hover {
    color: var(--color-primary-dark);
  }

  @media (max-width: 640px) {
    .policy-header h1 {
      font-size: 1.75rem;
    }

    .policy-section h2 {
      font-size: 1.2rem;
    }
  }
</style>
