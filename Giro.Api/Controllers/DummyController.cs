using Giro.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Giro.Api.Controllers
{
    [ApiController]
    [Route("api/helloworld")]
    public class DummyController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public DummyController(IEmailService emailService)
        { 
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult HelloWorld()
        {
            return Ok("Hello World");
        }
    }
}
