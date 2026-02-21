import { api } from '$api/client';
import type {
	Order,
	OrderStatusUpdate,
	PaginatedResponse,
} from '$types';

export async function getOrders(params?: { page?: number; pageSize?: number; status?: string }): Promise<PaginatedResponse<Order>> {
	return api.get<PaginatedResponse<Order>>('/orders', params as Record<string, string | number | boolean | undefined>);
}

export async function getOrderById(id: string): Promise<Order> {
	return api.get<Order>(`/orders/${id}`);
}

export async function getShopOrders(shopId: string, params?: { page?: number; pageSize?: number; status?: string }): Promise<PaginatedResponse<Order>> {
	return api.get<PaginatedResponse<Order>>(`/shops/${shopId}/orders`, params as Record<string, string | number | boolean | undefined>);
}

export async function updateOrderStatus(id: string, data: OrderStatusUpdate): Promise<Order> {
	return api.put<Order>(`/orders/${id}/status`, data);
}

export async function addTrackingNumber(id: string, data: { trackingNumber: string }): Promise<Order> {
	return api.post<Order>(`/orders/${id}/tracking`, data);
}

export async function cancelOrder(id: string): Promise<Order> {
	return api.post<Order>(`/orders/${id}/cancel`);
}
