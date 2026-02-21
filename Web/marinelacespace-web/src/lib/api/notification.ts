import { api } from '$api/client';

export interface SendEmailRequest {
	to: string;
	subject: string;
	body: string;
}

export interface ContactFormRequest {
	name: string;
	email: string;
	subject: string;
	message: string;
}

/**
 * Send a contact form message.
 * This sends an email to the site's support address via the Notification service.
 */
export async function sendContactForm(data: ContactFormRequest): Promise<void> {
	return api.post<void>('/notifications/contact', data);
}

/**
 * Send an email notification (admin only).
 */
export async function sendEmail(data: SendEmailRequest): Promise<void> {
	return api.post<void>('/notifications/email', data);
}
