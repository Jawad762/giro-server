using System.Text.Json.Serialization;

namespace Giro.Api.Dtos.SocketEvents
{
    public class RideResponseInfoDto
    {
        [JsonPropertyName("driverName")]
        public string DriverName { get; set; }

        [JsonPropertyName("driverProfilePicture")]
        public string DriverProfilePicture { get; set; }

        [JsonPropertyName("driverId")]
        public int DriverId { get; set; }

        [JsonPropertyName("riderId")]
        public int RiderId { get; set; }

        [JsonPropertyName("car")]
        public string Car { get; set; }

        [JsonPropertyName("location")]
        public decimal[] Location { get; set; }
    }
}
