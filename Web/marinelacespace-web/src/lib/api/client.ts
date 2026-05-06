import type { ApiError, RESTResultEnvelope } from '$types';
import { notificationStore } from '$stores/notification.svelte';

const BASE_URL = (typeof window !== 'undefined' && (window as unknown as Record<string, unknown>).__PUBLIC_API_URL__) as string
	|| import.meta.env.PUBLIC_API_URL as string
	|| '/api';

const SESSION_ID_KEY = 'mls_session_id';

function getOrCreateSessionId(): string | null {
	if (typeof window === 'undefined') return null;
	let sessionId = localStorage.getItem(SESSION_ID_KEY);
	if (!sessionId) {
		sessionId = crypto.randomUUID();
		localStorage.setItem(SESSION_ID_KEY, sessionId);
	}
	return sessionId;
}

class ApiClient {
	private baseUrl: string;

	constructor(baseUrl: string) {
		this.baseUrl = baseUrl;
	}

	private getToken(): string | null {
		if (typeof window === 'undefined') return null;
		return localStorage.getItem('access_token');
	}

	private async handleResponse<T>(response: Response): Promise<T> {
		if (response.status === 204) {
			return undefined as T;
		}

		if (!response.ok) {
			if (response.status === 401) {
				if (typeof window !== 'undefined') {
					localStorage.removeItem('access_token');
					localStorage.removeItem('refresh_token');
					window.location.href = '/auth/login';
				}
			}

			let apiError: ApiError;
			try {
				const body = await response.json();
				// Handle RESTResult / RESTErrorResult envelope
				if (body && typeof body === 'object' && 'succeeded' in body) {
					const envelope = body as RESTResultEnvelope;
					const errors = (envelope.data && typeof envelope.data === 'object' && !Array.isArray(envelope.data))
						? envelope.data as Record<string, string[]>
						: undefined;
					const errorMessages: string[] = [];
					if (envelope.message) errorMessages.push(envelope.message);
					if (errors) {
						for (const fieldErrors of Object.values(errors)) {
							errorMessages.push(...fieldErrors);
						}
					}
					apiError = {
						message: errorMessages.join('. ') || `Request failed (${response.status})`,
						errors,
					};
				} else {
					apiError = { message: body?.message || body?.error || response.statusText || `HTTP ${response.status}` };
				}
			} catch {
				apiError = { message: response.statusText || `HTTP ${response.status}` };
			}

			// Auto-show backend error as toast
			if (typeof window !== 'undefined') {
				try { notificationStore.error(apiError.message); } catch { /* noop */ }
			}

			throw apiError;
		}

		const text = await response.text();
		if (!text) return undefined as T;

		const parsed = JSON.parse(text);

		// Unwrap RESTResult envelope: if it has { succeeded, data }, return .data
		if (parsed && typeof parsed === 'object' && 'succeeded' in parsed && 'data' in parsed) {
			return parsed.data as T;
		}

		return parsed as T;
	}

	private buildHeaders(contentType?: string): HeadersInit {
		const headers: Record<string, string> = {};
		const token = this.getToken();

		if (token) {
			headers['Authorization'] = `Bearer ${token}`;
		} else {
			// Anonymous user: send session ID for basket persistence
			const sessionId = getOrCreateSessionId();
			if (sessionId) {
				headers['X-Session-Id'] = sessionId;
			}
		}

		if (contentType) {
			headers['Content-Type'] = contentType;
		}

		return headers;
	}

	private buildUrl(path: string, params?: Record<string, string | number | boolean | undefined | null>): string {
		const url = new URL(`${this.baseUrl}${path}`, typeof window !== 'undefined' ? window.location.origin : undefined);

		if (params) {
			for (const [key, value] of Object.entries(params)) {
				if (value != null && value !== '') {
					url.searchParams.set(key, String(value));
				}
			}
		}

		return url.toString();
	}

	async get<T>(path: string, params?: Record<string, string | number | boolean | undefined | null>): Promise<T> {
		const response = await fetch(this.buildUrl(path, params), {
			method: 'GET',
			headers: this.buildHeaders(),
		});
		return this.handleResponse<T>(response);
	}

	async post<T>(path: string, body?: unknown): Promise<T> {
		const response = await fetch(this.buildUrl(path), {
			method: 'POST',
			headers: this.buildHeaders('application/json'),
			body: body != null ? JSON.stringify(body) : undefined,
		});
		return this.handleResponse<T>(response);
	}

	async put<T>(path: string, body?: unknown): Promise<T> {
		const response = await fetch(this.buildUrl(path), {
			method: 'PUT',
			headers: this.buildHeaders('application/json'),
			body: body != null ? JSON.stringify(body) : undefined,
		});
		return this.handleResponse<T>(response);
	}

	async patch<T>(path: string, body?: unknown): Promise<T> {
		const response = await fetch(this.buildUrl(path), {
			method: 'PATCH',
			headers: this.buildHeaders('application/json'),
			body: body != null ? JSON.stringify(body) : undefined,
		});
		return this.handleResponse<T>(response);
	}

	async delete<T>(path: string): Promise<T> {
		const response = await fetch(this.buildUrl(path), {
			method: 'DELETE',
			headers: this.buildHeaders(),
		});
		return this.handleResponse<T>(response);
	}

	async postFormData<T>(path: string, formData: FormData): Promise<T> {
		const response = await fetch(this.buildUrl(path), {
			method: 'POST',
			headers: this.buildHeaders(), // NO Content-Type - browser sets it with boundary
			body: formData,
		});
		return this.handleResponse<T>(response);
	}

	async putFormData<T>(path: string, formData: FormData): Promise<T> {
		const response = await fetch(this.buildUrl(path), {
			method: 'PUT',
			headers: this.buildHeaders(), // NO Content-Type
			body: formData,
		});
		return this.handleResponse<T>(response);
	}
}

export const api = new ApiClient(BASE_URL);
export { ApiClient, getOrCreateSessionId, SESSION_ID_KEY };
