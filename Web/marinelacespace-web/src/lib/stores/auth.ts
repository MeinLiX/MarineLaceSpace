import type {
	AuthUser,
	LoginRequest,
	LoginResponse,
	RegisterRequest,
} from '$types';
import * as authApi from '$api/auth';

interface AuthState {
	currentUser: AuthUser | null;
	isLoading: boolean;
}

function createAuthStore() {
	let state = $state<AuthState>({
		currentUser: null,
		isLoading: false,
	});

	const isAuthenticated = $derived(state.currentUser !== null && !state.currentUser.isAnonymous);
	const isAdmin = $derived(state.currentUser?.roles.includes('Admin') ?? false);
	const isSeller = $derived(state.currentUser?.roles.includes('Seller') ?? false);

	function persistTokens(response: LoginResponse): void {
		localStorage.setItem('access_token', response.accessToken);
		localStorage.setItem('refresh_token', response.refreshToken);
		localStorage.setItem('token_expires_at', response.expiresAt);
	}

	function clearTokens(): void {
		localStorage.removeItem('access_token');
		localStorage.removeItem('refresh_token');
		localStorage.removeItem('token_expires_at');
	}

	async function login(data: LoginRequest): Promise<void> {
		state.isLoading = true;
		try {
			const response = await authApi.login(data);
			persistTokens(response);
			state.currentUser = await authApi.getMe();
		} finally {
			state.isLoading = false;
		}
	}

	async function register(data: RegisterRequest): Promise<void> {
		state.isLoading = true;
		try {
			const response = await authApi.register(data);
			persistTokens(response);
			state.currentUser = await authApi.getMe();
		} finally {
			state.isLoading = false;
		}
	}

	function logout(): void {
		clearTokens();
		state.currentUser = null;
		window.location.href = '/auth/login';
	}

	async function refreshToken(): Promise<boolean> {
		const token = localStorage.getItem('refresh_token');
		if (!token) return false;

		try {
			const response = await authApi.refreshToken({ refreshToken: token });
			persistTokens(response);
			return true;
		} catch {
			clearTokens();
			state.currentUser = null;
			return false;
		}
	}

	async function loadUser(): Promise<void> {
		const token = localStorage.getItem('access_token');
		if (!token) return;

		state.isLoading = true;
		try {
			state.currentUser = await authApi.getMe();
		} catch {
			const refreshed = await refreshToken();
			if (refreshed) {
				try {
					state.currentUser = await authApi.getMe();
				} catch {
					clearTokens();
					state.currentUser = null;
				}
			}
		} finally {
			state.isLoading = false;
		}
	}

	return {
		get currentUser() { return state.currentUser; },
		get isLoading() { return state.isLoading; },
		get isAuthenticated() { return isAuthenticated; },
		get isAdmin() { return isAdmin; },
		get isSeller() { return isSeller; },
		login,
		register,
		logout,
		refreshToken,
		loadUser,
	};
}

export const authStore = createAuthStore();
