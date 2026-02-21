import type { ApiError } from '$types';

const BASE_URL = (typeof window !== 'undefined' && (window as Record<string, unknown>).__PUBLIC_API_URL__) as string
	|| import.meta.env.PUBLIC_API_URL as string
	|| '/api';

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
				apiError = await response.json();
			} catch {
				apiError = { message: response.statusText || `HTTP ${response.status}` };
			}

			throw apiError;
		}

		const text = await response.text();
		if (!text) return undefined as T;
		return JSON.parse(text) as T;
	}

	private buildHeaders(contentType?: string): HeadersInit {
		const headers: Record<string, string> = {};
		const token = this.getToken();

		if (token) {
			headers['Authorization'] = `Bearer ${token}`;
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
}

export const api = new ApiClient(BASE_URL);
export { ApiClient };
