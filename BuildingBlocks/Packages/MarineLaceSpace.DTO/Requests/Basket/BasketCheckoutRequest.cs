using MarineLaceSpace.DTO.Common;

namespace MarineLaceSpace.DTO.Requests.Basket;

public class BasketCheckoutRequest
{
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
}
