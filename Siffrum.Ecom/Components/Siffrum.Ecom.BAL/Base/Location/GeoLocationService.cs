using AutoMapper;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.GeoLocation;
using System.Text.Json;

namespace Siffrum.Ecom.BAL.Base.Location
{
    public class GeoLocationService : SiffrumBalBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly APIConfiguration _apiConfiguration;

        public GeoLocationService(IMapper mapper,ApiDbContext context,IHttpClientFactory httpClientFactory,
            APIConfiguration apiConfiguration
            )
            : base(mapper, context)
        {
            _httpClientFactory = httpClientFactory;
            _apiConfiguration = apiConfiguration;
        }

        public async Task<UserAddressSM?> GetAddressFromLatLongAsync(
            double latitude, double longitude)
        {
            var client = _httpClientFactory.CreateClient();

            var apiKey = _apiConfiguration.GoogleCloudLocation.ApiKey;
            var url =
                $"https://maps.googleapis.com/maps/api/geocode/json" +
                $"?latlng={latitude},{longitude}&key={apiKey}";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Google Geocode Error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            

            var googleData =
                JsonSerializer.Deserialize<GoogleGeocodeResponse>(json);

            if (googleData?.Results == null || !googleData.Results.Any())
            {
                return new UserAddressSM();
            }

            
            string? pincode = null;
            string? city = null;
            string? state = null;
            string? country = null;
            string? address = null;
            var preferredTypes = new[] { "street_address", "premise", "route" };

            var bestResult = googleData.Results
                .FirstOrDefault(r => r.FormattedAddress != null && preferredTypes.Any(t => r.FormattedAddress.Contains(t)))
                ?? googleData.Results.First();
            if (bestResult.FormattedAddress.Contains(","))
            {
                address = bestResult.FormattedAddress.Substring(bestResult.FormattedAddress.IndexOf(",") + 1).Trim();
            }
            foreach (var result in googleData.Results)
            {
                if (result?.AddressComponents == null)
                    continue;
                foreach (var component in result.AddressComponents)
                {
                    if (component?.Types == null)
                        continue;

                    if (component.Types.Contains("postal_code"))
                        pincode = component.LongName;

                    if (component.Types.Contains("locality"))
                        city = component.LongName;

                    if (component.Types.Contains("administrative_area_level_1"))
                        state = component.LongName;

                    if (component.Types.Contains("country"))
                        country = component.LongName;
                    
                }
            }
           
            return new UserAddressSM
            {
                Address = address ?? "",
                Landmark = "",
                Area = "",
                Pincode = pincode ?? "",
                City = city ?? "",
                State = state ?? "",
                Country = country ?? "",
                Latitude = latitude,
                Longitude = longitude,
                IsDefault = false
            };
        }
        public async Task<List<UserAddressSM>> GetAddressFromSearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<UserAddressSM>();

            var client = _httpClientFactory.CreateClient();
            var apiKey = _apiConfiguration.GoogleCloudLocation.ApiKey;

            var url =
                $"https://maps.googleapis.com/maps/api/geocode/json" +
                $"?address={Uri.EscapeDataString(searchText)}&key={apiKey}";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Google Geocode Error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var googleData =
                JsonSerializer.Deserialize<GoogleGeocodeResponse>(json);

            if (googleData?.Results == null || !googleData.Results.Any())
                return new List<UserAddressSM>();

            var addresses = new List<UserAddressSM>();

            foreach (var result in googleData.Results)
            {
                string? pincode = null;
                string? city = null;
                string? state = null;
                string? country = null;

                if (result.AddressComponents != null)
                {
                    foreach (var component in result.AddressComponents)
                    {
                        if (component.Types == null)
                            continue;

                        if (component.Types.Contains("postal_code"))
                            pincode = component.LongName;

                        if (component.Types.Contains("locality"))
                            city = component.LongName;

                        if (component.Types.Contains("administrative_area_level_1"))
                            state = component.LongName;

                        if (component.Types.Contains("country"))
                            country = component.LongName;
                    }
                }

                addresses.Add(new UserAddressSM
                {
                    Address = result.FormattedAddress ?? "",
                    Landmark = "",
                    Area = "",
                    Pincode = pincode ?? "",
                    City = city ?? "",
                    State = state ?? "",
                    Country = country ?? "",
                    Latitude = result.Geometry?.Location?.Lat ?? 0,
                    Longitude = result.Geometry?.Location?.Lng ?? 0,
                    IsDefault = false
                });
            }

            return addresses;
        }
    }
}