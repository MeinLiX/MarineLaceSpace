import type { en } from './translations/en';

export type Translations = typeof en;
export type Locale = 'en' | 'uk';

/** Nested key path helper: 'home.heroTitle' | 'auth.email' etc. */
type NestedKeyOf<T, Prefix extends string = ''> = T extends string
	? Prefix
	: {
			[K in keyof T & string]: NestedKeyOf<T[K], Prefix extends '' ? K : `${Prefix}.${K}`>;
		}[keyof T & string];

export type TranslationKey = NestedKeyOf<Translations>;
