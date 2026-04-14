using Newtonsoft.Json;

namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class ApiResponse<T> where T : class
    {
        [JsonProperty("responseStatusCode")]
        [JsonIgnore]
        public int ResponseStatusCode { get; set; }

        [JsonProperty("successData")]
        public T? SuccessData { get; set; }

        [JsonProperty("isError")]
        public bool IsError { get; set; }

        [JsonProperty("errorData")]
        public ErrorData ErrorData { get; set; }
    }
}
