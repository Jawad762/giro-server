using System.Text.Json.Serialization;

public class RideRequestInfoDto
{
    [JsonPropertyName("riderName")]
    public string RiderName { get; set; }

    [JsonPropertyName("riderId")]
    public int RiderId { get; set; }

    [JsonPropertyName("riderProfilePicture")]
    public string RiderProfilePicture { get; set; }

    [JsonPropertyName("location")]
    public decimal[] Location { get; set; }

    [JsonPropertyName("destination")]
    public decimal[] Destination { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
