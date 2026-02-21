<script lang="ts">
  import { page } from '$app/stores';
  import Breadcrumb from '$components/Breadcrumb.svelte';

  const breadcrumbs = [
    { label: 'Головна', href: '/' },
    { label: 'Замовлення оформлено' }
  ];

  const orderId = $derived($page.url.searchParams.get('orderId') ?? '');
</script>

<svelte:head>
  <title>Замовлення оформлено — MarineLaceSpace</title>
</svelte:head>

<section class="success-page container" aria-label="Замовлення оформлено">
  <Breadcrumb items={breadcrumbs} />

  <div class="success-content">
    <div class="success-icon-wrap" aria-hidden="true">
      <div class="success-circle">
        <svg class="checkmark" viewBox="0 0 52 52" width="52" height="52" fill="none" aria-hidden="true">
          <circle class="checkmark-circle" cx="26" cy="26" r="24" stroke="currentColor" stroke-width="2.5" />
          <path class="checkmark-check" d="M14.1 27.2l7.1 7.2 16.7-16.8" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
      </div>
    </div>

    <h1 class="success-title">Замовлення оформлено!</h1>

    {#if orderId}
      <p class="order-number">Номер замовлення: <strong>{orderId}</strong></p>
    {/if}

    <p class="success-description">
      Ми надіслали підтвердження на ваш email.<br />
      Ви можете відстежувати статус замовлення у вашому акаунті.
    </p>

    <div class="success-actions">
      {#if orderId}
        <a href="/account/orders/{orderId}" class="btn btn-primary btn-lg">
          Переглянути замовлення
        </a>
      {/if}
      <a href="/catalog" class="btn btn-outline btn-lg">
        Продовжити покупки
      </a>
    </div>

    <div class="success-extras">
      <p class="success-trust">🔒 Безпечна оплата • Повернення протягом 14 днів • Безкоштовна доставка від 2000₴</p>
    </div>
  </div>
</section>

<style>
  .success-page {
    padding-block: var(--space-4) var(--space-16);
  }

  .success-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    max-width: 560px;
    margin-inline: auto;
    padding-block: var(--space-10);
  }

  /* ── Animated Checkmark ── */
  .success-icon-wrap {
    margin-bottom: var(--space-8);
  }

  .success-circle {
    width: 96px;
    height: 96px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-full);
    background-color: rgba(74, 139, 110, 0.08);
    animation: scaleInBounce 600ms ease both;
  }

  .checkmark {
    color: var(--color-success);
  }

  .checkmark-circle {
    stroke-dasharray: 166;
    stroke-dashoffset: 166;
    animation: checkCircle 600ms 200ms ease-out forwards;
  }

  .checkmark-check {
    stroke-dasharray: 48;
    stroke-dashoffset: 48;
    animation: checkStroke 400ms 600ms ease-out forwards;
  }

  @keyframes scaleInBounce {
    0% {
      transform: scale(0);
      opacity: 0;
    }
    60% {
      transform: scale(1.1);
    }
    100% {
      transform: scale(1);
      opacity: 1;
    }
  }

  @keyframes checkCircle {
    to {
      stroke-dashoffset: 0;
    }
  }

  @keyframes checkStroke {
    to {
      stroke-dashoffset: 0;
    }
  }

  /* ── Text ── */
  .success-title {
    font-family: var(--font-display);
    font-size: 2rem;
    font-weight: 700;
    color: var(--color-text);
    margin-bottom: var(--space-4);
    animation: fadeIn var(--transition-base) 400ms both;
  }

  .order-number {
    font-size: 1rem;
    color: var(--color-text-light);
    margin-bottom: var(--space-4);
    animation: fadeIn var(--transition-base) 500ms both;
  }

  .order-number strong {
    color: var(--color-text);
    font-weight: 600;
    font-family: monospace;
    letter-spacing: 0.03em;
  }

  .success-description {
    font-size: 1rem;
    color: var(--color-text-light);
    line-height: 1.7;
    margin-bottom: var(--space-8);
    animation: fadeIn var(--transition-base) 600ms both;
  }

  /* ── Actions ── */
  .success-actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    width: 100%;
    max-width: 320px;
    animation: fadeIn var(--transition-base) 700ms both;
  }

  .success-actions .btn {
    text-decoration: none;
    width: 100%;
  }

  /* ── Extras ── */
  .success-extras {
    margin-top: var(--space-10);
    animation: fadeIn var(--transition-base) 800ms both;
  }

  .success-trust {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  /* ── Responsive ── */
  @media (min-width: 640px) {
    .success-title {
      font-size: 2.5rem;
    }

    .success-actions {
      flex-direction: row;
      max-width: none;
    }

    .success-actions .btn {
      width: auto;
    }
  }
</style>
