using System.Text.Json.Serialization;

namespace Siffrum.Ecom.ServiceModels.v1.GeoLocation
{
    public class NominatimAddress
    {
        [JsonPropertyName("suburb")]
        public string? Suburb { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("county")]
        public string? County { get; set; }

        [JsonPropertyName("state_district")]
        public string? StateDistrict { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("ISO3166-2-lvl4")]
        public string? ISO3166_2_lvl4 { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }
    }
}
