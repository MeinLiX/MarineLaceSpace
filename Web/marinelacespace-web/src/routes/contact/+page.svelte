<script lang="ts">
  import { i18n } from '$i18n/index.svelte';
  import { notificationStore } from '$stores/notification.svelte';
  import { sendContactForm } from '$api/notification';

  // ─── Form State ─────────────────────────────────

  let name = $state('');
  let email = $state('');
  let subject = $state('');
  let message = $state('');
  let isSubmitting = $state(false);
  let isSubmitted = $state(false);

  let errors = $state<Record<string, string>>({});

  let subjectOptions = $derived([
    { value: '', label: i18n.t('contact.subjectPlaceholder') },
    { value: 'order', label: i18n.t('contact.subjectOrder') },
    { value: 'shipping', label: i18n.t('contact.subjectShipping') },
    { value: 'return', label: i18n.t('contact.subjectReturn') },
    { value: 'sizing', label: i18n.t('contact.subjectSizing') },
    { value: 'custom', label: i18n.t('contact.subjectCustom') },
    { value: 'collaboration', label: i18n.t('contact.subjectCollaboration') },
    { value: 'other', label: i18n.t('contact.subjectOther') },
  ]);

  // ─── Validation ─────────────────────────────────

  function validate(): boolean {
    const newErrors: Record<string, string> = {};

    if (!name.trim()) {
      newErrors.name = i18n.t('contact.nameRequired');
    }

    if (!email.trim()) {
      newErrors.email = i18n.t('contact.emailRequired');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = i18n.t('contact.emailInvalid');
    }

    if (!subject) {
      newErrors.subject = i18n.t('contact.subjectRequired');
    }

    if (!message.trim()) {
      newErrors.message = i18n.t('contact.messageRequired');
    } else if (message.trim().length < 10) {
      newErrors.message = i18n.t('contact.messageMinLength');
    }

    errors = newErrors;
    return Object.keys(newErrors).length === 0;
  }

  // ─── Submit ─────────────────────────────────────

  async function handleSubmit(e: SubmitEvent) {
    e.preventDefault();

    if (!validate()) return;

    isSubmitting = true;

    try {
      await sendContactForm({
        name: name.trim(),
        email: email.trim(),
        subject: subject,
        message: message.trim()
      });
      isSubmitted = true;
    } catch {
      notificationStore.error(i18n.t('contact.submitError'));
    } finally {
      isSubmitting = false;
    }
  }

  function resetForm() {
    name = '';
    email = '';
    subject = '';
    message = '';
    errors = {};
    isSubmitted = false;
  }
</script>

<svelte:head>
  <title>{i18n.t('contact.pageTitle')}</title>
  <meta name="description" content={i18n.t('contact.metaDescription')} />
</svelte:head>

<div class="container">
  <div class="contact-page">
    <header class="contact-header">
      <h1>{i18n.t('contact.title')}</h1>
      <p>{i18n.t('contact.subtitle')}</p>
    </header>

    <div class="contact-layout">
      <!-- Contact Form -->
      <div class="form-section">
        {#if isSubmitted}
          <div class="success-message">
            <div class="success-icon">✓</div>
            <h2>{i18n.t('contact.success')}</h2>
            <p>{i18n.t('contact.successText')}</p>
            <button class="btn btn-outline" onclick={resetForm}>{i18n.t('contact.sendAnother')}</button>
          </div>
        {:else}
          <form class="contact-form" onsubmit={handleSubmit} novalidate>
            <div class="form-group">
              <label for="name" class="form-label">{i18n.t('contact.name')} <span class="required">*</span></label>
              <input
                type="text"
                id="name"
                class="input"
                class:input-error={errors.name}
                bind:value={name}
                placeholder={i18n.t('contact.namePlaceholder')}
              />
              {#if errors.name}
                <span class="error-text">{errors.name}</span>
              {/if}
            </div>

            <div class="form-group">
              <label for="email" class="form-label">{i18n.t('contact.email')} <span class="required">*</span></label>
              <input
                type="email"
                id="email"
                class="input"
                class:input-error={errors.email}
                bind:value={email}
                placeholder="your@email.com"
              />
              {#if errors.email}
                <span class="error-text">{errors.email}</span>
              {/if}
            </div>

            <div class="form-group">
              <label for="subject" class="form-label">{i18n.t('contact.subject')} <span class="required">*</span></label>
              <select
                id="subject"
                class="input"
                class:input-error={errors.subject}
                bind:value={subject}
              >
                {#each subjectOptions as option (option.value)}
                  <option value={option.value} disabled={option.value === ''}>{option.label}</option>
                {/each}
              </select>
              {#if errors.subject}
                <span class="error-text">{errors.subject}</span>
              {/if}
            </div>

            <div class="form-group">
              <label for="message" class="form-label">{i18n.t('contact.message')} <span class="required">*</span></label>
              <textarea
                id="message"
                class="input"
                class:input-error={errors.message}
                bind:value={message}
                placeholder={i18n.t('contact.messagePlaceholder')}
                rows="5"
              ></textarea>
              {#if errors.message}
                <span class="error-text">{errors.message}</span>
              {/if}
            </div>

            <button type="submit" class="btn btn-primary btn-lg submit-btn" disabled={isSubmitting}>
              {#if isSubmitting}
                {i18n.t('contact.sending')}
              {:else}
                {i18n.t('contact.send')}
              {/if}
            </button>
          </form>
        {/if}
      </div>

      <!-- Contact Info Sidebar -->
      <aside class="info-sidebar">
        <div class="info-card">
          <h3>{i18n.t('contact.contactInfo')}</h3>

          <div class="info-items">
            <div class="info-item">
              <span class="info-icon">📧</span>
              <div>
                <p class="info-label">Email</p>
                <a href="mailto:support@marinelacespace.com" class="info-value">support@marinelacespace.com</a>
              </div>
            </div>

            <div class="info-item">
              <span class="info-icon">📞</span>
              <div>
                <p class="info-label">{i18n.t('contact.phone')}</p>
                <a href="tel:+380501234567" class="info-value">+38 (050) 123-45-67</a>
              </div>
            </div>

            <div class="info-item">
              <span class="info-icon">🕐</span>
              <div>
                <p class="info-label">{i18n.t('contact.workingHours')}</p>
                <p class="info-value">{i18n.t('contact.workingHoursValue')}</p>
                <p class="info-value">{i18n.t('contact.workingHoursSat')}</p>
                <p class="info-value text-muted">{i18n.t('contact.workingHoursSun')}</p>
              </div>
            </div>

            <div class="info-item">
              <span class="info-icon">📍</span>
              <div>
                <p class="info-label">{i18n.t('contact.address')}</p>
                <p class="info-value">{i18n.t('contact.addressValue')}</p>
              </div>
            </div>
          </div>
        </div>

        <div class="info-card">
          <h3>{i18n.t('contact.socialTitle')}</h3>
          <div class="social-links">
            <a href="https://www.instagram.com/marinelacespace" target="_blank" rel="noopener noreferrer" class="social-link">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zM12 0C8.741 0 8.333.014 7.053.072 2.695.272.273 2.69.073 7.052.014 8.333 0 8.741 0 12c0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98C8.333 23.986 8.741 24 12 24c3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98C15.668.014 15.259 0 12 0zm0 5.838a6.162 6.162 0 100 12.324 6.162 6.162 0 000-12.324zM12 16a4 4 0 110-8 4 4 0 010 8zm6.406-11.845a1.44 1.44 0 100 2.881 1.44 1.44 0 000-2.881z"/></svg>
              <span>Instagram</span>
            </a>
            <a href="https://www.etsy.com/shop/MarineLaceSpace" target="_blank" rel="noopener noreferrer" class="social-link">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M9.16 4.945c0-.04.02-.08.06-.1C9.42 4.7 10.24 4.5 11.5 4.5c2.04 0 3.22.44 3.22.44.18.08.28.26.28.46v2.6c0 .12-.06.24-.16.3-.1.08-.22.1-.34.08-.42-.1-1.26-.28-2.38-.28-1.56 0-2.3.52-2.3 1.24 0 .86.82 1.18 2.52 1.86 2.3.92 3.52 1.82 3.52 3.92 0 2.72-1.88 4.38-5.12 4.38-2 0-3.36-.44-3.66-.56-.14-.06-.24-.2-.24-.36v-2.78c0-.14.08-.26.2-.32.12-.06.26-.06.36.02.52.3 1.58.72 3.14.72 1.28 0 2.18-.52 2.18-1.42 0-.88-.66-1.28-2.42-2-2.36-.96-3.62-1.88-3.62-3.86 0-2.4 1.82-4.14 4.92-4.14 1.98 0 3.08.38 3.38.5.16.06.26.22.26.38v2.56c0 .14-.08.26-.2.32-.12.06-.26.06-.38 0-.44-.22-1.42-.56-2.82-.56-1.14 0-2.02.44-2.02 1.16z"/></svg>
              <span>Etsy</span>
            </a>
            <a href="https://www.facebook.com/marinelacespace" target="_blank" rel="noopener noreferrer" class="social-link">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/></svg>
              <span>Facebook</span>
            </a>
            <a href="https://t.me/marinelacespace" target="_blank" rel="noopener noreferrer" class="social-link">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor"><path d="M11.944 0A12 12 0 000 12a12 12 0 0012 12 12 12 0 0012-12A12 12 0 0012 0 12 12 0 0011.944 0zm4.962 7.224c.1-.002.321.023.465.14a.506.506 0 01.171.325c.016.093.036.306.02.472-.18 1.898-.962 6.502-1.36 8.627-.168.9-.499 1.201-.82 1.23-.696.065-1.225-.46-1.9-.902-1.056-.693-1.653-1.124-2.678-1.8-1.185-.78-.417-1.21.258-1.91.177-.184 3.247-2.977 3.307-3.23.007-.032.014-.15-.056-.212s-.174-.041-.249-.024c-.106.024-1.793 1.14-5.061 3.345-.479.33-.913.49-1.302.48-.428-.008-1.252-.241-1.865-.44-.752-.245-1.349-.374-1.297-.789.027-.216.325-.437.893-.663 3.498-1.524 5.83-2.529 6.998-3.014 3.332-1.386 4.025-1.627 4.476-1.635z"/></svg>
              <span>Telegram</span>
            </a>
          </div>
        </div>

        <div class="map-placeholder">
          <div class="map-inner">
            <span class="map-pin">📍</span>
            <p>{i18n.t('contact.addressValue')}</p>
            <p class="text-muted">{i18n.t('contact.showroomNote')}</p>
          </div>
        </div>
      </aside>
    </div>
  </div>
</div>

<style>
  .contact-page {
    max-width: 1060px;
    margin: 0 auto;
    padding-bottom: var(--space-16);
  }

  .contact-header {
    text-align: center;
    margin-bottom: var(--space-12);
  }

  .contact-header h1 {
    font-family: var(--font-display);
    font-size: 2.25rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .contact-header p {
    font-size: 1.0625rem;
    color: var(--color-text-muted);
    max-width: 520px;
    margin: 0 auto;
  }

  /* ─── Layout ────────────────────────────────── */

  .contact-layout {
    display: grid;
    grid-template-columns: 1fr 360px;
    gap: var(--space-10);
    align-items: start;
  }

  /* ─── Form ──────────────────────────────────── */

  .form-section {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    padding: var(--space-8);
  }

  .contact-form {
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .form-label {
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
  }

  .required {
    color: var(--color-error);
  }

  .input-error {
    border-color: var(--color-error) !important;
    box-shadow: 0 0 0 3px rgba(196, 85, 90, 0.1);
  }

  .error-text {
    font-size: 0.8125rem;
    color: var(--color-error);
  }

  .submit-btn {
    margin-top: var(--space-2);
    width: 100%;
  }

  /* ─── Success ───────────────────────────────── */

  .success-message {
    text-align: center;
    padding: var(--space-8) var(--space-4);
  }

  .success-icon {
    width: 64px;
    height: 64px;
    margin: 0 auto var(--space-6);
    border-radius: 50%;
    background-color: var(--color-success);
    color: #fff;
    font-size: 1.75rem;
    font-weight: 700;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .success-message h2 {
    font-family: var(--font-display);
    font-size: 1.5rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .success-message p {
    font-size: 0.9375rem;
    color: var(--color-text-light);
    line-height: 1.7;
    margin-bottom: var(--space-6);
  }

  /* ─── Info Sidebar ──────────────────────────── */

  .info-sidebar {
    display: flex;
    flex-direction: column;
    gap: var(--space-6);
  }

  .info-card {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    padding: var(--space-6);
  }

  .info-card h3 {
    font-family: var(--font-display);
    font-size: 1.125rem;
    color: var(--color-accent);
    margin-bottom: var(--space-5);
    padding-bottom: var(--space-3);
    border-bottom: 2px solid var(--color-secondary-light);
  }

  .info-items {
    display: flex;
    flex-direction: column;
    gap: var(--space-5);
  }

  .info-item {
    display: flex;
    gap: var(--space-3);
  }

  .info-icon {
    font-size: 1.25rem;
    flex-shrink: 0;
    margin-top: 2px;
  }

  .info-label {
    font-size: 0.75rem;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-text-muted);
    margin-bottom: 2px;
  }

  .info-value {
    font-size: 0.875rem;
    color: var(--color-text);
    line-height: 1.6;
  }

  a.info-value {
    color: var(--color-primary);
    text-decoration: underline;
    text-underline-offset: 2px;
  }

  a.info-value:hover {
    color: var(--color-primary-dark);
  }

  .text-muted {
    color: var(--color-text-muted) !important;
    font-style: italic;
  }

  /* ─── Socials ───────────────────────────────── */

  .social-links {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .social-link {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--color-text);
    transition: all var(--transition-fast);
  }

  .social-link:hover {
    border-color: var(--color-primary-light);
    background-color: var(--color-surface-hover);
    color: var(--color-primary);
  }

  .social-link svg {
    flex-shrink: 0;
  }

  /* ─── Map ────────────────────────────────────── */

  .map-placeholder {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    overflow: hidden;
  }

  .map-inner {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: var(--space-8);
    background: linear-gradient(135deg, var(--color-surface-hover) 0%, var(--color-background) 100%);
    text-align: center;
    gap: var(--space-2);
  }

  .map-pin {
    font-size: 2rem;
  }

  .map-inner p {
    font-size: 0.875rem;
    color: var(--color-text-light);
  }

  /* ─── Responsive ─────────────────────────────── */

  @media (max-width: 860px) {
    .contact-layout {
      grid-template-columns: 1fr;
    }

    .info-sidebar {
      order: -1;
    }
  }

  @media (max-width: 640px) {
    .contact-header h1 {
      font-size: 1.75rem;
    }

    .form-section {
      padding: var(--space-5);
    }
  }
</style>
