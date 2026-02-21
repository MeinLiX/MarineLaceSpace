import { en } from './translations/en';
import { uk } from './translations/uk';
import type { Locale, Translations, TranslationKey } from './types';

export type { Locale, TranslationKey, Translations };

const STORAGE_KEY = 'mls-locale';
const DEFAULT_LOCALE: Locale = 'en';

const translations: Record<Locale, Translations> = { en, uk };
const availableLocales: { code: Locale; label: string; flag: string }[] = [
	{ code: 'en', label: 'English', flag: 'us' },
	{ code: 'uk', label: 'Українська', flag: '🇺🇦' },
];

function getInitialLocale(): Locale {
	if (typeof window === 'undefined') return DEFAULT_LOCALE;
	const stored = localStorage.getItem(STORAGE_KEY);
	if (stored === 'en' || stored === 'uk') return stored;
	return DEFAULT_LOCALE;
}

function createI18nStore() {
	let locale = $state<Locale>(getInitialLocale());

	function setLocale(newLocale: Locale) {
		locale = newLocale;
		if (typeof window !== 'undefined') {
			localStorage.setItem(STORAGE_KEY, newLocale);
			document.documentElement.lang = newLocale;
		}
	}

	/**
	 * Translate a dotted key like 'home.heroTitle'.
	 * Supports simple `{placeholder}` replacement via params.
	 */
	function t(key: string, params?: Record<string, string | number>): string {
		const parts = key.split('.');
		let result: unknown = translations[locale];
		for (const part of parts) {
			if (result && typeof result === 'object') {
				result = (result as Record<string, unknown>)[part];
			} else {
				return key; // fallback to key itself
			}
		}
		if (typeof result !== 'string') return key;

		if (params) {
			return result.replace(/\{(\w+)\}/g, (_, k) => String(params[k] ?? `{${k}}`));
		}
		return result;
	}

	// Set lang attribute on mount
	if (typeof window !== 'undefined') {
		document.documentElement.lang = locale;
	}

	return {
		get locale() { return locale; },
		get locales() { return availableLocales; },
		setLocale,
		t,
	};
}

export const i18n = createI18nStore();

/** Shorthand for the translate function */
export const t = (key: string, params?: Record<string, string | number>) => i18n.t(key, params);
