<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  interface ShippingMethod {
    name: string;
    icon: string;
    deliveryTime: string;
    cost: string;
    freeTreshold: boolean;
    description: string;
    features: string[];
  }

  let shippingMethods: ShippingMethod[] = $derived([
    {
      name: i18n.t('shipping.ukrposhtaName'),
      icon: '✉️',
      deliveryTime: i18n.t('shipping.ukrposhtaTime'),
      cost: i18n.t('shipping.ukrposhtaCost'),
      freeTreshold: true,
      description: i18n.t('shipping.ukrposhtaDesc'),
      features: [i18n.t('shipping.ukrposhtaF1'), i18n.t('shipping.ukrposhtaF2'), i18n.t('shipping.ukrposhtaF3')],
    },
    {
      name: i18n.t('shipping.internationalMethodName'),
      icon: '🌍',
      deliveryTime: i18n.t('shipping.internationalMethodTime'),
      cost: i18n.t('shipping.internationalMethodCost'),
      freeTreshold: false,
      description: i18n.t('shipping.internationalMethodDesc'),
      features: [i18n.t('shipping.internationalMethodF1'), i18n.t('shipping.internationalMethodF2'), i18n.t('shipping.internationalMethodF3')],
    },
  ]);
</script>

<svelte:head>
  <title>{i18n.t('shipping.pageTitle')}</title>
  <meta name="description" content={i18n.t('shipping.metaDescription')} />
</svelte:head>

<div class="container">
  <div class="shipping-page">
    <header class="shipping-header">
      <h1>{i18n.t('shipping.title')}</h1>
      <p>{i18n.t('shipping.subtitle')}</p>
    </header>

    <!-- Free Shipping Banner -->
    <div class="free-shipping-banner">
      <span class="banner-icon">🎁</span>
      <div>
        <strong>{i18n.t('shipping.freeShippingTitle')}</strong>
        <p>{i18n.t('shipping.freeShippingText')}</p>
      </div>
    </div>

    <!-- Shipping Methods Cards -->
    <section class="methods-section">
      <h2>{i18n.t('shipping.methods')}</h2>

      <div class="methods-grid">
        {#each shippingMethods as method (method.name)}
          <div class="method-card card">
            <div class="card-body">
              <div class="method-header">
                <span class="method-icon">{method.icon}</span>
                <div>
                  <h3>{method.name}</h3>
                  {#if method.freeTreshold}
                    <span class="free-badge">{i18n.t('shipping.freeLabel')}</span>
                  {/if}
                </div>
              </div>

              <p class="method-description">{method.description}</p>

              <div class="method-details">
                <div class="detail">
                  <span class="detail-label">{i18n.t('shipping.timeLabel')}</span>
                  <span class="detail-value">{method.deliveryTime}</span>
                </div>
                <div class="detail">
                  <span class="detail-label">{i18n.t('shipping.costLabel')}</span>
                  <span class="detail-value">{method.cost}</span>
                </div>
              </div>

              <ul class="method-features">
                {#each method.features as feature}
                  <li>
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
                      <path d="M3.5 8L6.5 11L12.5 5" stroke="var(--color-success)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                    </svg>
                    {feature}
                  </li>
                {/each}
              </ul>
            </div>
          </div>
        {/each}
      </div>
    </section>

    <!-- Comparison Table -->
    <section class="table-section">
      <h2>{i18n.t('shipping.comparison')}</h2>
      <div class="table-wrapper">
        <table class="shipping-table">
          <thead>
            <tr>
              <th>{i18n.t('shipping.tableMethod')}</th>
              <th>{i18n.t('shipping.tableTime')}</th>
              <th>{i18n.t('shipping.tableCost')}</th>
              <th>{i18n.t('shipping.tableFree')}</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td><strong>{i18n.t('shipping.tableUkrposhta')}</strong></td>
              <td>{i18n.t('shipping.tableUkrposhtaTime')}</td>
              <td>{i18n.t('shipping.ukrposhtaCost')}</td>
              <td><span class="check">✓</span></td>
            </tr>
            <tr>
              <td><strong>{i18n.t('shipping.tableInternational')}</strong></td>
              <td>{i18n.t('shipping.tableInternationalTime')}</td>
              <td>{i18n.t('shipping.internationalMethodCost')}</td>
              <td><span class="cross">✗</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- Order Tracking -->
    <section class="tracking-section">
      <div class="tracking-card">
        <div class="tracking-content">
          <h2>{i18n.t('shipping.tracking')}</h2>
          <p>{i18n.t('shipping.trackingText')}</p>
          <div class="tracking-steps">
            <div class="step">
              <div class="step-number">1</div>
              <div class="step-text">
                <strong>{i18n.t('shipping.trackingStep1')}</strong>
                <span>{i18n.t('shipping.trackingStep1Text')}</span>
              </div>
            </div>
            <div class="step-line"></div>
            <div class="step">
              <div class="step-number">2</div>
              <div class="step-text">
                <strong>{i18n.t('shipping.trackingStep2')}</strong>
                <span>{i18n.t('shipping.trackingStep2Text')}</span>
              </div>
            </div>
            <div class="step-line"></div>
            <div class="step">
              <div class="step-number">3</div>
              <div class="step-text">
                <strong>{i18n.t('shipping.trackingStep3')}</strong>
                <span>{i18n.t('shipping.trackingStep3Text')}</span>
              </div>
            </div>
            <div class="step-line"></div>
            <div class="step">
              <div class="step-number">4</div>
              <div class="step-text">
                <strong>{i18n.t('shipping.trackingStep4')}</strong>
                <span>{i18n.t('shipping.trackingStep4Text')}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- International Shipping -->
    <section class="international-section">
      <h2>{i18n.t('shipping.international')}</h2>
      <div class="international-grid">
        <div class="international-card">
          <h3>{i18n.t('shipping.internationalEurope')}</h3>
          <p>{i18n.t('shipping.internationalEuropeText')}</p>
        </div>
        <div class="international-card">
          <h3>{i18n.t('shipping.internationalAmerica')}</h3>
          <p>{i18n.t('shipping.internationalAmericaText')}</p>
        </div>
        <div class="international-card">
          <h3>{i18n.t('shipping.internationalOther')}</h3>
          <p>{i18n.t('shipping.internationalOtherText')}</p>
        </div>
      </div>
    </section>

    <!-- CTA -->
    <section class="shipping-cta">
      <h2>{i18n.t('shipping.ctaTitle')}</h2>
      <p>{i18n.t('shipping.ctaText')}</p>
      <a href="/catalog" class="btn btn-primary btn-lg">{i18n.t('shipping.ctaCatalog')}</a>
    </section>
  </div>
</div>

<style>
  .shipping-page {
    max-width: 960px;
    margin: 0 auto;
    padding-bottom: var(--space-16);
  }

  .shipping-header {
    text-align: center;
    margin-bottom: var(--space-10);
  }

  .shipping-header h1 {
    font-family: var(--font-display);
    font-size: 2.25rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .shipping-header p {
    font-size: 1.0625rem;
    color: var(--color-text-muted);
  }

  /* ─── Free Shipping Banner ───────────────────── */

  .free-shipping-banner {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    background: linear-gradient(135deg, var(--color-secondary-light) 0%, #f5e6d0 100%);
    border-radius: var(--radius-xl);
    padding: var(--space-6);
    margin-bottom: var(--space-12);
  }

  .banner-icon {
    font-size: 2rem;
    flex-shrink: 0;
  }

  .free-shipping-banner strong {
    font-family: var(--font-display);
    font-size: 1.125rem;
    color: var(--color-accent);
  }

  .free-shipping-banner p {
    font-size: 0.875rem;
    color: var(--color-text-light);
    margin-top: var(--space-1);
  }

  /* ─── Methods ────────────────────────────────── */

  .methods-section {
    margin-bottom: var(--space-14);
  }

  .methods-section h2,
  .table-section h2,
  .international-section h2 {
    font-family: var(--font-display);
    font-size: 1.5rem;
    color: var(--color-accent);
    margin-bottom: var(--space-6);
    text-align: center;
  }

  .methods-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: var(--space-6);
  }

  .method-card {
    transition: transform var(--transition-base), box-shadow var(--transition-base);
  }

  .method-card:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-lg);
  }

  .method-header {
    display: flex;
    align-items: flex-start;
    gap: var(--space-3);
    margin-bottom: var(--space-4);
  }

  .method-icon {
    font-size: 1.75rem;
    line-height: 1;
    flex-shrink: 0;
  }

  .method-header h3 {
    font-family: var(--font-display);
    font-size: 1.0625rem;
    color: var(--color-text);
    margin-bottom: var(--space-1);
  }

  .free-badge {
    display: inline-block;
    font-size: 0.6875rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-success);
    background-color: rgba(74, 139, 110, 0.1);
    padding: 2px var(--space-2);
    border-radius: var(--radius-sm);
  }

  .method-description {
    font-size: 0.875rem;
    line-height: 1.65;
    color: var(--color-text-light);
    margin-bottom: var(--space-4);
  }

  .method-details {
    display: flex;
    gap: var(--space-6);
    margin-bottom: var(--space-4);
    padding: var(--space-3) var(--space-4);
    background-color: var(--color-background);
    border-radius: var(--radius-md);
  }

  .detail {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .detail-label {
    font-size: 0.6875rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--color-text-muted);
  }

  .detail-value {
    font-size: 0.875rem;
    font-weight: 600;
    color: var(--color-text);
  }

  .method-features {
    list-style: none;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .method-features li {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    font-size: 0.8125rem;
    color: var(--color-text-light);
  }

  .method-features svg {
    flex-shrink: 0;
  }

  /* ─── Table ──────────────────────────────────── */

  .table-section {
    margin-bottom: var(--space-14);
  }

  .table-wrapper {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .shipping-table {
    width: 100%;
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    overflow: hidden;
    font-size: 0.875rem;
  }

  .shipping-table th,
  .shipping-table td {
    padding: var(--space-3) var(--space-4);
    text-align: left;
    border-bottom: 1px solid var(--color-border-light);
  }

  .shipping-table th {
    background-color: var(--color-surface-hover);
    font-weight: 600;
    font-size: 0.8125rem;
    text-transform: uppercase;
    letter-spacing: 0.03em;
    color: var(--color-text-light);
    white-space: nowrap;
  }

  .shipping-table tbody tr:last-child td {
    border-bottom: none;
  }

  .shipping-table tbody tr:hover {
    background-color: var(--color-surface-hover);
  }

  .check {
    color: var(--color-success);
    font-weight: 700;
  }

  .cross {
    color: var(--color-text-muted);
  }

  /* ─── Tracking ───────────────────────────────── */

  .tracking-section {
    margin-bottom: var(--space-14);
  }

  .tracking-card {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    padding: var(--space-10);
  }

  .tracking-content h2 {
    font-family: var(--font-display);
    font-size: 1.375rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
    text-align: center;
  }

  .tracking-content > p {
    font-size: 0.9375rem;
    color: var(--color-text-light);
    text-align: center;
    line-height: 1.7;
    max-width: 600px;
    margin: 0 auto var(--space-8);
  }

  .tracking-steps {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0;
    flex-wrap: wrap;
  }

  .step {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    gap: var(--space-2);
    min-width: 100px;
  }

  .step-number {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background-color: var(--color-primary);
    color: #fff;
    font-weight: 700;
    font-size: 0.875rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .step-text {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .step-text strong {
    font-size: 0.8125rem;
    color: var(--color-text);
  }

  .step-text span {
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  .step-line {
    width: 40px;
    height: 2px;
    background-color: var(--color-secondary-light);
    margin-bottom: var(--space-8);
    flex-shrink: 0;
  }

  /* ─── International ──────────────────────────── */

  .international-section {
    margin-bottom: var(--space-14);
  }

  .international-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
    gap: var(--space-6);
  }

  .international-card {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    padding: var(--space-6);
    transition: box-shadow var(--transition-base);
  }

  .international-card:hover {
    box-shadow: var(--shadow-md);
  }

  .international-card h3 {
    font-family: var(--font-display);
    font-size: 1.125rem;
    margin-bottom: var(--space-3);
  }

  .international-card p {
    font-size: 0.875rem;
    line-height: 1.7;
    color: var(--color-text-light);
  }

  /* ─── CTA ────────────────────────────────────── */

  .shipping-cta {
    text-align: center;
    padding: var(--space-12) var(--space-6);
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
  }

  .shipping-cta h2 {
    font-family: var(--font-display);
    font-size: 1.5rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .shipping-cta p {
    font-size: 0.9375rem;
    color: var(--color-text-muted);
    margin-bottom: var(--space-6);
  }

  @media (max-width: 640px) {
    .shipping-header h1 {
      font-size: 1.75rem;
    }

    .free-shipping-banner {
      flex-direction: column;
      text-align: center;
    }

    .tracking-card {
      padding: var(--space-6);
    }

    .tracking-steps {
      flex-direction: column;
    }

    .step-line {
      width: 2px;
      height: 24px;
      margin-bottom: 0;
    }

    .step {
      flex-direction: row;
      text-align: left;
    }

    .step-text {
      align-items: flex-start;
    }
  }
</style>
