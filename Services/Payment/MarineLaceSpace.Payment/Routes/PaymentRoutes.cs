using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Payment;

namespace Payment.WebHost.Routes;

public static class PaymentRoutes
{
    public static void MapPaymentRoutes(this IEndpointRouteBuilder app)
    {
        var paymentsGroup = app.MapGroup("/api/payments")
            .WithTags("Payments");

        paymentsGroup.MapPost("/checkout", PaymentHandlers.CreateCheckoutHandler)
            .WithSummary("Create a payment checkout session")
            .RequireAuthorization();

        paymentsGroup.MapGet("/{id}", PaymentHandlers.GetPaymentByIdHandler)
            .WithSummary("Get payment by ID")
            .RequireAuthorization();

        paymentsGroup.MapPost("/webhook/{provider}", PaymentHandlers.WebhookHandler)
            .WithSummary("Payment provider webhook");

        paymentsGroup.MapPost("/{id}/refund", PaymentHandlers.RefundHandler)
            .WithSummary("Refund a payment")
            .RequireAuthorization("AdminOnly");

        paymentsGroup.MapGet("/{id}/history", PaymentHandlers.GetPaymentHistoryHandler)
            .WithSummary("Get payment history")
            .RequireAuthorization();

        app.MapGroup("/api/orders/{orderId}/payments")
            .WithTags("Payments")
            .MapGet("/", PaymentHandlers.GetPaymentsByOrderHandler)
            .WithSummary("Get payments for an order")
            .RequireAuthorization();
    }
}
