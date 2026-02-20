using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
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

    internal static Delegate GetProductReviewsHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ReviewServices>(sp, async (services) =>
            {
                var reviews = await services.ReviewRepository.GetByProductIdAsync(productId);
                var response = reviews.Select(MapReviewToResponse);
                return Results.Ok(response);
            });

    internal static Delegate GetReviewSummaryHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ReviewServices>(sp, async (services) =>
            {
                var reviews = await services.ReviewRepository.GetByProductIdAsync(productId);
                var avg = await services.ReviewRepository.GetAverageRatingAsync(productId);
                return Results.Ok(new ReviewSummaryResponse
                {
                    AverageRating = avg,
                    TotalReviews = reviews.Count()
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
                        Comment = request.Comment,
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
        Comment = r.Comment,
        UserName = r.UserName,
        CreatedAt = r.CreatedAt,
        IsVerified = r.IsVerified
    };
}
