// ─── Auth Types ──────────────────────────────────────────────────────────────

export interface AuthUser {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	roles: string[];
	isAnonymous: boolean;
}

export interface LoginRequest {
	email: string;
	password: string;
}

export interface LoginResponse {
	accessToken: string;
	refreshToken: string;
	expiresAt: string;
}

export interface RegisterRequest {
	email: string;
	password: string;
	firstName: string;
	lastName: string;
}

export interface RefreshTokenRequest {
	refreshToken: string;
}

export interface UpdateProfileRequest {
	firstName: string;
	lastName: string;
	phoneNumber?: string;
}

export interface ForgotPasswordRequest {
	email: string;
}

export interface ResetPasswordRequest {
	email: string;
	token: string;
	newPassword: string;
}

// ─── Catalog Types ───────────────────────────────────────────────────────────

export interface Category {
	id: string;
	name: string;
	slug: string;
	parentCategoryId: string | null;
	level: number;
	fullPath: string;
	childCategories: Category[];
	productCount: number;
}

export interface Shop {
	id: string;
	name: string;
	slug: string;
	description: string;
	logoUrl: string | null;
	bannerUrl: string | null;
	ownerId: string;
	ownerName: string;
	createdAt: string;
	productCount: number;
	averageRating: number;
	reviewCount: number;
}

export type ProductStatus = 'Draft' | 'Active' | 'Inactive' | 'SoldOut';

export interface Product {
	id: string;
	title: string;
	description: string;
	categoryId: string;
	categoryName: string;
	shopId: string;
	shopName: string;
	status: ProductStatus;
	isPersonalizationEnabled: boolean;
	personalizationPrompt: string | null;
	minPrice: number;
	maxPrice: number;
	mainImageUrl: string | null;
	averageRating: number;
	reviewCount: number;
	createdAt: string;
	updatedAt: string;
}

export interface ProductImage {
	id: string;
	url: string;
	altText: string | null;
	isMain: boolean;
	sortOrder: number;
	colorId: string | null;
	materialId: string | null;
}

export type Gender = 'Male' | 'Female' | 'Unisex';

export interface ProductSize {
	sizeId: string;
	sizeName: string;
	gender: Gender;
	priceModifier: number;
}

export interface ProductColor {
	colorId: string;
	colorName: string;
	hexCode: string;
	priceModifier: number;
}

export interface ProductMaterial {
	materialId: string;
	materialName: string;
	priceModifier: number;
}

export interface ProductPrice {
	id: string;
	sizeId: string;
	colorId: string;
	materialId: string;
	basePrice: number;
	oldPrice: number | null;
	discountPercentage: number | null;
	quantity: number;
}

export interface ProductDetail extends Product {
	images: ProductImage[];
	sizes: ProductSize[];
	colors: ProductColor[];
	materials: ProductMaterial[];
	prices: ProductPrice[];
}

export interface Size {
	id: string;
	name: string;
	gender: Gender;
}

export interface Color {
	id: string;
	name: string;
	hexCode: string;
}

export interface Material {
	id: string;
	name: string;
}

export interface ProductReview {
	id: string;
	productId: string;
	userId: string | null;
	guestName: string | null;
	rating: number;
	title: string | null;
	text: string;
	isVerifiedPurchase: boolean;
	createdAt: string;
}

export interface ReviewSummary {
	averageRating: number;
	totalCount: number;
	distribution: Record<number, number>;
}

// ─── Basket Types ────────────────────────────────────────────────────────────

export interface Basket {
	buyerId: string;
	items: BasketItem[];
	totalPrice: number;
}

export interface BasketItem {
	itemId: string;
	productId: string;
	productName: string;
	sizeId: string | null;
	sizeName: string | null;
	colorId: string | null;
	colorName: string | null;
	materialId: string | null;
	materialName: string | null;
	unitPrice: number;
	quantity: number;
	personalization: string | null;
	imageUrl: string | null;
}

export interface AddToBasketRequest {
	productId: string;
	sizeId?: string;
	colorId?: string;
	materialId?: string;
	quantity: number;
	personalization?: string;
}

export interface UpdateBasketItemRequest {
	quantity: number;
	personalization?: string;
}

export interface ShippingAddress {
	fullName: string;
	addressLine1: string;
	addressLine2?: string;
	city: string;
	state: string;
	postalCode: string;
	country: string;
	phone: string;
}

export interface CheckoutRequest {
	shippingAddress: ShippingAddress;
	paymentMethod: string;
}

// ─── Order Types ─────────────────────────────────────────────────────────────

export type OrderStatus =
	| 'New'
	| 'PendingPayment'
	| 'Paid'
	| 'Processing'
	| 'Shipped'
	| 'Delivered'
	| 'Completed'
	| 'Canceled'
	| 'Refunded';

export interface Order {
	id: string;
	buyerId: string;
	buyerEmail: string;
	status: OrderStatus;
	shippingAddress: ShippingAddress;
	items: OrderItem[];
	totalPrice: number;
	trackingNumber: string | null;
	createdAt: string;
	updatedAt: string;
}

export interface OrderItem {
	productId: string;
	productName: string;
	sizeName: string | null;
	colorName: string | null;
	materialName: string | null;
	unitPrice: number;
	quantity: number;
	personalization: string | null;
	imageUrl: string | null;
}

export interface OrderStatusUpdate {
	status: string;
}

// ─── Payment Types ───────────────────────────────────────────────────────────

export type PaymentProvider = 'Stripe' | 'LiqPay' | 'PayPal';

export type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed' | 'Refunded';

export interface PaymentRecord {
	id: string;
	orderId: string;
	amount: number;
	currency: string;
	provider: PaymentProvider;
	status: PaymentStatus;
	createdAt: string;
	completedAt: string | null;
}

export interface CreatePaymentRequest {
	orderId: string;
	provider: PaymentProvider;
	returnUrl: string;
}

export interface PaymentSession {
	sessionId: string;
	paymentUrl: string;
}

// ─── Common Types ────────────────────────────────────────────────────────────

export interface PaginatedResponse<T> {
	items: T[];
	totalCount: number;
	page: number;
	pageSize: number;
	totalPages: number;
}

export interface ApiError {
	message: string;
	errors?: Record<string, string[]>;
}

export interface PriceRange {
	minPrice: number;
	maxPrice: number;
}
