using Giro.Api.Models;

namespace Giro.Api.Interfaces
{
    public interface ICacheService
    {
        Dictionary<int, DriverCacheModel> DriverLocations { get; set; }
        Dictionary<int, string> RiderConnections { get; set; }
        Dictionary<int, List<int>> RideRequests { get; set; }
    }
}
