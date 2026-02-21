import { api } from '$api/client';
import type {
	Basket,
	AddToBasketRequest,
	UpdateBasketItemRequest,
	CheckoutRequest,
	Order,
} from '$types';

export async function getBasket(): Promise<Basket> {
	return api.get<Basket>('/basket');
}

export async function addItem(data: AddToBasketRequest): Promise<Basket> {
	return api.post<Basket>('/basket/items', data);
}

export async function updateItem(itemId: string, data: UpdateBasketItemRequest): Promise<Basket> {
	return api.put<Basket>(`/basket/items/${itemId}`, data);
}

export async function removeItem(itemId: string): Promise<Basket> {
	return api.delete<Basket>(`/basket/items/${itemId}`);
}

export async function clearBasket(): Promise<void> {
	return api.delete<void>('/basket');
}

export async function checkout(data: CheckoutRequest): Promise<Order> {
	return api.post<Order>('/basket/checkout', data);
}
