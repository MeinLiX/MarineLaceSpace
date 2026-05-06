using BB.Common.Routes;
using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Catalog.WebHost.Routes;

internal class ReviewHandlers
{
    private record ReviewServices : BasicRouteServices
    {
        public required IProductReviewRepository ReviewRepository { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<ReviewHandlers> Logger { get; init; }
    }

    internal static Delegate GetAllReviewsHandler =>
        async (int? page, int? pageSize, int? rating, IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<CatalogDbContext>();

            var clampedPage = Math.Max(1, page ?? 1);
            var clampedSize = Math.Clamp(pageSize ?? 20, 1, 50);

            IQueryable<ProductReview> query = dbContext.ProductReviews.AsNoTracking();

            if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
                query = query.Where(r => (int)r.Rating == rating.Value);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((clampedPage - 1) * clampedSize)
                .Take(clampedSize)
                .Select(r => MapReviewToResponse(r))
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / clampedSize);

            return Results.Ok(RESTResult<object>.Success(new
            {
                Items = reviews,
                TotalCount = totalCount,
                Page = clampedPage,
                PageSize = clampedSize,
                TotalPages = totalPages
            }));
        };

    internal static Delegate GetProductReviewsHandler =>
        async (string productId, int? page, int? pageSize, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ReviewServices>(sp, async (services) =>
            {
                var allReviews = await services.ReviewRepository.GetByProductIdAsync(productId);
                var reviewsList = allReviews.ToList();
                var totalCount = reviewsList.Count;

                var clampedPage = Math.Max(1, page ?? 1);
                var clampedSize = Math.Clamp(pageSize ?? 10, 1, 50);
                var totalPages = (int)Math.Ceiling((double)totalCount / clampedSize);

                var paged = reviewsList
                    .Skip((clampedPage - 1) * clampedSize)
                    .Take(clampedSize)
                    .Select(MapReviewToResponse)
                    .ToList();

                return Results.Ok(new
                {
                    Items = paged,
                    TotalCount = totalCount,
                    Page = clampedPage,
                    PageSize = clampedSize,
                    TotalPages = totalPages
                });
            });

    internal static Delegate GetReviewSummaryHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ReviewServices>(sp, async (services) =>
            {
                var reviews = await services.ReviewRepository.GetByProductIdAsync(productId);
                var reviewsList = reviews.ToList();
                var avg = await services.ReviewRepository.GetAverageRatingAsync(productId);

                var distribution = new Dictionary<int, int>
                {
                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
                };
                foreach (var r in reviewsList)
                {
                    var star = Math.Clamp((int)Math.Round(r.Rating), 1, 5);
                    distribution[star]++;
                }

                return Results.Ok(new ReviewSummaryResponse
                {
                    AverageRating = avg,
                    TotalCount = reviewsList.Count,
                    Distribution = distribution
                });
            });

    internal static Delegate CreateReviewHandler =>
        async (string productId, CreateReviewRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateReviewRequest, ReviewServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext;
                    var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var review = new ProductReview
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = productId,
                        Rating = request.Rating,
                        Comment = !string.IsNullOrEmpty(request.Text) ? request.Text : request.Comment ?? string.Empty,
                        UserId = userId ?? string.Empty,
                        UserName = request.UserName ?? httpContext?.User.FindFirstValue(ClaimTypes.Email) ?? "Guest",
                        ContactInfo = request.ContactInfo ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        IsVerified = !string.IsNullOrEmpty(userId)
                    };

                    var created = await services.ReviewRepository.AddAsync(review);
                    return Results.Created($"/api/products/{productId}/reviews/{created.Id}", MapReviewToResponse(created));
                });

    internal static Delegate DeleteReviewHandler =>
        async (string productId, string reviewId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ReviewServices>(sp, async (services) =>
            {
                try
                {
                    await services.ReviewRepository.DeleteAsync(reviewId);
                    return Results.NoContent();
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    private static ReviewResponse MapReviewToResponse(ProductReview r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        Rating = r.Rating,
        Title = null,
        Text = r.Comment,
        UserId = string.IsNullOrEmpty(r.UserId) ? null : r.UserId,
        GuestName = r.UserName,
        CreatedAt = r.CreatedAt,
        IsVerifiedPurchase = r.IsVerified
    };
}
