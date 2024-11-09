namespace Giro.Api.Dtos.Checkout
{
    public class CheckoutDto
    {
    }

    public class CheckoutSessionRequestDto
    {
        public int Price { get; set; }
        public string RideType { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class CheckSessionRequestDto
    {
        public string SessionId { get; set; }
    }
}
