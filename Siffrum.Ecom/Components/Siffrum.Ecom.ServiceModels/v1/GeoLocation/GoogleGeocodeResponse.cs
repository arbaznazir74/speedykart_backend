using System.Text.Json.Serialization;

namespace Siffrum.Ecom.ServiceModels.v1.GeoLocation
{
    public class GoogleGeocodeResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("results")]
        public List<GoogleResult>? Results { get; set; }
    }

    public class GoogleResult
    {
        [JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }

        [JsonPropertyName("address_components")]
        public List<AddressComponent>? AddressComponents { get; set; }
        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }
    }
    public class Geometry
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]   // 🔥 FIXED
        public double Lng { get; set; }
    }
    public class AddressComponent
    {
        [JsonPropertyName("long_name")]
        public string? LongName { get; set; }

        [JsonPropertyName("short_name")]
        public string? ShortName { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }
    }
}