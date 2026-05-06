using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;

namespace Catalog.WebHost.Routes;

public static class ReviewRoutes
{
    public static void MapReviewRoutes(this IEndpointRouteBuilder app)
    {
        // Standalone reviews endpoint (admin panel — all reviews with pagination)
        var standaloneGroup = app.MapGroup("/api/reviews")
            .WithTags("Reviews");

        standaloneGroup.MapGet("/", ReviewHandlers.GetAllReviewsHandler)
            .WithSummary("Get all reviews with pagination and optional rating filter");

        // Product-scoped reviews — accessed via /api/products/{rest} → /api/v1/products/{rest}
        var reviewsGroup = app.MapGroup("/api/v1/products/{productId}/reviews")
            .WithTags("Reviews");

        reviewsGroup.MapGet("/", ReviewHandlers.GetProductReviewsHandler)
            .WithSummary("Get reviews for a product");

        reviewsGroup.MapGet("/summary", ReviewHandlers.GetReviewSummaryHandler)
            .WithSummary("Get review summary (average rating, count)");

        reviewsGroup.MapPost("/", ReviewHandlers.CreateReviewHandler)
            .WithSummary("Create a review for a product");

        reviewsGroup.MapDelete("/{reviewId}", ReviewHandlers.DeleteReviewHandler)
            .WithSummary("Delete a review")
            .RequireAuthorization("AdminOnly");
    }
}
