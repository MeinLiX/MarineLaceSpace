import { HubConnectionBuilder, LogLevel, type HubConnection, HttpTransportType } from '@microsoft/signalr';
import { notificationStore } from '$stores/notification.svelte';

let connection: HubConnection | null = null;

function getHubUrl(): string {
	const base =
		((typeof window !== 'undefined' && (window as unknown as Record<string, unknown>).__PUBLIC_API_URL__) as string) ||
		(import.meta.env.PUBLIC_API_URL as string) ||
		'/api';
	// Replace trailing /api with the hub path, or append if pattern differs
	return base.replace(/\/api\/?$/, '') + '/api/notifications/hub';
}

export async function startSignalR(): Promise<void> {
	if (connection) return;

	const token = typeof window !== 'undefined' ? localStorage.getItem('access_token') : null;
	if (!token) return;

	connection = new HubConnectionBuilder()
		.withUrl(getHubUrl(), {
			accessTokenFactory: () => localStorage.getItem('access_token') || '',
			transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
		})
		.withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
		.configureLogging(LogLevel.Warning)
		.build();

	connection.on('ReceiveNotification', (notification: { type: string; payload: any; timestamp: string }) => {
		handleNotification(notification);
	});

	connection.onreconnecting(() => {
		console.log('[SignalR] Reconnecting...');
	});

	connection.onreconnected(() => {
		console.log('[SignalR] Reconnected');
	});

	connection.onclose(() => {
		console.log('[SignalR] Connection closed');
		connection = null;
	});

	try {
		await connection.start();
		console.log('[SignalR] Connected');
	} catch (err) {
		console.error('[SignalR] Connection failed:', err);
		connection = null;
	}
}

export async function stopSignalR(): Promise<void> {
	if (connection) {
		await connection.stop();
		connection = null;
	}
}

export function getConnection(): HubConnection | null {
	return connection;
}

function handleNotification(notification: { type: string; payload: any; timestamp: string }): void {
	const { type, payload } = notification;

	switch (type) {
		case 'OrderCreated':
			notificationStore.success(`Order #${payload.orderId} placed successfully!`);
			break;
		case 'OrderStatusChanged':
			notificationStore.info(`Order #${payload.orderId} → ${payload.newStatus}`);
			break;
		case 'PaymentSucceeded':
			notificationStore.success('Payment confirmed!');
			break;
		case 'PaymentFailed':
			notificationStore.error(`Payment failed${payload.reason ? ': ' + payload.reason : ''}`);
			break;
		default:
			notificationStore.info(payload?.message || 'New notification');
	}
}
