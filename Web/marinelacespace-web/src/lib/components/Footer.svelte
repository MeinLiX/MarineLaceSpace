<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  let email = $state('');
  let subscribed = $state(false);

  function handleNewsletterSubmit(e: Event) {
    e.preventDefault();
    if (email.trim()) {
      subscribed = true;
      email = '';
      setTimeout(() => (subscribed = false), 4000);
    }
  }

  const currentYear = new Date().getFullYear();
</script>

<footer class="footer">
  <div class="footer-main container">
    <div class="footer-grid">
      <!-- About -->
      <div class="footer-col">
        <h3 class="footer-brand">{i18n.t('common.brand')}</h3>
        <p class="footer-about">
          {i18n.t('footer.aboutDescLong')}
        </p>
      </div>

      <!-- Shop -->
      <div class="footer-col">
        <h4 class="footer-heading">{i18n.t('footer.shop')}</h4>
        <nav class="footer-nav" aria-label={i18n.t('footer.shop')}>
          <a href="/catalog" class="footer-link">{i18n.t('nav.catalog')}</a>
          <a href="/catalog?sort=newest" class="footer-link">{i18n.t('footer.newArrivals')}</a>
          <a href="/catalog?sale=true" class="footer-link">{i18n.t('footer.sale')}</a>
          <a href="/shops" class="footer-link">{i18n.t('nav.allShops')}</a>
        </nav>
      </div>

      <!-- Customer Service -->
      <div class="footer-col">
        <h4 class="footer-heading">{i18n.t('footer.customerService')}</h4>
        <nav class="footer-nav" aria-label={i18n.t('footer.customerService')}>
          <a href="/shipping" class="footer-link">{i18n.t('shipping.title')}</a>
          <a href="/returns" class="footer-link">{i18n.t('footer.returns')}</a>
          <a href="/faq" class="footer-link">{i18n.t('nav.faq')}</a>
          <a href="/contact" class="footer-link">{i18n.t('nav.contact')}</a>
        </nav>
      </div>

      <!-- Connect -->
      <div class="footer-col">
        <h4 class="footer-heading">{i18n.t('footer.connect')}</h4>
        <div class="social-links">
          <a href="https://www.etsy.com/shop/MarineLaceSpace" class="social-link" aria-label="Etsy" target="_blank" rel="noopener noreferrer">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="currentColor" stroke="none" aria-hidden="true">
              <circle cx="12" cy="12" r="11" fill="none" stroke="currentColor" stroke-width="1.5" />
              <text x="12" y="16.5" text-anchor="middle" font-size="12" font-weight="700" font-family="Georgia, serif" fill="currentColor">E</text>
            </svg>
          </a>
          <a href="https://instagram.com/marinelacespace" class="social-link" aria-label="Instagram" target="_blank" rel="noopener noreferrer">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <rect x="2" y="2" width="20" height="20" rx="5" ry="5" />
              <circle cx="12" cy="12" r="5" />
              <circle cx="17.5" cy="6.5" r="1.5" fill="currentColor" stroke="none" />
            </svg>
          </a>
          <a href="https://pinterest.com/marinelacespace" class="social-link" aria-label="Pinterest" target="_blank" rel="noopener noreferrer">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <circle cx="12" cy="12" r="10" />
              <path d="M8 12c0-2.2 1.8-4 4-4s4 1.8 4 4-1.8 4-4 4" />
              <path d="M9 18l1.5-6" />
            </svg>
          </a>
          <a href="https://tiktok.com/@marinelacespace" class="social-link" aria-label="TikTok" target="_blank" rel="noopener noreferrer">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path d="M9 12a4 4 0 104 4V4a5 5 0 005 5" />
            </svg>
          </a>
        </div>

        <!-- Newsletter -->
        <div class="newsletter">
          <p class="newsletter-label">{i18n.t('footer.newsletter')}</p>
          {#if subscribed}
            <p class="newsletter-success">{i18n.t('footer.subscribeSuccess')}</p>
          {:else}
            <form class="newsletter-form" onsubmit={handleNewsletterSubmit}>
              <input
                type="email"
                class="newsletter-input"
                placeholder={i18n.t('footer.newsletterPlaceholder')}
                bind:value={email}
                required
                aria-label={i18n.t('footer.newsletter')}
              />
              <button type="submit" class="newsletter-btn" aria-label={i18n.t('common.submit')}>
                <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                  <line x1="5" y1="12" x2="19" y2="12" />
                  <polyline points="12 5 19 12 12 19" />
                </svg>
              </button>
            </form>
          {/if}
        </div>
      </div>
    </div>
  </div>

  <!-- Bottom Bar -->
  <div class="footer-bottom">
    <div class="footer-bottom-inner container">
      <p class="copyright">{i18n.t('footer.copyright', { year: currentYear })}</p>
      <nav class="legal-links" aria-label="Legal">
        <a href="/privacy" class="legal-link">{i18n.t('privacy.title')}</a>
        <a href="/terms" class="legal-link">{i18n.t('terms.title')}</a>
        <a href="/faq" class="legal-link">{i18n.t('nav.faq')}</a>
      </nav>
      <div class="payment-methods">
        <span class="payment-icon" aria-label="Visa">Visa</span>
        <span class="payment-icon" aria-label="Mastercard">MC</span>
        <span class="payment-icon" aria-label="Apple Pay">Apple Pay</span>
        <span class="payment-icon" aria-label="Google Pay">G Pay</span>
      </div>
    </div>
  </div>
</footer>

<style>
  .footer {
    background-color: #F5F0EC;
    margin-top: auto;
  }

  .footer-main {
    padding-block: var(--space-16) var(--space-12);
  }

  .footer-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: var(--space-10);
  }

  @media (min-width: 640px) {
    .footer-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }

  @media (min-width: 1024px) {
    .footer-grid {
      grid-template-columns: 1.5fr 1fr 1fr 1.2fr;
    }
  }

  .footer-brand {
    font-family: var(--font-display);
    font-size: 1.25rem;
    font-weight: 700;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .footer-about {
    font-size: 0.875rem;
    color: var(--color-text-light);
    line-height: 1.7;
  }

  .footer-heading {
    font-family: var(--font-body);
    font-size: 0.8125rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--color-text);
    margin-bottom: var(--space-4);
  }

  .footer-nav {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .footer-link {
    font-size: 0.875rem;
    color: var(--color-text-light);
    transition: color var(--transition-fast);
    text-decoration: none;
  }

  .footer-link:hover {
    color: var(--color-primary);
  }

  /* Social */
  .social-links {
    display: flex;
    gap: var(--space-3);
    margin-bottom: var(--space-6);
  }

  .social-link {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: var(--radius-full);
    color: var(--color-text-light);
    background-color: var(--color-surface);
    transition: color var(--transition-fast), background-color var(--transition-fast);
    text-decoration: none;
  }

  .social-link:hover {
    color: var(--color-primary);
    background-color: var(--color-surface-hover);
  }

  /* Newsletter */
  .newsletter-label {
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text-light);
    margin-bottom: var(--space-2);
  }

  .newsletter-form {
    display: flex;
    border: 1px solid var(--color-border);
    border-radius: var(--radius-full);
    overflow: hidden;
    background-color: var(--color-surface);
    transition: border-color var(--transition-fast);
  }

  .newsletter-form:focus-within {
    border-color: var(--color-primary-light);
  }

  .newsletter-input {
    flex: 1;
    border: none;
    padding: var(--space-2) var(--space-4);
    font-size: 0.8125rem;
    background: transparent;
    outline: none;
    min-width: 0;
  }

  .newsletter-input::placeholder {
    color: var(--color-text-muted);
  }

  .newsletter-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-2) var(--space-3);
    background-color: var(--color-primary);
    color: #FFFFFF;
    border: none;
    cursor: pointer;
    transition: background-color var(--transition-fast);
  }

  .newsletter-btn:hover {
    background-color: var(--color-primary-dark);
  }

  .newsletter-success {
    font-size: 0.8125rem;
    color: var(--color-success);
    font-weight: 500;
  }

  /* Bottom Bar */
  .footer-bottom {
    border-top: 1px solid var(--color-border);
    background-color: #EDE7E2;
  }

  .footer-bottom-inner {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-3);
    padding-block: var(--space-4);
  }

  @media (min-width: 640px) {
    .footer-bottom-inner {
      flex-direction: row;
      justify-content: space-between;
    }
  }

  .copyright {
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  .payment-methods {
    display: flex;
    gap: var(--space-2);
  }

  .payment-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-1) var(--space-2);
    font-size: 0.625rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-text-muted);
    background-color: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-sm);
  }

  /* Legal links */
  .legal-links {
    display: flex;
    flex-wrap: wrap;
    gap: var(--space-2) var(--space-4);
    justify-content: center;
  }

  .legal-link {
    font-size: 0.75rem;
    color: var(--color-text-muted);
    text-decoration: none;
    transition: color var(--transition-fast);
  }

  .legal-link:hover {
    color: var(--color-primary);
  }
</style>
