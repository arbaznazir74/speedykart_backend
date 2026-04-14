using Newtonsoft.Json;

namespace Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot
{
    public class QueryFilter
    {
        [JsonProperty("$skip")]
        public int? Skip { get; set; }

        [JsonProperty("$top")]
        public int? Top { get; set; }

        public QueryFilter()
        {
            Skip = -1;
            Top = -1;
        }
    }
}
