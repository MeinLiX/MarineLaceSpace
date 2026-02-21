export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface Notification {
	id: string;
	message: string;
	type: NotificationType;
	timeout: number;
}

const DEFAULT_TIMEOUT = 5000;

function createNotificationStore() {
	let notifications = $state<Notification[]>([]);

	function addNotification(message: string, type: NotificationType, timeout = DEFAULT_TIMEOUT): string {
		const id = crypto.randomUUID();
		const notification: Notification = { id, message, type, timeout };

		notifications = [...notifications, notification];

		if (timeout > 0) {
			setTimeout(() => removeNotification(id), timeout);
		}

		return id;
	}

	function removeNotification(id: string): void {
		notifications = notifications.filter((n) => n.id !== id);
	}

	function success(message: string, timeout?: number): string {
		return addNotification(message, 'success', timeout);
	}

	function error(message: string, timeout?: number): string {
		return addNotification(message, 'error', timeout);
	}

	function warning(message: string, timeout?: number): string {
		return addNotification(message, 'warning', timeout);
	}

	function info(message: string, timeout?: number): string {
		return addNotification(message, 'info', timeout);
	}

	return {
		get notifications() { return notifications; },
		addNotification,
		removeNotification,
		success,
		error,
		warning,
		info,
	};
}

export const notificationStore = createNotificationStore();
