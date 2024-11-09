using Giro.Api.Models;

namespace Giro.Api.Dtos.Auth
{
    public class RegisterRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public VehicleModel? Vehicle { get; set; }
    }

}
