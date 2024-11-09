using Giro.Api.Models;
using System.Text.Json.Serialization;

namespace Giro.Api.Dtos.Users
{
    public class UpdateUserDto
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("profilePicture")]
        public string? ProfilePicture { get; set; }

        [JsonPropertyName("vehicle")]
        public VehicleDto? Vehicle { get; set; }
    }

    public class VehicleDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("licenseNumber")]
        public string LicenseNumber { get; set; }
    }

}
