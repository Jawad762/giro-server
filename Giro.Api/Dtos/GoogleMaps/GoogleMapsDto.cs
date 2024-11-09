using System.Text.Json.Serialization;

namespace Giro.Api.Dtos.GoogleMaps
{
    public class AutocompleteResponse
    {
        public List<Prediction> Predictions { get; set; }
    }

    public class Prediction
    {
        public string place_id { get; set; }
        public string Description { get; set; }
    }

    public class PlaceIdResponse
    {
        public ResultDto Result { get; set; }
    }

    public class ResultDto
    {
        public Geometry Geometry { get; set; }
    }

    public class Geometry
    {
        public LocationDto Location { get; set; }

    }

    public class LocationDto
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }

    public class GoogleDistanceMatrixResponseDto
    {
        [JsonPropertyName("destination_addresses")]
        public List<string> DestinationAddresses { get; set; }

        [JsonPropertyName("origin_addresses")]
        public List<string> OriginAddresses { get; set; }
        public List<RowDto> Rows { get; set; }
        public string Status { get; set; }
    }

    public class RowDto
    {
        public List<ElementDto> Elements { get; set; }
    }

    public class ElementDto
    {
        public DistanceDto Distance { get; set; }
        public DurationDto Duration { get; set; }
        public string Status { get; set; }
    }

    public class DistanceDto
    {
        public string Text { get; set; }
        public long Value { get; set; }
    }

    public class DurationDto
    {
        public string Text { get; set; }
        public long Value { get; set; }
    }

    public class DriverConnection : LocationDto
    {
        public int DriverId { get; set; }
        public double Distance { get; set; }
    }

}
