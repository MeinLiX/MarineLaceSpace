using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Payment;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Payment;
using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Database.Payment;
using MarineLaceSpace.Models.Events;
using MarineLaceSpace.Models.Routes;
using Microsoft.EntityFrameworkCore;
using Payment.WebHost.Data;
using System.Security.Claims;
using RefundStatus = MarineLaceSpace.Enumerations.RefundStatus;

namespace Payment.WebHost.Routes;

internal class PaymentHandlers
{
    private record PaymentServices : BasicRouteServices
    {
        public required PaymentDbContext DbContext { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<PaymentHandlers> Logger { get; init; }
        public IEventBus? EventBus { get; init; }
    }

    internal static Delegate CreateCheckoutHandler =>
        async (CreatePaymentRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreatePaymentRequest, PaymentServices>(request, sp,
                async (services) =>
                {
                    var userId = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                    var email = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

                    // Check if a payment record already exists for this order (created by OrderCreatedEvent)
                    var existingPayment = await services.DbContext.Payments
                        .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.StatusId == PaymentStatus.Pending.Id);

                    if (existingPayment != null)
                    {
                        existingPayment.ProviderId = request.ProviderId;
                        existingPayment.BuyerEmail = email;
                        await services.DbContext.SaveChangesAsync();

                        services.Logger.LogInformation("Existing payment {PaymentId} found for order {OrderId}", existingPayment.Id, existingPayment.OrderId);
                        return Results.Ok(MapPaymentToResponse(existingPayment));
                    }

                    var payment = new PaymentRecord
                    {
                        Id = Guid.NewGuid().ToString(),
                        OrderId = request.OrderId,
                        ProviderId = request.ProviderId,
                        StatusId = PaymentStatus.Pending.Id,
                        BuyerEmail = email,
                        Amount = request.Amount
                    };

                    await services.DbContext.Payments.AddAsync(payment);

                    await services.DbContext.PaymentStatusHistory.AddAsync(new PaymentStatusHistory
                    {
                        PaymentId = payment.Id,
                        OldStatusId = 0,
                        NewStatusId = PaymentStatus.Pending.Id,
                        Note = "Payment created via checkout."
                    });

                    await services.DbContext.SaveChangesAsync();

                    services.Logger.LogInformation("Payment {PaymentId} created for order {OrderId}", payment.Id, payment.OrderId);
                    return Results.Ok(MapPaymentToResponse(payment));
                });

    internal static Delegate GetPaymentByIdHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PaymentServices>(sp, async (services) =>
            {
                var payment = await services.DbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (payment == null) return Results.NotFound(RESTResult.Fail("Payment not found."));
                return Results.Ok(MapPaymentToResponse(payment));
            });

    internal static Delegate GetPaymentsByOrderHandler =>
        async (string orderId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PaymentServices>(sp, async (services) =>
            {
                var payments = await services.DbContext.Payments
                    .Where(p => p.OrderId == orderId)
                    .AsNoTracking()
                    .ToListAsync();
                return Results.Ok(payments.Select(MapPaymentToResponse));
            });

    internal static Delegate WebhookHandler =>
        async (string provider, HttpContext httpContext, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PaymentServices>(sp, async (services) =>
            {
                // Simulate webhook processing - in production, verify signature per provider
                var body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                services.Logger.LogInformation("Received webhook from {Provider}: {Body}", provider, body);

                // For demo: mark latest pending payment as succeeded
                var pendingPayment = await services.DbContext.Payments
                    .FirstOrDefaultAsync(p => p.StatusId == PaymentStatus.Pending.Id);

                if (pendingPayment != null)
                {
                    // Simulate: 80% success, 20% failure for demo purposes
                    var isSuccess = Random.Shared.Next(100) < 80;

                    if (isSuccess)
                    {
                        var oldStatusId = pendingPayment.StatusId;
                        pendingPayment.StatusId = PaymentStatus.Succeeded.Id;
                        pendingPayment.CompletedAt = DateTime.UtcNow;
                        pendingPayment.ProviderPaymentId = $"{provider}_{Guid.NewGuid():N}";

                        await services.DbContext.PaymentStatusHistory.AddAsync(new PaymentStatusHistory
                        {
                            PaymentId = pendingPayment.Id,
                            OldStatusId = oldStatusId,
                            NewStatusId = PaymentStatus.Succeeded.Id,
                            Note = $"Payment succeeded via {provider} webhook."
                        });

                        await services.DbContext.SaveChangesAsync();

                        if (services.EventBus != null)
                        {
                            await services.EventBus.PublishAsync(new PaymentSucceededEvent
                            {
                                PaymentId = pendingPayment.Id,
                                OrderId = pendingPayment.OrderId,
                                Amount = pendingPayment.Amount,
                                BuyerEmail = pendingPayment.BuyerEmail
                            });
                        }

                        services.Logger.LogInformation("Payment {PaymentId} succeeded for order {OrderId}", pendingPayment.Id, pendingPayment.OrderId);
                    }
                    else
                    {
                        var oldStatusId = pendingPayment.StatusId;
                        pendingPayment.StatusId = PaymentStatus.Failed.Id;
                        pendingPayment.CompletedAt = DateTime.UtcNow;

                        await services.DbContext.PaymentStatusHistory.AddAsync(new PaymentStatusHistory
                        {
                            PaymentId = pendingPayment.Id,
                            OldStatusId = oldStatusId,
                            NewStatusId = PaymentStatus.Failed.Id,
                            Note = $"Payment failed via {provider} webhook."
                        });

                        await services.DbContext.SaveChangesAsync();

                        if (services.EventBus != null)
                        {
                            await services.EventBus.PublishAsync(new PaymentFailedEvent
                            {
                                PaymentId = pendingPayment.Id,
                                OrderId = pendingPayment.OrderId,
                                Reason = "Payment declined by provider.",
                                BuyerEmail = pendingPayment.BuyerEmail
                            });
                        }

                        services.Logger.LogWarning("Payment {PaymentId} failed for order {OrderId}", pendingPayment.Id, pendingPayment.OrderId);
                    }
                }

                return Results.Ok();
            });

    internal static Delegate RefundHandler =>
        async (string id, RefundPaymentRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<RefundPaymentRequest, PaymentServices>(request, sp,
                async (services) =>
                {
                    var payment = await services.DbContext.Payments.FindAsync(id);
                    if (payment == null) return Results.NotFound(RESTResult.Fail("Payment not found."));

                    if (payment.StatusId != PaymentStatus.Succeeded.Id && payment.StatusId != PaymentStatus.PartiallyRefunded.Id)
                        return Results.BadRequest(RESTResult.Fail("Only succeeded or partially refunded payments can be refunded."));

                    var refundAmount = request.Amount ?? payment.Amount;

                    // Calculate already refunded total
                    var alreadyRefunded = await services.DbContext.Refunds
                        .Where(r => r.PaymentId == id && (r.StatusId == RefundStatus.Completed.Id || r.StatusId == RefundStatus.Approved.Id))
                        .SumAsync(r => r.Amount);

                    if (refundAmount + alreadyRefunded > payment.Amount)
                        return Results.BadRequest(RESTResult.Fail("Refund amount exceeds the remaining refundable amount."));

                    var refund = new RefundRecord
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentId = id,
                        Amount = refundAmount,
                        Reason = request.Reason,
                        StatusId = RefundStatus.Completed.Id,
                        InitiatedBy = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
                        CompletedAt = DateTime.UtcNow
                    };
                    await services.DbContext.Refunds.AddAsync(refund);

                    var oldStatusId = payment.StatusId;
                    var isFullRefund = refundAmount + alreadyRefunded >= payment.Amount;
                    payment.StatusId = isFullRefund ? PaymentStatus.Refunded.Id : PaymentStatus.PartiallyRefunded.Id;
                    payment.CompletedAt = DateTime.UtcNow;

                    await services.DbContext.PaymentStatusHistory.AddAsync(new PaymentStatusHistory
                    {
                        PaymentId = id,
                        OldStatusId = oldStatusId,
                        NewStatusId = payment.StatusId,
                        Note = isFullRefund
                            ? $"Full refund of {refundAmount}. Reason: {request.Reason}"
                            : $"Partial refund of {refundAmount}. Reason: {request.Reason}"
                    });

                    await services.DbContext.SaveChangesAsync();

                    if (services.EventBus != null)
                    {
                        await services.EventBus.PublishAsync(new PaymentRefundedEvent
                        {
                            PaymentId = payment.Id,
                            OrderId = payment.OrderId,
                            RefundId = refund.Id,
                            RefundAmount = refundAmount,
                            BuyerEmail = payment.BuyerEmail,
                            Reason = request.Reason
                        });
                    }

                    services.Logger.LogInformation("Payment {PaymentId} refunded {Amount} (RefundId: {RefundId})", id, refundAmount, refund.Id);
                    return Results.Ok(MapPaymentToResponse(payment));
                });

    internal static Delegate GetPaymentHistoryHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PaymentServices>(sp, async (services) =>
            {
                var payment = await services.DbContext.Payments
                    .Include(p => p.StatusHistory)
                    .Include(p => p.Refunds)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null) return Results.NotFound(RESTResult.Fail("Payment not found."));

                var response = new PaymentHistoryResponse
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Status = PaymentStatus.FromId<PaymentStatus>(payment.StatusId)?.Name ?? "Unknown",
                    StatusChanges = payment.StatusHistory
                        .OrderBy(h => h.ChangedAt)
                        .Select(h => new StatusChangeEntry
                        {
                            OldStatus = PaymentStatus.FromId<PaymentStatus>(h.OldStatusId)?.Name ?? "None",
                            NewStatus = PaymentStatus.FromId<PaymentStatus>(h.NewStatusId)?.Name ?? "Unknown",
                            Note = h.Note,
                            ChangedAt = h.ChangedAt
                        }).ToList(),
                    Refunds = payment.Refunds
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new RefundEntry
                        {
                            Id = r.Id,
                            Amount = r.Amount,
                            Reason = r.Reason,
                            Status = RefundStatus.FromId<RefundStatus>(r.StatusId)?.Name ?? "Unknown",
                            CreatedAt = r.CreatedAt,
                            CompletedAt = r.CompletedAt
                        }).ToList()
                };

                return Results.Ok(response);
            });

    private static PaymentResponse MapPaymentToResponse(PaymentRecord p) => new()
    {
        Id = p.Id,
        OrderId = p.OrderId,
        Amount = p.Amount,
        Currency = p.Currency,
        Provider = PaymentProvider.FromId<PaymentProvider>(p.ProviderId)?.Name ?? "Unknown",
        Status = PaymentStatus.FromId<PaymentStatus>(p.StatusId)?.Name ?? "Unknown",
        ProviderPaymentId = p.ProviderPaymentId,
        CreatedAt = p.CreatedAt,
        CompletedAt = p.CompletedAt
    };
}
