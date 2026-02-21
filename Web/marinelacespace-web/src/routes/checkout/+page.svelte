<script lang="ts">
  import { goto } from '$app/navigation';
  import type { ShippingAddress, CheckoutRequest, BasketItem } from '$types';
  import { basketStore } from '$stores/basket';
  import { notificationStore } from '$stores/notification';
  import Breadcrumb from '$components/Breadcrumb.svelte';
  import PriceDisplay from '$components/PriceDisplay.svelte';
  import LoadingSpinner from '$components/LoadingSpinner.svelte';

  const breadcrumbs = [
    { label: 'Головна', href: '/' },
    { label: 'Кошик', href: '/basket' },
    { label: 'Оформлення' }
  ];

  /* ── Steps ── */
  const STEPS = [
    { num: 1, label: 'Доставка' },
    { num: 2, label: 'Оплата' },
    { num: 3, label: 'Підтвердження' }
  ] as const;

  let currentStep = $state(1);

  /* ── Shipping Form ── */
  let shipping = $state<ShippingAddress>({
    fullName: '',
    phone: '',
    country: 'Україна',
    city: '',
    addressLine1: '',
    addressLine2: '',
    state: '',
    postalCode: ''
  });

  let shippingErrors = $state<Partial<Record<keyof ShippingAddress, string>>>({});
  let shippingTouched = $state<Partial<Record<keyof ShippingAddress, boolean>>>({});

  function validateShipping(): boolean {
    const errs: Partial<Record<keyof ShippingAddress, string>> = {};
    if (!shipping.fullName.trim()) errs.fullName = "Ім'я обов'язкове";
    if (!shipping.phone.trim()) errs.phone = "Телефон обов'язковий";
    if (!shipping.country.trim()) errs.country = "Країна обов'язкова";
    if (!shipping.city.trim()) errs.city = "Місто обов'язкове";
    if (!shipping.addressLine1.trim()) errs.addressLine1 = "Адреса обов'язкова";
    if (!shipping.postalCode.trim()) errs.postalCode = "Поштовий індекс обов'язковий";
    if (!shipping.state.trim()) errs.state = "Область обов'язкова";
    shippingErrors = errs;
    return Object.keys(errs).length === 0;
  }

  /* ── Payment ── */
  type PaymentOption = { id: string; icon: string; label: string; recommended?: boolean };
  const paymentOptions: PaymentOption[] = [
    { id: 'Stripe', icon: '💳', label: 'Банківська картка (Stripe)', recommended: true },
    { id: 'LiqPay', icon: '🏦', label: 'LiqPay' },
    { id: 'PayPal', icon: '💰', label: 'PayPal' }
  ];

  let selectedPayment = $state('Stripe');

  /* ── Terms ── */
  let termsAccepted = $state(false);
  let termsError = $state('');

  /* ── Derived ── */
  const items = $derived(basketStore.basket?.items ?? []);
  const subtotal = $derived(items.reduce((s, i) => s + i.unitPrice * i.quantity, 0));
  const FREE_SHIPPING_THRESHOLD = 2000;
  const SHIPPING_COST = 150;
  const shippingCost = $derived(subtotal >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_COST);
  const total = $derived(subtotal + shippingCost);

  $effect(() => {
    if (!basketStore.basket) basketStore.loadBasket();
  });

  /* ── Navigation ── */
  function nextStep() {
    if (currentStep === 1) {
      // Mark all fields touched
      shippingTouched = {
        fullName: true, phone: true, country: true, city: true,
        addressLine1: true, postalCode: true, state: true
      };
      if (!validateShipping()) return;
    }
    if (currentStep < 3) currentStep++;
  }

  function prevStep() {
    if (currentStep > 1) currentStep--;
  }

  function goToStep(step: number) {
    if (step < currentStep) currentStep = step;
  }

  /* ── Submit ── */
  let isSubmitting = $state(false);

  async function handleSubmit() {
    if (!termsAccepted) {
      termsError = 'Необхідно погодитись з умовами';
      return;
    }
    termsError = '';
    isSubmitting = true;

    const req: CheckoutRequest = {
      shippingAddress: { ...shipping },
      paymentMethod: selectedPayment
    };

    try {
      const order = await basketStore.checkout(req);
      notificationStore.success('Замовлення оформлено!');
      goto(`/checkout/success?orderId=${order.id}`);
    } catch {
      notificationStore.error('Помилка при оформленні замовлення. Спробуйте ще раз.');
    } finally {
      isSubmitting = false;
    }
  }

  function formatVariant(item: BasketItem): string {
    const parts: string[] = [];
    if (item.sizeName) parts.push(item.sizeName);
    if (item.colorName) parts.push(item.colorName);
    if (item.materialName) parts.push(item.materialName);
    return parts.join(' / ');
  }
</script>

<svelte:head>
  <title>Оформлення замовлення — MarineLaceSpace</title>
</svelte:head>

<section class="checkout-page container" aria-label="Оформлення замовлення">
  <Breadcrumb items={breadcrumbs} />

  {#if basketStore.isLoading && !basketStore.basket}
    <LoadingSpinner size="lg" message="Завантаження…" />
  {:else if items.length === 0}
    <div class="checkout-empty">
      <p>Кошик порожній. <a href="/catalog" class="link-primary">Перейти до каталогу</a></p>
    </div>
  {:else}
    <!-- Stepper -->
    <nav class="stepper" aria-label="Кроки оформлення">
      <ol class="stepper-list">
        {#each STEPS as step (step.num)}
          <li class="stepper-item" class:active={currentStep === step.num} class:completed={currentStep > step.num}>
            <button
              class="stepper-btn"
              onclick={() => goToStep(step.num)}
              disabled={step.num > currentStep}
              aria-current={currentStep === step.num ? 'step' : undefined}
            >
              <span class="stepper-circle">
                {#if currentStep > step.num}
                  <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="3" aria-hidden="true">
                    <polyline points="20 6 9 17 4 12" />
                  </svg>
                {:else}
                  {step.num}
                {/if}
              </span>
              <span class="stepper-label">{step.label}</span>
            </button>
            {#if step.num < STEPS.length}
              <div class="stepper-line" class:completed={currentStep > step.num}></div>
            {/if}
          </li>
        {/each}
      </ol>
    </nav>

    <div class="checkout-layout">
      <div class="checkout-main">
        <!-- Step 1: Shipping -->
        {#if currentStep === 1}
          <div class="step-panel animate-fade-in" role="group" aria-label="Адреса доставки">
            <h2 class="step-heading">Адреса доставки</h2>

            <form class="shipping-form" onsubmit={(e) => { e.preventDefault(); nextStep(); }} novalidate>
              <div class="form-grid">
                <div class="form-field full-width">
                  <label for="fullName" class="form-label">Повне ім'я <span class="required">*</span></label>
                  <input
                    id="fullName"
                    type="text"
                    class="input"
                    class:input-error={shippingTouched.fullName && shippingErrors.fullName}
                    bind:value={shipping.fullName}
                    onblur={() => { shippingTouched.fullName = true; validateShipping(); }}
                    placeholder="Іваненко Марія Олексіївна"
                    autocomplete="name"
                    required
                  />
                  {#if shippingTouched.fullName && shippingErrors.fullName}
                    <p class="field-error" role="alert">{shippingErrors.fullName}</p>
                  {/if}
                </div>

                <div class="form-field full-width">
                  <label for="phone" class="form-label">Телефон <span class="required">*</span></label>
                  <input
                    id="phone"
                    type="tel"
                    class="input"
                    class:input-error={shippingTouched.phone && shippingErrors.phone}
                    bind:value={shipping.phone}
                    onblur={() => { shippingTouched.phone = true; validateShipping(); }}
                    placeholder="+380 XX XXX XX XX"
                    autocomplete="tel"
                    required
                  />
                  {#if shippingTouched.phone && shippingErrors.phone}
                    <p class="field-error" role="alert">{shippingErrors.phone}</p>
                  {/if}
                </div>

                <div class="form-field">
                  <label for="country" class="form-label">Країна <span class="required">*</span></label>
                  <select
                    id="country"
                    class="input"
                    bind:value={shipping.country}
                  >
                    <option value="Україна">Україна</option>
                    <option value="Польща">Польща</option>
                    <option value="Німеччина">Німеччина</option>
                    <option value="Чехія">Чехія</option>
                    <option value="Інше">Інше</option>
                  </select>
                </div>

                <div class="form-field">
                  <label for="city" class="form-label">Місто <span class="required">*</span></label>
                  <input
                    id="city"
                    type="text"
                    class="input"
                    class:input-error={shippingTouched.city && shippingErrors.city}
                    bind:value={shipping.city}
                    onblur={() => { shippingTouched.city = true; validateShipping(); }}
                    placeholder="Київ"
                    autocomplete="address-level2"
                    required
                  />
                  {#if shippingTouched.city && shippingErrors.city}
                    <p class="field-error" role="alert">{shippingErrors.city}</p>
                  {/if}
                </div>

                <div class="form-field full-width">
                  <label for="addressLine1" class="form-label">Адреса <span class="required">*</span></label>
                  <input
                    id="addressLine1"
                    type="text"
                    class="input"
                    class:input-error={shippingTouched.addressLine1 && shippingErrors.addressLine1}
                    bind:value={shipping.addressLine1}
                    onblur={() => { shippingTouched.addressLine1 = true; validateShipping(); }}
                    placeholder="вул. Хрещатик, 1, кв. 10"
                    autocomplete="address-line1"
                    required
                  />
                  {#if shippingTouched.addressLine1 && shippingErrors.addressLine1}
                    <p class="field-error" role="alert">{shippingErrors.addressLine1}</p>
                  {/if}
                </div>

                <div class="form-field full-width">
                  <label for="addressLine2" class="form-label">Адреса (додатково)</label>
                  <input
                    id="addressLine2"
                    type="text"
                    class="input"
                    bind:value={shipping.addressLine2}
                    placeholder="Поверх, під'їзд тощо"
                    autocomplete="address-line2"
                  />
                </div>

                <div class="form-field">
                  <label for="postalCode" class="form-label">Поштовий індекс <span class="required">*</span></label>
                  <input
                    id="postalCode"
                    type="text"
                    class="input"
                    class:input-error={shippingTouched.postalCode && shippingErrors.postalCode}
                    bind:value={shipping.postalCode}
                    onblur={() => { shippingTouched.postalCode = true; validateShipping(); }}
                    placeholder="01001"
                    autocomplete="postal-code"
                    required
                  />
                  {#if shippingTouched.postalCode && shippingErrors.postalCode}
                    <p class="field-error" role="alert">{shippingErrors.postalCode}</p>
                  {/if}
                </div>

                <div class="form-field">
                  <label for="state" class="form-label">Область <span class="required">*</span></label>
                  <input
                    id="state"
                    type="text"
                    class="input"
                    class:input-error={shippingTouched.state && shippingErrors.state}
                    bind:value={shipping.state}
                    onblur={() => { shippingTouched.state = true; validateShipping(); }}
                    placeholder="Київська"
                    autocomplete="address-level1"
                    required
                  />
                  {#if shippingTouched.state && shippingErrors.state}
                    <p class="field-error" role="alert">{shippingErrors.state}</p>
                  {/if}
                </div>
              </div>

              <div class="step-nav">
                <span></span>
                <button type="submit" class="btn btn-primary btn-lg">Далі</button>
              </div>
            </form>
          </div>
        {/if}

        <!-- Step 2: Payment -->
        {#if currentStep === 2}
          <div class="step-panel animate-fade-in" role="group" aria-label="Спосіб оплати">
            <h2 class="step-heading">Спосіб оплати</h2>

            <div class="payment-options" role="radiogroup" aria-label="Оберіть спосіб оплати">
              {#each paymentOptions as option (option.id)}
                <label
                  class="payment-card card"
                  class:selected={selectedPayment === option.id}
                >
                  <input
                    type="radio"
                    name="paymentMethod"
                    value={option.id}
                    bind:group={selectedPayment}
                    class="sr-only"
                  />
                  <span class="payment-icon" aria-hidden="true">{option.icon}</span>
                  <span class="payment-label">
                    {option.label}
                    {#if option.recommended}
                      <span class="badge badge-primary recommended-badge">Рекомендовано</span>
                    {/if}
                  </span>
                  <span class="payment-check" aria-hidden="true">
                    {#if selectedPayment === option.id}
                      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5">
                        <circle cx="12" cy="12" r="10" />
                        <polyline points="16 9 10.5 15 8 12.5" />
                      </svg>
                    {:else}
                      <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.5">
                        <circle cx="12" cy="12" r="10" />
                      </svg>
                    {/if}
                  </span>
                </label>
              {/each}
            </div>

            <div class="step-nav">
              <button type="button" class="btn btn-outline" onclick={prevStep}>Назад</button>
              <button type="button" class="btn btn-primary btn-lg" onclick={nextStep}>Далі</button>
            </div>
          </div>
        {/if}

        <!-- Step 3: Confirmation -->
        {#if currentStep === 3}
          <div class="step-panel animate-fade-in" role="group" aria-label="Підтвердження замовлення">
            <h2 class="step-heading">Перевірте замовлення</h2>

            <!-- Shipping Summary -->
            <div class="confirm-section card">
              <div class="card-body">
                <div class="confirm-section-header">
                  <h3 class="confirm-section-title">📦 Доставка</h3>
                  <button class="edit-link" onclick={() => goToStep(1)}>Змінити</button>
                </div>
                <address class="shipping-summary">
                  <p class="summary-name">{shipping.fullName}</p>
                  <p>{shipping.addressLine1}{shipping.addressLine2 ? `, ${shipping.addressLine2}` : ''}</p>
                  <p>{shipping.city}, {shipping.state} {shipping.postalCode}</p>
                  <p>{shipping.country}</p>
                  <p class="summary-phone">{shipping.phone}</p>
                </address>
              </div>
            </div>

            <!-- Payment Summary -->
            <div class="confirm-section card">
              <div class="card-body">
                <div class="confirm-section-header">
                  <h3 class="confirm-section-title">💳 Оплата</h3>
                  <button class="edit-link" onclick={() => goToStep(2)}>Змінити</button>
                </div>
                <p>{paymentOptions.find(o => o.id === selectedPayment)?.icon} {paymentOptions.find(o => o.id === selectedPayment)?.label}</p>
              </div>
            </div>

            <!-- Items Summary -->
            <div class="confirm-section card">
              <div class="card-body">
                <h3 class="confirm-section-title">🛍️ Товари</h3>
                <ul class="confirm-items-list">
                  {#each items as item (item.itemId)}
                    <li class="confirm-item">
                      <div class="confirm-item-info">
                        <span class="confirm-item-name">{item.productName}</span>
                        {#if formatVariant(item)}
                          <span class="confirm-item-variant">{formatVariant(item)}</span>
                        {/if}
                        <span class="confirm-item-qty">× {item.quantity}</span>
                      </div>
                      <span class="confirm-item-price">
                        <PriceDisplay price={item.unitPrice * item.quantity} />
                      </span>
                    </li>
                  {/each}
                </ul>
              </div>
            </div>

            <!-- Totals -->
            <div class="confirm-totals card">
              <div class="card-body">
                <dl class="totals-list">
                  <div class="totals-row">
                    <dt>Товари</dt>
                    <dd>₴{subtotal.toFixed(2)}</dd>
                  </div>
                  <div class="totals-row">
                    <dt>Доставка</dt>
                    <dd>
                      {#if shippingCost === 0}
                        <span class="free-shipping">Безкоштовно</span>
                      {:else}
                        ₴{shippingCost.toFixed(2)}
                      {/if}
                    </dd>
                  </div>
                  <hr class="divider" />
                  <div class="totals-row totals-grand">
                    <dt>Разом</dt>
                    <dd>₴{total.toFixed(2)}</dd>
                  </div>
                </dl>
              </div>
            </div>

            <!-- Terms + Submit -->
            <div class="checkout-submit">
              <label class="terms-label">
                <input type="checkbox" bind:checked={termsAccepted} class="terms-checkbox" />
                <span>Я погоджуюсь з <a href="/terms" class="link-primary">умовами використання</a> та <a href="/privacy" class="link-primary">політикою конфіденційності</a></span>
              </label>
              {#if termsError}
                <p class="field-error" role="alert">{termsError}</p>
              {/if}

              <div class="step-nav step-nav-final">
                <button type="button" class="btn btn-outline" onclick={prevStep}>Назад</button>
                <button
                  type="button"
                  class="btn btn-primary btn-lg"
                  onclick={handleSubmit}
                  disabled={isSubmitting || basketStore.isLoading}
                >
                  {#if isSubmitting}
                    Обробка…
                  {:else}
                    Оплатити ₴{total.toFixed(2)}
                  {/if}
                </button>
              </div>
            </div>
          </div>
        {/if}
      </div>

      <!-- Sidebar Summary (visible on desktop) -->
      <aside class="checkout-sidebar" aria-label="Підсумок замовлення">
        <div class="sidebar-card card">
          <div class="card-body">
            <h3 class="sidebar-heading">Ваше замовлення</h3>

            <ul class="sidebar-items">
              {#each items as item (item.itemId)}
                <li class="sidebar-item">
                  <div class="sidebar-item-thumb">
                    {#if item.imageUrl}
                      <img src={item.imageUrl} alt={item.productName} width="48" height="48" loading="lazy" />
                    {:else}
                      <div class="sidebar-thumb-placeholder">📷</div>
                    {/if}
                    <span class="sidebar-item-qty-badge">{item.quantity}</span>
                  </div>
                  <div class="sidebar-item-info">
                    <span class="sidebar-item-name">{item.productName}</span>
                    {#if formatVariant(item)}
                      <span class="sidebar-item-variant">{formatVariant(item)}</span>
                    {/if}
                  </div>
                  <span class="sidebar-item-price">₴{(item.unitPrice * item.quantity).toFixed(2)}</span>
                </li>
              {/each}
            </ul>

            <hr class="divider" />

            <dl class="sidebar-totals">
              <div class="sidebar-totals-row">
                <dt>Товари</dt>
                <dd>₴{subtotal.toFixed(2)}</dd>
              </div>
              <div class="sidebar-totals-row">
                <dt>Доставка</dt>
                <dd>
                  {#if shippingCost === 0}
                    <span class="free-shipping">Безкоштовно</span>
                  {:else}
                    ₴{shippingCost.toFixed(2)}
                  {/if}
                </dd>
              </div>
              <hr class="divider" />
              <div class="sidebar-totals-row sidebar-grand">
                <dt>Разом</dt>
                <dd>₴{total.toFixed(2)}</dd>
              </div>
            </dl>
          </div>
        </div>
      </aside>
    </div>
  {/if}
</section>

<style>
  /* ── Page ── */
  .checkout-page {
    padding-block: var(--space-4) var(--space-12);
  }

  .checkout-empty {
    text-align: center;
    padding: var(--space-16);
    font-size: 1.125rem;
    color: var(--color-text-light);
  }

  .link-primary {
    color: var(--color-primary);
    text-decoration: underline;
    text-underline-offset: 2px;
  }

  .link-primary:hover {
    color: var(--color-primary-dark);
  }

  /* ── Stepper ── */
  .stepper {
    margin-bottom: var(--space-8);
  }

  .stepper-list {
    display: flex;
    align-items: center;
    justify-content: center;
    list-style: none;
    padding: 0;
    margin: 0;
    gap: 0;
  }

  .stepper-item {
    display: flex;
    align-items: center;
  }

  .stepper-btn {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    background: none;
    border: none;
    cursor: pointer;
    padding: var(--space-2);
    transition: opacity var(--transition-fast);
  }

  .stepper-btn:disabled {
    cursor: default;
    opacity: 0.6;
  }

  .stepper-circle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: var(--radius-full);
    font-size: 0.875rem;
    font-weight: 700;
    border: 2px solid var(--color-border);
    color: var(--color-text-light);
    background-color: var(--color-surface);
    transition: all var(--transition-base);
    flex-shrink: 0;
  }

  .stepper-item.active .stepper-circle {
    border-color: var(--color-primary);
    background-color: var(--color-primary);
    color: #FFFFFF;
  }

  .stepper-item.completed .stepper-circle {
    border-color: var(--color-success);
    background-color: var(--color-success);
    color: #FFFFFF;
  }

  .stepper-label {
    font-size: 0.8125rem;
    font-weight: 500;
    color: var(--color-text-light);
    white-space: nowrap;
  }

  .stepper-item.active .stepper-label {
    color: var(--color-text);
    font-weight: 600;
  }

  .stepper-item.completed .stepper-label {
    color: var(--color-success);
  }

  .stepper-line {
    width: 40px;
    height: 2px;
    background-color: var(--color-border);
    margin-inline: var(--space-2);
    transition: background-color var(--transition-base);
    flex-shrink: 0;
  }

  .stepper-line.completed {
    background-color: var(--color-success);
  }

  /* ── Layout ── */
  .checkout-layout {
    display: flex;
    flex-direction: column;
    gap: var(--space-8);
  }

  .checkout-main {
    flex: 1;
  }

  .checkout-sidebar {
    display: none;
  }

  /* ── Step Panel ── */
  .step-panel {
    animation: fadeIn var(--transition-base) both;
  }

  .step-heading {
    font-family: var(--font-display);
    font-size: 1.5rem;
    font-weight: 600;
    margin-bottom: var(--space-6);
  }

  .step-nav {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: var(--space-8);
    gap: var(--space-4);
  }

  .step-nav-final {
    margin-top: var(--space-6);
  }

  /* ── Shipping Form ── */
  .form-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: var(--space-4);
  }

  .form-field {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
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
    box-shadow: 0 0 0 3px rgba(196, 85, 90, 0.12) !important;
  }

  .field-error {
    font-size: 0.8125rem;
    color: var(--color-error);
    margin-top: var(--space-1);
  }

  /* ── Payment Options ── */
  .payment-options {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .payment-card {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    padding: var(--space-4) var(--space-5);
    cursor: pointer;
    border: 2px solid var(--color-border-light);
    transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
  }

  .payment-card:hover {
    border-color: var(--color-primary-light);
    box-shadow: var(--shadow-sm);
  }

  .payment-card.selected {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 3px rgba(139, 94, 107, 0.12);
  }

  .payment-icon {
    font-size: 1.5rem;
    flex-shrink: 0;
  }

  .payment-label {
    flex: 1;
    font-size: 0.9375rem;
    font-weight: 500;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: var(--space-2);
  }

  .recommended-badge {
    font-size: 0.625rem;
    padding: 2px var(--space-2);
  }

  .payment-check {
    flex-shrink: 0;
    color: var(--color-text-muted);
  }

  .payment-card.selected .payment-check {
    color: var(--color-primary);
  }

  /* ── Confirmation ── */
  .confirm-section {
    margin-bottom: var(--space-4);
  }

  .confirm-section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: var(--space-3);
  }

  .confirm-section-title {
    font-family: var(--font-display);
    font-size: 1rem;
    font-weight: 600;
  }

  .edit-link {
    font-size: 0.8125rem;
    color: var(--color-primary);
    cursor: pointer;
    background: none;
    border: none;
    text-decoration: underline;
    text-underline-offset: 2px;
    transition: color var(--transition-fast);
  }

  .edit-link:hover {
    color: var(--color-primary-dark);
  }

  .shipping-summary {
    font-style: normal;
    font-size: 0.9375rem;
    line-height: 1.6;
    color: var(--color-text-light);
  }

  .summary-name {
    font-weight: 600;
    color: var(--color-text);
  }

  .summary-phone {
    margin-top: var(--space-1);
  }

  .confirm-items-list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    margin-top: var(--space-3);
  }

  .confirm-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: var(--space-3);
    font-size: 0.875rem;
  }

  .confirm-item-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
    flex: 1;
    min-width: 0;
  }

  .confirm-item-name {
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .confirm-item-variant {
    font-size: 0.8125rem;
    color: var(--color-text-muted);
  }

  .confirm-item-qty {
    font-size: 0.8125rem;
    color: var(--color-text-light);
  }

  .confirm-item-price {
    flex-shrink: 0;
    font-weight: 600;
  }

  /* ── Totals ── */
  .confirm-totals {
    margin-bottom: var(--space-4);
  }

  .totals-list {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .totals-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.9375rem;
  }

  .totals-row dt {
    color: var(--color-text-light);
  }

  .totals-row dd {
    font-weight: 500;
  }

  .totals-grand {
    font-size: 1.25rem;
    font-weight: 700;
  }

  .totals-grand dd {
    color: var(--color-primary);
    font-family: var(--font-display);
    font-size: 1.5rem;
  }

  .free-shipping {
    color: var(--color-success);
    font-weight: 600;
  }

  /* ── Terms ── */
  .checkout-submit {
    margin-top: var(--space-6);
  }

  .terms-label {
    display: flex;
    align-items: flex-start;
    gap: var(--space-3);
    cursor: pointer;
    font-size: 0.875rem;
    line-height: 1.5;
    color: var(--color-text-light);
  }

  .terms-checkbox {
    width: 18px;
    height: 18px;
    margin-top: 2px;
    flex-shrink: 0;
    accent-color: var(--color-primary);
  }

  /* ── Sidebar ── */
  .sidebar-heading {
    font-family: var(--font-display);
    font-size: 1.125rem;
    font-weight: 600;
    margin-bottom: var(--space-4);
  }

  .sidebar-items {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .sidebar-item {
    display: flex;
    align-items: center;
    gap: var(--space-3);
  }

  .sidebar-item-thumb {
    position: relative;
    flex-shrink: 0;
  }

  .sidebar-item-thumb img {
    width: 48px;
    height: 48px;
    object-fit: cover;
    border-radius: var(--radius-md);
    background-color: var(--color-surface-hover);
  }

  .sidebar-thumb-placeholder {
    width: 48px;
    height: 48px;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: var(--color-surface-hover);
    border-radius: var(--radius-md);
    font-size: 1rem;
  }

  .sidebar-item-qty-badge {
    position: absolute;
    top: -6px;
    right: -6px;
    background-color: var(--color-primary);
    color: #FFFFFF;
    font-size: 0.6875rem;
    font-weight: 700;
    width: 20px;
    height: 20px;
    border-radius: var(--radius-full);
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .sidebar-item-info {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    gap: 1px;
  }

  .sidebar-item-name {
    font-size: 0.8125rem;
    font-weight: 500;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .sidebar-item-variant {
    font-size: 0.75rem;
    color: var(--color-text-muted);
  }

  .sidebar-item-price {
    font-size: 0.875rem;
    font-weight: 600;
    flex-shrink: 0;
  }

  .sidebar-totals {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }

  .sidebar-totals-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.875rem;
  }

  .sidebar-totals-row dt {
    color: var(--color-text-light);
  }

  .sidebar-totals-row dd {
    font-weight: 500;
  }

  .sidebar-grand {
    font-size: 1.125rem;
    font-weight: 700;
  }

  .sidebar-grand dd {
    color: var(--color-primary);
    font-family: var(--font-display);
    font-size: 1.25rem;
  }

  /* ── Responsive ── */
  @media (min-width: 640px) {
    .form-grid {
      grid-template-columns: 1fr 1fr;
    }

    .full-width {
      grid-column: 1 / -1;
    }

    .stepper-line {
      width: 60px;
    }
  }

  @media (min-width: 1024px) {
    .checkout-layout {
      flex-direction: row;
      align-items: flex-start;
    }

    .checkout-main {
      flex: 2;
    }

    .checkout-sidebar {
      display: block;
      flex: 1;
      max-width: 380px;
      position: sticky;
      top: var(--space-6);
    }

    .stepper-line {
      width: 80px;
    }
  }
</style>
