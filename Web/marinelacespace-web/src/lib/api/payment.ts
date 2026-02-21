import { api } from '$api/client';
import type {
	PaymentRecord,
	CreatePaymentRequest,
	PaymentSession,
} from '$types';

export async function createCheckoutSession(data: CreatePaymentRequest): Promise<PaymentSession> {
	return api.post<PaymentSession>('/payments/checkout', data);
}

export async function getPaymentById(id: string): Promise<PaymentRecord> {
	return api.get<PaymentRecord>(`/payments/${id}`);
}

export async function getPaymentsByOrder(orderId: string): Promise<PaymentRecord[]> {
	return api.get<PaymentRecord[]>(`/orders/${orderId}/payments`);
}

export async function refundPayment(id: string): Promise<PaymentRecord> {
	return api.post<PaymentRecord>(`/payments/${id}/refund`);
}
