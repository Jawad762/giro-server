using Giro.Api.Dtos.GoogleMaps;
using Giro.Api.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Giro.Api.Controllers
{
    [Controller]
    [Route("api/places")]
    public class PlacesController : ControllerBase
    {
        private readonly string _googleKey;

        public PlacesController(IConfiguration config) {
            _googleKey = config["GoogleApiKey"];
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetPlaces()
        {
            try
            {
                string input = HttpContext.Request.Query["input"];
                string countryCode = HttpContext.Request.Query["country"];
                if (String.IsNullOrEmpty(input) || String.IsNullOrEmpty(countryCode)) {
                    throw new Exception("Missing query params");
                }
                var options = new RestClientOptions("https://maps.googleapis.com");
                var client = new RestClient(options);
                var response = await client.GetJsonAsync<AutocompleteResponse>($"maps/api/place/autocomplete/json?input={input}&key={_googleKey}&components=country:{countryCode}");
                List<dynamic> places = new List<dynamic>();
                foreach (var prediction in response.Predictions)
                {
                    var latLongResponse = await client.GetJsonAsync<PlaceIdResponse>($"/maps/api/place/details/json?place_id={prediction.place_id}&key={_googleKey}");
                    places.Add(new
                    {
                        Description = prediction.Description,
                        Lat = latLongResponse.Result.Geometry.Location.Lat,
                        Long = latLongResponse.Result.Geometry.Location.Lng
                    });
                }

                return Ok(new ApiSuccessResponse<dynamic>
                {
                    Status = "success",
                    Data = places
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
