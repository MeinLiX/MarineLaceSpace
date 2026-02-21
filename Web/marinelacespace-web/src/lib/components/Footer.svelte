<script lang="ts">
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
        <h3 class="footer-brand">MarineLaceSpace</h3>
        <p class="footer-about">
          Premium lingerie curated with care. Discover elegance, comfort, and confidence 
          in every piece — from delicate lace to everyday essentials.
        </p>
      </div>

      <!-- Shop -->
      <div class="footer-col">
        <h4 class="footer-heading">Shop</h4>
        <nav class="footer-nav" aria-label="Shop links">
          <a href="/catalog" class="footer-link">Catalog</a>
          <a href="/catalog?sort=newest" class="footer-link">New Arrivals</a>
          <a href="/catalog?sale=true" class="footer-link">Sale</a>
          <a href="/shops" class="footer-link">All Shops</a>
        </nav>
      </div>

      <!-- Customer Service -->
      <div class="footer-col">
        <h4 class="footer-heading">Customer Service</h4>
        <nav class="footer-nav" aria-label="Customer service links">
          <a href="/shipping" class="footer-link">Shipping</a>
          <a href="/returns" class="footer-link">Returns & Exchanges</a>
          <a href="/faq" class="footer-link">FAQ</a>
          <a href="/contact" class="footer-link">Contact Us</a>
        </nav>
      </div>

      <!-- Connect -->
      <div class="footer-col">
        <h4 class="footer-heading">Connect</h4>
        <div class="social-links">
          <a href="#" class="social-link" aria-label="Instagram">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <rect x="2" y="2" width="20" height="20" rx="5" ry="5" />
              <circle cx="12" cy="12" r="5" />
              <circle cx="17.5" cy="6.5" r="1.5" fill="currentColor" stroke="none" />
            </svg>
          </a>
          <a href="#" class="social-link" aria-label="Facebook">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path d="M18 2h-3a5 5 0 00-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 011-1h3z" />
            </svg>
          </a>
          <a href="#" class="social-link" aria-label="Pinterest">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <circle cx="12" cy="12" r="10" />
              <path d="M8 12c0-2.2 1.8-4 4-4s4 1.8 4 4-1.8 4-4 4" />
              <path d="M9 18l1.5-6" />
            </svg>
          </a>
          <a href="#" class="social-link" aria-label="Twitter / X">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
              <path d="M4 4l16 16M4 20L20 4" />
            </svg>
          </a>
        </div>

        <!-- Newsletter -->
        <div class="newsletter">
          <p class="newsletter-label">Stay in the loop</p>
          {#if subscribed}
            <p class="newsletter-success">Thank you for subscribing!</p>
          {:else}
            <form class="newsletter-form" on:submit={handleNewsletterSubmit}>
              <input
                type="email"
                class="newsletter-input"
                placeholder="Your email"
                bind:value={email}
                required
                aria-label="Email for newsletter"
              />
              <button type="submit" class="newsletter-btn" aria-label="Subscribe to newsletter">
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
      <p class="copyright">&copy; {currentYear} MarineLaceSpace. All rights reserved.</p>
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
</style>
