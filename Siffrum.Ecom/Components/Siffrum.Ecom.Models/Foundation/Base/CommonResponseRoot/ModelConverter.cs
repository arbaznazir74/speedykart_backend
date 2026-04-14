using CoreVisionServiceModels.Foundation.Base.Enums;

namespace CoreVisionServiceModels.Foundation.Base.CommonResponseRoot
{
    public class ModelConverter
    {
        public static ApiRequest<T> FormNewRequestObject<T>(T innerRequest, QueryFilter? queryFilter = null) where T : class
        {
            return new ApiRequest<T>
            {
                ReqData = innerRequest,
                QueryFilter = queryFilter
            };
        }

        public static ApiResponse<T> FormNewSuccessResponse<T>(T innerResponse) where T : class
        {
            return new ApiResponse<T>
            {
                SuccessData = innerResponse,
                IsError = false
            };
        }

        public static ApiResponse<T> FormNewErrorResponse<T>(string displayMessage = "", ApiErrorTypeSM apiErrorType = ApiErrorTypeSM.Fatal_Log, Dictionary<string, object>? additionalProps = null, int respStatusCode = 0) where T : class
        {
            ApiResponse<T> apiResponse = new ApiResponse<T>
            {
                SuccessData = null,
                IsError = true,
                ErrorData = new ErrorData
                {
                    ApiErrorType = apiErrorType,
                    DisplayMessage = displayMessage ?? "Unknown Error Occured in Api. Please retry after sometime or contact service team.",
                    AdditionalProps = additionalProps ?? new Dictionary<string, object>()
                }
            };
            if (respStatusCode != 0)
            {
                apiResponse.ResponseStatusCode = respStatusCode;
            }

            return apiResponse;
        }

        public static ApiResponse<T> FormNewErrorResponse<T>(Exception e, string displayMessage = "", ApiErrorTypeSM apiErrorType = ApiErrorTypeSM.Fatal_Log, Dictionary<string, object>? additionalProps = null, int respStatusCode = 0) where T : class
        {
            additionalProps = additionalProps ?? new Dictionary<string, object>();
            additionalProps.Add("ExpMsg", e.Message);
            displayMessage = displayMessage ?? e.Message;
            return FormNewErrorResponse<T>(displayMessage, apiErrorType, additionalProps, respStatusCode);
        }

        public static ApiResponse<object> FormNewErrorResponse(string displayMessage = "", ApiErrorTypeSM apiErrorType = ApiErrorTypeSM.Fatal_Log, Dictionary<string, object>? additionalProps = null, int respStatusCode = 0)
        {
            return FormNewErrorResponse<object>(displayMessage, apiErrorType, additionalProps, respStatusCode);
        }
    }
}
