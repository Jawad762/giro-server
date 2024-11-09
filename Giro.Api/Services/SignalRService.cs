using Giro.Api.Dtos.GoogleMaps;
using Giro.Api.Dtos.SocketEvents;
using Giro.Api.Interfaces;
using Giro.Api.Models;
using Microsoft.AspNetCore.SignalR;
using RestSharp;
using System.Data;

namespace Giro.Api.Services
{
    public class SignalRService : Hub
    {

        private readonly string _googleKey;
        private readonly Dictionary<int, DriverCacheModel> _driverLocations;
        private readonly Dictionary<int, string> _riderConnections;
        private readonly Dictionary<int, List<int>> _rideRequests;

        public SignalRService(IConfiguration config, ICacheService cacheService)
        {
            _googleKey = config["GoogleApiKey"];
            _driverLocations = cacheService.DriverLocations;
            _riderConnections = cacheService.RiderConnections;
            _rideRequests = cacheService.RideRequests;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"];
            var role = httpContext.Request.Query["role"].ToString();

            if (role.Equals("rider"))
            {
                _riderConnections[int.Parse(userId)] = Context.ConnectionId;
            }
            else if (role.Equals("driver"))
            {
                _driverLocations[int.Parse(userId)] = new DriverCacheModel
                {
                    ConnectionId = Context.ConnectionId,
                };
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client {Context.ConnectionId} disconnected.");
            await base.OnDisconnectedAsync(exception);
        }

        public void SubscribeToLocation(int driverId, decimal lat, decimal @long)
        {
            _driverLocations[driverId] = new DriverCacheModel
            {
                ConnectionId = Context.ConnectionId,
                Lat = lat,
                Long = @long
            };
        }

        public void UnsubscribeFromLocation(int driverId)
        {
            _driverLocations.Remove(driverId);
        }

        public async Task RequestRide(RideRequestInfoDto rideInfo)
        {
            decimal userLat = rideInfo.Location[0];
            decimal userLong = rideInfo.Location[1];

            var destinations = new List<DriverConnection>();

            foreach (KeyValuePair<int, DriverCacheModel> entry in _driverLocations)
            {
                if (entry.Value == null) continue;
                decimal driverLat = entry.Value.Lat;
                decimal driverLong = entry.Value.Long;

                double distance = CalculateHaversineDistance(userLat, userLong, driverLat, driverLong);

                if (distance > 3) continue;

                destinations.Add(new DriverConnection
                {
                    Lat = driverLat,
                    Lng = driverLong,
                    Distance = distance,
                    DriverId = entry.Key
                });
            }

            if (!destinations.Any()) return;

            _rideRequests[rideInfo.RiderId] = new List<int>();

            var closestDrivers = destinations.OrderBy(d => d.Distance).Take(10).ToList();

            var destinationQuery = string.Join("|", closestDrivers.Select(d => $"{d.Lat},{d.Lng}"));

            var path = $"/maps/api/distancematrix/json?origins={userLat},{userLong}&destinations={destinationQuery}&mode=driving&key={_googleKey}";

            var options = new RestClientOptions("https://maps.googleapis.com");
            var client = new RestClient(options);
            var response = await client.GetJsonAsync<GoogleDistanceMatrixResponseDto>(path);

            var elements = response.Rows.FirstOrDefault()?.Elements;
            if (elements == null) return;

            for (int i = 0; i < elements.Count(); i++)
            {
                var element = elements[i];
                if (element.Status == "OK")
                {
                    var durationInMinutes = element.Duration.Value / 60;
                    var locationAddress = response.OriginAddresses.FirstOrDefault();
                    var destinationAddress = response.DestinationAddresses[i];

                    var extendedRideInfo = new
                    {
                        RiderName = rideInfo.RiderName,
                        RiderId = rideInfo.RiderId,
                        RiderProfilePicture = rideInfo.RiderProfilePicture,
                        Location = rideInfo.Location,
                        Destination = rideInfo.Destination,
                        Price = rideInfo.Price,
                        DurationInMinutes = durationInMinutes,
                        DistanceInKilometers = closestDrivers.ElementAt(i).Distance,
                        LocationAddress = locationAddress,
                        DestinationAddress = destinationAddress
                    };

                    var driverId = closestDrivers.ElementAt(i).DriverId;
                    var driverConnectionId = _driverLocations[driverId].ConnectionId;
                    await Clients.Client(driverConnectionId).SendAsync("RideRequest", extendedRideInfo);
                    _rideRequests[rideInfo.RiderId].Add(driverId);
                }
            }
        }

        public double CalculateHaversineDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371;
            var lat1Rad = ToRadians((double)lat1);
            var lon1Rad = ToRadians((double)lon1);
            var lat2Rad = ToRadians((double)lat2);
            var lon2Rad = ToRadians((double)lon2);

            var dLat = lat2Rad - lat1Rad;
            var dLon = lon2Rad - lon1Rad;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }

        public async Task ConfirmRide(RideResponseInfoDto rideInfo)
        {
            var riderConnection = _riderConnections[rideInfo.RiderId];
            await Clients.Client(riderConnection).SendAsync("AcceptRide", rideInfo);

            var driverIds = _rideRequests[rideInfo.RiderId];

            foreach (var driverId in driverIds)
            {
                if (driverId != rideInfo.DriverId) {
                    var driverConnection = _driverLocations[driverId].ConnectionId;
                    await Clients.Client(driverConnection).SendAsync("CancelRide", rideInfo.RiderId);
                    driverIds.Remove(driverId);
                }
            }
        }

        public async Task CancelRide(int riderId)
        {
            var riderConnection = _riderConnections[riderId];
            await Clients.Client(riderConnection).SendAsync("CancelRide", riderId);

            var driverIds = _rideRequests[riderId];

            foreach (var driverId in driverIds)
            {
                var driverConnection = _driverLocations[driverId].ConnectionId;
                await Clients.Client(driverConnection).SendAsync("CancelRide", riderId);
            }
        }

        public async Task DriverLocationChange(decimal lat, decimal @long, int riderId, int driverId)
        {
            var riderConnection = _riderConnections[riderId];

            _driverLocations[driverId] = new DriverCacheModel
            {
                ConnectionId = Context.ConnectionId,
                Lat = lat,
                Long = @long
            };

            if (riderConnection == null) return;

            await Clients.Client(riderConnection).SendAsync("LocationChange", new { Lat = lat, Long = @long });
        }

    }
}
