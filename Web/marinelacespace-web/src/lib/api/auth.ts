import { api } from '$api/client';
import type {
	AuthUser,
	LoginRequest,
	LoginResponse,
	RegisterRequest,
	RefreshTokenRequest,
	UpdateProfileRequest,
	ForgotPasswordRequest,
	ResetPasswordRequest,
	PaginatedResponse,
} from '$types';

export async function register(data: RegisterRequest): Promise<LoginResponse> {
	return api.post<LoginResponse>('/auth/register', data);
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
	return api.post<LoginResponse>('/auth/login', data);
}

export async function refreshToken(data: RefreshTokenRequest): Promise<LoginResponse> {
	return api.post<LoginResponse>('/auth/refresh-token', data);
}

export async function forgotPassword(data: ForgotPasswordRequest): Promise<void> {
	return api.post<void>('/auth/forgot-password', data);
}

export async function resetPassword(data: ResetPasswordRequest): Promise<void> {
	return api.post<void>('/auth/reset-password', data);
}

export async function getMe(): Promise<AuthUser> {
	return api.get<AuthUser>('/users/me');
}

export async function updateMe(data: UpdateProfileRequest): Promise<AuthUser> {
	return api.put<AuthUser>('/users/me', data);
}

export async function getUserById(id: string): Promise<AuthUser> {
	return api.get<AuthUser>(`/users/${id}`);
}

export async function assignRoles(userId: string, roles: string[]): Promise<void> {
	return api.post<void>(`/users/${userId}/roles`, roles);
}

export async function getUsers(params?: { page?: number; pageSize?: number; search?: string }): Promise<PaginatedResponse<AuthUser>> {
	return api.get<PaginatedResponse<AuthUser>>('/users', params as Record<string, string | number | boolean | undefined>);
}
