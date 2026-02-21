<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
</script>

<svelte:head>
  <title>{i18n.t('terms.pageTitle')}</title>
  <meta name="description" content={i18n.t('terms.metaDescription')} />
</svelte:head>

<div class="container">
  <article class="terms">
    <header class="terms-header">
      <h1>{i18n.t('terms.title')}</h1>
      <p class="updated">{i18n.t('terms.lastUpdated')}</p>
    </header>

    <section class="terms-section">
      <h2>{i18n.t('terms.acceptance')}</h2>
      <p>{@html i18n.t('terms.acceptanceText')}</p>
      <p>{i18n.t('terms.acceptanceText2')}</p>
      <p>{i18n.t('terms.acceptanceText3')}</p>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.usage')}</h2>
      <p>{i18n.t('terms.usageText')}</p>
      <ul>
        <li>{i18n.t('terms.usageItem1')}</li>
        <li>{i18n.t('terms.usageItem2')}</li>
        <li>{i18n.t('terms.usageItem3')}</li>
        <li>{i18n.t('terms.usageItem4')}</li>
        <li>{i18n.t('terms.usageItem5')}</li>
      </ul>
      <p>{i18n.t('terms.usageOutro')}</p>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.accounts')}</h2>
      <p>{i18n.t('terms.accountsText')}</p>
      <ul>
        <li>{i18n.t('terms.accountsItem1')}</li>
        <li>{i18n.t('terms.accountsItem2')}</li>
        <li>{i18n.t('terms.accountsItem3')}</li>
        <li>{i18n.t('terms.accountsItem4')}</li>
      </ul>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.purchases')}</h2>
      <p>{i18n.t('terms.purchasesText')}</p>
      <ul>
        <li>{i18n.t('terms.purchasesItem1')}</li>
        <li>{i18n.t('terms.purchasesItem2')}</li>
        <li>{i18n.t('terms.purchasesItem3')}</li>
        <li>{i18n.t('terms.purchasesItem4')}</li>
        <li>{i18n.t('terms.purchasesItem5')}</li>
      </ul>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.delivery')}</h2>
      <h3>{i18n.t('terms.deliverySubtitle')}</h3>
      <p>{i18n.t('terms.deliveryText')}</p>
      <ul>
        <li>{i18n.t('terms.deliveryItem1')}</li>
        <li>{i18n.t('terms.deliveryItem2')}</li>
        <li>{i18n.t('terms.deliveryItem3')}</li>
      </ul>
      <h3>{i18n.t('terms.returnsSubtitle')}</h3>
      <p>{i18n.t('terms.returnsText')}</p>
      <ul>
        <li>{i18n.t('terms.returnsItem1')}</li>
        <li>{i18n.t('terms.returnsItem2')}</li>
        <li>{i18n.t('terms.returnsItem3')}</li>
        <li>{i18n.t('terms.returnsItem4')}</li>
      </ul>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.intellectual')}</h2>
      <p>{@html i18n.t('terms.intellectualText')}</p>
      <ul>
        <li>{i18n.t('terms.intellectualItem1')}</li>
        <li>{i18n.t('terms.intellectualItem2')}</li>
        <li>{i18n.t('terms.intellectualItem3')}</li>
      </ul>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.limitation')}</h2>
      <p>{i18n.t('terms.limitationText')}</p>
      <ul>
        <li>{i18n.t('terms.limitationItem1')}</li>
        <li>{i18n.t('terms.limitationItem2')}</li>
        <li>{i18n.t('terms.limitationItem3')}</li>
        <li>{i18n.t('terms.limitationItem4')}</li>
        <li>{i18n.t('terms.limitationItem5')}</li>
      </ul>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.changes')}</h2>
      <p>{i18n.t('terms.changesText')}</p>
    </section>

    <section class="terms-section">
      <h2>{i18n.t('terms.governing')}</h2>
      <p>{i18n.t('terms.governingText')}</p>
      <div class="contact-block">
        <p><strong>MarineLaceSpace</strong></p>
        <p>📧 Email: <a href="mailto:support@marinelacespace.com">support@marinelacespace.com</a></p>
        <p>📞 {i18n.t('contact.phone')}: <a href="tel:+380501234567">+38 (050) 123-45-67</a></p>
        <p>📍 {i18n.t('contact.address')}: {i18n.t('contact.addressValue')}</p>
      </div>
    </section>
  </article>
</div>

<style>
  .terms {
    max-width: 800px;
    margin: 0 auto;
    padding-bottom: var(--space-16);
  }

  .terms-header {
    text-align: center;
    margin-bottom: var(--space-12);
    padding-bottom: var(--space-8);
    border-bottom: 1px solid var(--color-border-light);
  }

  .terms-header h1 {
    font-family: var(--font-display);
    font-size: 2.25rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .updated {
    font-size: 0.875rem;
    color: var(--color-text-muted);
  }

  .terms-section {
    margin-bottom: var(--space-10);
  }

  .terms-section h2 {
    font-family: var(--font-display);
    font-size: 1.375rem;
    color: var(--color-accent);
    margin-bottom: var(--space-4);
    padding-bottom: var(--space-2);
    border-bottom: 2px solid var(--color-secondary-light);
  }

  .terms-section h3 {
    font-family: var(--font-display);
    font-size: 1.125rem;
    color: var(--color-text);
    margin-top: var(--space-6);
    margin-bottom: var(--space-3);
  }

  .terms-section p {
    font-size: 0.9375rem;
    line-height: 1.75;
    color: var(--color-text-light);
    margin-bottom: var(--space-4);
  }

  .terms-section ul {
    list-style: none;
    padding: 0;
    margin-bottom: var(--space-4);
  }

  .terms-section ul li {
    position: relative;
    padding-left: var(--space-6);
    margin-bottom: var(--space-3);
    font-size: 0.9375rem;
    line-height: 1.7;
    color: var(--color-text-light);
  }

  .terms-section ul li::before {
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
    .terms-header h1 {
      font-size: 1.75rem;
    }

    .terms-section h2 {
      font-size: 1.2rem;
    }
  }
</style>
