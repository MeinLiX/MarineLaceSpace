using System.Text;
using System.Text.Json;

namespace ApiGateway.WebHost.Aggregators.Checkout
{
    public interface ICheckoutAggregator
    {
        Task<CheckoutResultDto> ProcessCheckoutAsync(CheckoutRequestDto request);
    }

    public class CheckoutRequestDto
    {
        public int UserId { get; set; }
        public int BasketId { get; set; }
        public ShippingAddressDto ShippingAddress { get; set; }
        public PaymentInfoDto PaymentInfo { get; set; }
    }

    public class ShippingAddressDto
    {
        public string FullName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class PaymentInfoDto
    {
        public string PaymentMethod { get; set; }
        public string PaymentProviderToken { get; set; }
    }

    public class CheckoutResultDto
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string PaymentStatus { get; set; }
        public string RedirectUrl { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class CheckoutAggregator : ICheckoutAggregator
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CheckoutAggregator> _logger;

        public CheckoutAggregator(IHttpClientFactory httpClientFactory, ILogger<CheckoutAggregator> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<CheckoutResultDto> ProcessCheckoutAsync(CheckoutRequestDto request)
        {
            var result = new CheckoutResultDto();

            try
            {
                // 1. Отримання даних кошика
                var basketClient = _httpClientFactory.CreateClient("basket-api");
                var basketResponse = await basketClient.GetAsync($"/api/basket/{request.BasketId}");
                basketResponse.EnsureSuccessStatusCode();


                // 2. Створення замовлення
                var orderClient = _httpClientFactory.CreateClient("order-api");
                var orderRequest = new
                {
                    request.UserId,
                    request.BasketId,
                    request.ShippingAddress
                };

                var orderContent = new StringContent(
                    JsonSerializer.Serialize(orderRequest),
                    Encoding.UTF8,
                    "application/json");

                var orderResponse = await orderClient.PostAsync("/api/orders", orderContent);
                orderResponse.EnsureSuccessStatusCode();

                var orderResult = await JsonSerializer.DeserializeAsync<OrderCreationResultDto>(
                    await orderResponse.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                result.OrderId = orderResult.OrderId;


                // 3. Обробка платежу
                var paymentClient = _httpClientFactory.CreateClient("payment-api");
                var paymentRequest = new
                {
                    orderResult.OrderId,
                    Amount = orderResult.TotalAmount,
                    Currency = "UAH",
                    request.PaymentInfo.PaymentMethod,
                    request.PaymentInfo.PaymentProviderToken
                };

                var paymentContent = new StringContent(
                    JsonSerializer.Serialize(paymentRequest),
                    Encoding.UTF8,
                    "application/json");

                var paymentResponse = await paymentClient.PostAsync("/api/payments", paymentContent);
                paymentResponse.EnsureSuccessStatusCode();

                var paymentResult = await JsonSerializer.DeserializeAsync<PaymentResultDto>(
                    await paymentResponse.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                result.PaymentId = paymentResult.PaymentId;
                result.PaymentStatus = paymentResult.Status;
                result.RedirectUrl = paymentResult.RedirectUrl;
                result.Success = true;

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error processing checkout request");
                result.Success = false;
                result.Errors.Add("Помилка під час обробки замовлення: " + ex.Message);
                return result;
            }
        }

        private class OrderCreationResultDto
        {
            public string OrderId { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private class PaymentResultDto
        {
            public string PaymentId { get; set; }
            public string Status { get; set; }
            public string RedirectUrl { get; set; }
        }
    }
}