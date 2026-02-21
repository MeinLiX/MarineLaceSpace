import { api } from '$api/client';
import type {
	Category,
	Shop,
	Product,
	ProductDetail,
	ProductPhoto,
	ProductReview,
	ReviewSummary,
	PaginatedResponse,
	ProductInventoryItem,
	Size,
	Color,
	Material,
} from '$types';

// ─── Categories ──────────────────────────────────────────────────────────────

export async function getCategoryTree(): Promise<Category[]> {
	return api.get<Category[]>('/categories/tree');
}

export async function getCategoryById(id: string): Promise<Category> {
	return api.get<Category>(`/categories/${id}`);
}

export async function getCategoryByPath(path: string): Promise<Category> {
	return api.get<Category>('/categories/path', { path });
}

export async function createCategory(data: { name: string; parentCategoryId?: string }): Promise<Category> {
	return api.post<Category>('/categories', data);
}

export async function updateCategory(id: string, data: { name: string }): Promise<Category> {
	return api.put<Category>(`/categories/${id}`, data);
}

export async function deleteCategory(id: string): Promise<void> {
	return api.delete<void>(`/categories/${id}`);
}

// ─── Shops ───────────────────────────────────────────────────────────────────

export async function getShops(params?: { page?: number; pageSize?: number; search?: string }): Promise<PaginatedResponse<Shop>> {
	return api.get<PaginatedResponse<Shop>>('/shops', params as Record<string, string | number | boolean | undefined>);
}

export async function getShopById(id: string): Promise<Shop> {
	return api.get<Shop>(`/shops/${id}`);
}

export async function getShopBySlug(slug: string): Promise<Shop> {
	return api.get<Shop>(`/shops/slug/${slug}`);
}

export async function getShopsByUser(userId: string): Promise<Shop[]> {
	return api.get<Shop[]>(`/users/${userId}/shops`);
}

export async function getMyShops(): Promise<Shop[]> {
	return api.get<Shop[]>('/shops/my');
}

export async function createShop(data: { name: string; description: string }): Promise<Shop> {
	return api.post<Shop>('/shops', data);
}

export async function updateShop(id: string, data: Partial<{ name: string; description: string; logoUrl: string; bannerUrl: string }>): Promise<Shop> {
	return api.put<Shop>(`/shops/${id}`, data);
}

export async function deleteShop(id: string): Promise<void> {
	return api.delete<void>(`/shops/${id}`);
}

// ─── Products ────────────────────────────────────────────────────────────────

export interface ProductFilters {
	page?: number;
	pageSize?: number;
	search?: string;
	categoryId?: string;
	shopId?: string;
	minPrice?: number;
	maxPrice?: number;
	sortBy?: string;
	sortOrder?: 'asc' | 'desc';
	sizeId?: string;
	colorId?: string;
	materialId?: string;
	status?: string;
}

export async function getProducts(filters?: ProductFilters): Promise<PaginatedResponse<Product>> {
	return api.get<PaginatedResponse<Product>>('/products/active', filters as Record<string, string | number | boolean | undefined>);
}

export async function getAdminProducts(filters?: ProductFilters): Promise<PaginatedResponse<Product>> {
	return api.get<PaginatedResponse<Product>>('/products/admin', filters as Record<string, string | number | boolean | undefined>);
}

export async function getProductById(id: string): Promise<ProductDetail> {
	return api.get<ProductDetail>(`/products/${id}`);
}

export async function getProductsBatch(ids: string[]): Promise<ProductDetail[]> {
	return api.post<ProductDetail[]>('/products/batch', ids);
}

export async function getProductsByShop(shopId: string, filters?: ProductFilters): Promise<PaginatedResponse<Product>> {
	return api.get<PaginatedResponse<Product>>(`/shops/${shopId}/products`, filters as Record<string, string | number | boolean | undefined>);
}

export async function getProductsByCategory(categoryId: string, filters?: ProductFilters): Promise<PaginatedResponse<Product>> {
	return api.get<PaginatedResponse<Product>>(`/categories/${categoryId}/products`, filters as Record<string, string | number | boolean | undefined>);
}

export async function createProduct(shopId: string, data: Partial<ProductDetail>): Promise<ProductDetail> {
	return api.post<ProductDetail>(`/shops/${shopId}/products`, data);
}

export async function updateProduct(id: string, data: Partial<ProductDetail>): Promise<ProductDetail> {
	return api.put<ProductDetail>(`/products/${id}`, data);
}

export async function deleteProduct(id: string): Promise<void> {
	return api.delete<void>(`/products/${id}`);
}

// ─── Inventory ───────────────────────────────────────────────────────────────

export async function updateProductInventory(productId: string, inventory: ProductInventoryItem[]): Promise<void> {
	return api.put<void>(`/products/${productId}/inventory`, { items: inventory });
}

// ─── Images ──────────────────────────────────────────────────────────────────

export async function getProductImages(productId: string): Promise<ProductPhoto[]> {
	return api.get<ProductPhoto[]>(`/product-images/${productId}/images`);
}

export async function addProductImage(shopId: string, productId: string, data: { url: string; title?: string; isMain?: boolean }): Promise<ProductPhoto> {
	const params = new URLSearchParams();
	params.set('url', data.url);
	if (data.title) params.set('title', data.title);
	params.set('isMain', String(data.isMain ?? false));
	return api.post<ProductPhoto>(`/shops/${shopId}/products/${productId}/images?${params.toString()}`);
}

export async function deleteProductImage(shopId: string, productId: string, imageId: string): Promise<void> {
	return api.delete<void>(`/shops/${shopId}/products/${productId}/images/${imageId}`);
}

// ─── Reviews ─────────────────────────────────────────────────────────────────

export async function getProductReviews(productId: string, params?: { page?: number; pageSize?: number }): Promise<PaginatedResponse<ProductReview>> {
	return api.get<PaginatedResponse<ProductReview>>(`/products/${productId}/reviews`, params as Record<string, string | number | boolean | undefined>);
}

export async function getReviewSummary(productId: string): Promise<ReviewSummary> {
	return api.get<ReviewSummary>(`/products/${productId}/reviews/summary`);
}

export async function createReview(productId: string, data: { rating: number; title?: string; text: string }): Promise<ProductReview> {
	return api.post<ProductReview>(`/products/${productId}/reviews`, data);
}

export async function deleteReview(productId: string, reviewId: string): Promise<void> {
	return api.delete<void>(`/products/${productId}/reviews/${reviewId}`);
}

export async function getReviews(params?: { page?: number; pageSize?: number; rating?: number }): Promise<PaginatedResponse<ProductReview>> {
	return api.get<PaginatedResponse<ProductReview>>('/reviews', params as Record<string, string | number | boolean | undefined>);
}

// ─── Dictionaries: Sizes ─────────────────────────────────────────────────────

export async function getSizes(): Promise<Size[]> {
	return api.get<Size[]>('/sizes');
}

export async function createSize(data: { name: string; gender: string }): Promise<Size> {
	return api.post<Size>('/sizes', data);
}

export async function updateSize(id: string, data: { name: string; gender: string }): Promise<Size> {
	return api.put<Size>(`/sizes/${id}`, data);
}

export async function deleteSize(id: string): Promise<void> {
	return api.delete<void>(`/sizes/${id}`);
}

// ─── Dictionaries: Colors ────────────────────────────────────────────────────

export async function getColors(): Promise<Color[]> {
	return api.get<Color[]>('/colors');
}

export async function createColor(data: { name: string; hexCode: string }): Promise<Color> {
	return api.post<Color>('/colors', data);
}

export async function updateColor(id: string, data: { name: string; hexCode: string }): Promise<Color> {
	return api.put<Color>(`/colors/${id}`, data);
}

export async function deleteColor(id: string): Promise<void> {
	return api.delete<void>(`/colors/${id}`);
}

// ─── Dictionaries: Materials ─────────────────────────────────────────────────

export async function getMaterials(): Promise<Material[]> {
	return api.get<Material[]>('/materials');
}

export async function createMaterial(data: { name: string; imageUrl?: string }): Promise<Material> {
	return api.post<Material>('/materials', data);
}

export async function updateMaterial(id: string, data: { name: string; imageUrl?: string }): Promise<Material> {
	return api.put<Material>(`/materials/${id}`, data);
}

export async function deleteMaterial(id: string): Promise<void> {
	return api.delete<void>(`/materials/${id}`);
}
