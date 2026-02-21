<script lang="ts">
  import { i18n } from '$i18n/index.svelte';

  interface FaqItem {
    question: string;
    answer: string;
  }

  interface FaqCategory {
    title: string;
    icon: string;
    items: FaqItem[];
  }

  let faqCategories: FaqCategory[] = $derived([
    {
      title: i18n.t('faq.orders'),
      icon: '🛒',
      items: [
        { question: i18n.t('faq.ordersQ1'), answer: i18n.t('faq.ordersA1') },
        { question: i18n.t('faq.ordersQ2'), answer: i18n.t('faq.ordersA2') },
        { question: i18n.t('faq.ordersQ3'), answer: i18n.t('faq.ordersA3') },
        { question: i18n.t('faq.ordersQ4'), answer: i18n.t('faq.ordersA4') },
      ],
    },
    {
      title: i18n.t('faq.shipping'),
      icon: '📦',
      items: [
        { question: i18n.t('faq.shippingQ1'), answer: i18n.t('faq.shippingA1') },
        { question: i18n.t('faq.shippingQ2'), answer: i18n.t('faq.shippingA2') },
        { question: i18n.t('faq.shippingQ3'), answer: i18n.t('faq.shippingA3') },
        { question: i18n.t('faq.shippingQ4'), answer: i18n.t('faq.shippingA4') },
      ],
    },
    {
      title: i18n.t('faq.payment'),
      icon: '💳',
      items: [
        { question: i18n.t('faq.paymentQ1'), answer: i18n.t('faq.paymentA1') },
        { question: i18n.t('faq.paymentQ2'), answer: i18n.t('faq.paymentA2') },
        { question: i18n.t('faq.paymentQ3'), answer: i18n.t('faq.paymentA3') },
        { question: i18n.t('faq.paymentQ4'), answer: i18n.t('faq.paymentA4') },
      ],
    },
    {
      title: i18n.t('faq.returns'),
      icon: '↩️',
      items: [
        { question: i18n.t('faq.returnsQ1'), answer: i18n.t('faq.returnsA1') },
        { question: i18n.t('faq.returnsQ2'), answer: i18n.t('faq.returnsA2') },
        { question: i18n.t('faq.returnsQ3'), answer: i18n.t('faq.returnsA3') },
        { question: i18n.t('faq.returnsQ4'), answer: i18n.t('faq.returnsA4') },
      ],
    },
    {
      title: i18n.t('faq.sizesTitle'),
      icon: '📏',
      items: [
        { question: i18n.t('faq.sizesQ1'), answer: i18n.t('faq.sizesA1') },
        { question: i18n.t('faq.sizesQ2'), answer: i18n.t('faq.sizesA2') },
        { question: i18n.t('faq.sizesQ3'), answer: i18n.t('faq.sizesA3') },
        { question: i18n.t('faq.sizesQ4'), answer: i18n.t('faq.sizesA4') },
      ],
    },
    {
      title: i18n.t('faq.careTitle'),
      icon: '🧼',
      items: [
        { question: i18n.t('faq.careQ1'), answer: i18n.t('faq.careA1') },
        { question: i18n.t('faq.careQ2'), answer: i18n.t('faq.careA2') },
        { question: i18n.t('faq.careQ3'), answer: i18n.t('faq.careA3') },
        { question: i18n.t('faq.careQ4'), answer: i18n.t('faq.careA4') },
      ],
    },
  ]);

  let openItems = $state<Record<string, boolean>>({});

  function toggleItem(categoryIndex: number, itemIndex: number) {
    const key = `${categoryIndex}-${itemIndex}`;
    openItems = { ...openItems, [key]: !openItems[key] };
  }

  function isOpen(categoryIndex: number, itemIndex: number): boolean {
    return !!openItems[`${categoryIndex}-${itemIndex}`];
  }
</script>

<svelte:head>
  <title>{i18n.t('faq.pageTitle')}</title>
  <meta name="description" content={i18n.t('faq.metaDescription')} />
</svelte:head>

<div class="container">
  <div class="faq-page">
    <header class="faq-header">
      <h1>{i18n.t('faq.title')}</h1>
      <p>{i18n.t('faq.subtitle')}</p>
    </header>

    <div class="faq-categories">
      {#each faqCategories as category, ci (ci)}
        <section class="faq-category">
          <div class="category-header">
            <span class="category-icon">{category.icon}</span>
            <h2>{category.title}</h2>
          </div>

          <div class="accordion">
            {#each category.items as item, ii (ii)}
              <div class="accordion-item" class:is-open={isOpen(ci, ii)}>
                <button
                  class="accordion-trigger"
                  onclick={() => toggleItem(ci, ii)}
                  aria-expanded={isOpen(ci, ii)}
                >
                  <span class="accordion-question">{item.question}</span>
                  <span class="accordion-icon" aria-hidden="true">
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                      <path
                        d="M5 7.5L10 12.5L15 7.5"
                        stroke="currentColor"
                        stroke-width="1.5"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                      />
                    </svg>
                  </span>
                </button>
                <div class="accordion-content">
                  <div class="accordion-body">
                    <p>{item.answer}</p>
                  </div>
                </div>
              </div>
            {/each}
          </div>
        </section>
      {/each}
    </div>

    <section class="faq-contact">
      <div class="faq-contact-inner">
        <h2>{i18n.t('faq.noAnswer')}</h2>
        <p>{i18n.t('faq.noAnswerText')}</p>
        <div class="faq-contact-actions">
          <a href="/contact" class="btn btn-primary">{i18n.t('faq.writeUs')}</a>
          <a href="tel:+380501234567" class="btn btn-outline">{i18n.t('faq.callUs')}</a>
        </div>
      </div>
    </section>
  </div>
</div>

<style>
  .faq-page {
    max-width: 860px;
    margin: 0 auto;
    padding-bottom: var(--space-16);
  }

  .faq-header {
    text-align: center;
    margin-bottom: var(--space-12);
  }

  .faq-header h1 {
    font-family: var(--font-display);
    font-size: 2.25rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .faq-header p {
    font-size: 1.0625rem;
    color: var(--color-text-muted);
    max-width: 480px;
    margin: 0 auto;
  }

  /* ─── Category ─────────────────────────────────── */

  .faq-categories {
    display: flex;
    flex-direction: column;
    gap: var(--space-10);
  }

  .category-header {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-bottom: var(--space-5);
    padding-bottom: var(--space-3);
    border-bottom: 2px solid var(--color-secondary-light);
  }

  .category-icon {
    font-size: 1.5rem;
    line-height: 1;
  }

  .category-header h2 {
    font-family: var(--font-display);
    font-size: 1.375rem;
    color: var(--color-accent);
  }

  /* ─── Accordion ────────────────────────────────── */

  .accordion {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }

  .accordion-item {
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-lg);
    overflow: hidden;
    transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
  }

  .accordion-item:hover {
    border-color: var(--color-border);
  }

  .accordion-item.is-open {
    border-color: var(--color-primary-light);
    box-shadow: var(--shadow-sm);
  }

  .accordion-trigger {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    padding: var(--space-5) var(--space-6);
    background: none;
    border: none;
    cursor: pointer;
    text-align: left;
    gap: var(--space-4);
    transition: background-color var(--transition-fast);
  }

  .accordion-trigger:hover {
    background-color: var(--color-surface-hover);
  }

  .accordion-question {
    font-family: var(--font-body);
    font-size: 0.9375rem;
    font-weight: 500;
    color: var(--color-text);
    line-height: 1.5;
  }

  .accordion-icon {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    color: var(--color-text-muted);
    transition: transform var(--transition-base), color var(--transition-fast);
  }

  .is-open .accordion-icon {
    transform: rotate(180deg);
    color: var(--color-primary);
  }

  .accordion-content {
    display: grid;
    grid-template-rows: 0fr;
    transition: grid-template-rows var(--transition-base);
  }

  .is-open .accordion-content {
    grid-template-rows: 1fr;
  }

  .accordion-body {
    overflow: hidden;
  }

  .accordion-body p {
    padding: 0 var(--space-6) var(--space-5);
    font-size: 0.9375rem;
    line-height: 1.75;
    color: var(--color-text-light);
  }

  /* ─── Contact CTA ──────────────────────────────── */

  .faq-contact {
    margin-top: var(--space-14);
  }

  .faq-contact-inner {
    text-align: center;
    background-color: var(--color-surface);
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-xl);
    padding: var(--space-12) var(--space-8);
  }

  .faq-contact-inner h2 {
    font-family: var(--font-display);
    font-size: 1.5rem;
    color: var(--color-primary);
    margin-bottom: var(--space-3);
  }

  .faq-contact-inner p {
    font-size: 0.9375rem;
    color: var(--color-text-muted);
    margin-bottom: var(--space-6);
  }

  .faq-contact-actions {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-4);
    flex-wrap: wrap;
  }

  @media (max-width: 640px) {
    .faq-header h1 {
      font-size: 1.75rem;
    }

    .accordion-trigger {
      padding: var(--space-4) var(--space-4);
    }

    .accordion-body p {
      padding: 0 var(--space-4) var(--space-4);
    }

    .faq-contact-inner {
      padding: var(--space-8) var(--space-4);
    }
  }
</style>
