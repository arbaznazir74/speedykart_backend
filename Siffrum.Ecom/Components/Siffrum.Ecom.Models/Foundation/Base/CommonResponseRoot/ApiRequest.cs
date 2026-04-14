using Newtonsoft.Json;

namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class ApiRequest<T> where T : class
    {
        [JsonProperty("reqData")]
        public T ReqData { get; set; }

        [JsonProperty("queryFilter")]
        public QueryFilter? QueryFilter { get; set; }

        public ApiRequest()
        {
            QueryFilter = null;
        }
    }
}
