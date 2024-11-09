using Giro.Api.Dtos.Checkout;
using Giro.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Giro.Api.Controllers
{
    [Route("api/checkout")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly string _stripeApiKey;

        public CheckoutController(IConfiguration config) {
            _stripeApiKey = config["Stripe:ApiKey"];
        }

        [HttpPost("create-session")]
        public IActionResult CreateSession([FromBody] CheckoutSessionRequestDto req)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeApiKey;

                var options = new SessionCreateOptions
                {
                    SuccessUrl = $"{req.ReturnUrl}&session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = req.ReturnUrl,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Quantity = 1,
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "usd",
                                UnitAmount = req.Price,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Giro Ride, {req.RideType}"
                                }
                            },
                        },
                    },
                    Mode = "payment",
                };

                var service = new SessionService();
                var session = service.Create(options);

                return Ok(new ApiSuccessResponse<Session>
                {
                    Status = "success",
                    Data = session
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "Something went wrong"
                });
            }
        }

        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            try
            {
                var sessionId = HttpContext.Request.Query["session_id"].ToString();
                if (sessionId == null)
                {
                    throw new Exception();
                }
                StripeConfiguration.ApiKey = _stripeApiKey;

                var service = new SessionService();
                var sessionInfo = service.Get(sessionId, new SessionGetOptions
                {
                    Expand = new List<string> { "line_items" }
                });

                return Ok(new ApiSuccessResponse<Session>
                {
                    Status = "success",
                    Data = sessionInfo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "Something went wrong"
                });
            }
        }

    }
}
