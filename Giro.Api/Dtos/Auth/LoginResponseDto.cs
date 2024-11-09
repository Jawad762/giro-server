using Giro.Api.Models;

namespace Giro.Api.Dtos.Auth
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public string Role { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class LoginResponseDto
    {
        public JwtModel Jwt { get; set; }
        public UserDto User { get; set; }
        public VehicleModel? Vehicle { get; set; }
    }
}
