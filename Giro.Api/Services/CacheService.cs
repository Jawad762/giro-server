using Giro.Api.Interfaces;
using Giro.Api.Models;

namespace Giro.Api.Services
{
    public class CacheService : ICacheService
    {
        public Dictionary<int, DriverCacheModel> DriverLocations { get; set; } = new Dictionary<int, DriverCacheModel>();
        public Dictionary<int, string> RiderConnections { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, List<int>> RideRequests { get; set; } = new Dictionary<int, List<int>>();
    }
}
