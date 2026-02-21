import type {
	Basket,
	AddToBasketRequest,
	UpdateBasketItemRequest,
	CheckoutRequest,
	Order,
} from '$types';
import * as basketApi from '$api/basket';

function createBasketStore() {
	let basket = $state<Basket | null>(null);
	let isLoading = $state(false);

	const itemCount = $derived(
		basket?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0
	);

	async function loadBasket(): Promise<void> {
		isLoading = true;
		try {
			basket = await basketApi.getBasket();
		} catch {
			basket = null;
		} finally {
			isLoading = false;
		}
	}

	async function addItem(data: AddToBasketRequest): Promise<void> {
		isLoading = true;
		try {
			basket = await basketApi.addItem(data);
		} finally {
			isLoading = false;
		}
	}

	async function updateItem(itemId: string, data: UpdateBasketItemRequest): Promise<void> {
		isLoading = true;
		try {
			basket = await basketApi.updateItem(itemId, data);
		} finally {
			isLoading = false;
		}
	}

	async function removeItem(itemId: string): Promise<void> {
		isLoading = true;
		try {
			basket = await basketApi.removeItem(itemId);
		} finally {
			isLoading = false;
		}
	}

	async function clearBasket(): Promise<void> {
		isLoading = true;
		try {
			await basketApi.clearBasket();
			basket = null;
		} finally {
			isLoading = false;
		}
	}

	async function checkout(data: CheckoutRequest): Promise<Order> {
		isLoading = true;
		try {
			const order = await basketApi.checkout(data);
			basket = null;
			return order;
		} finally {
			isLoading = false;
		}
	}

	return {
		get basket() { return basket; },
		get isLoading() { return isLoading; },
		get itemCount() { return itemCount; },
		loadBasket,
		addItem,
		updateItem,
		removeItem,
		clearBasket,
		checkout,
	};
}

export const basketStore = createBasketStore();
