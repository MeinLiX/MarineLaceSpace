using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;

namespace Catalog.WebHost.Routes;

public static class ReviewRoutes
{
    public static void MapReviewRoutes(this IEndpointRouteBuilder app)
    {
        var reviewsGroup = app.MapGroup("/api/products/{productId}/reviews")
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
