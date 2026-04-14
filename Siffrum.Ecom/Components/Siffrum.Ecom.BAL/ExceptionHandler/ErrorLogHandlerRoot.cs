using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DomainModels.Foundation;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Newtonsoft.Json;
using Siffrum.Ecom.BAL.Foundation.Config;
using Siffrum.Ecom.BAL.Foundation;
using Siffrum.Ecom.BAL.Foundation.Web;

namespace Siffrum.Ecom.BAL.ExceptionHandler
{
    public class ErrorLogHandlerRoot
    {
        private readonly APIConfigRoot _apiConfigRoot;

        private readonly ErrorLogProcessRoot _errorLogProcessRoot;

        private readonly ApplicationIdentificationRoot _applicationIdentificationRoot;

        public ErrorLogHandlerRoot(APIConfigRoot apiConfigRoot, ErrorLogProcessRoot errorLogProcessRoot, ApplicationIdentificationRoot applicationIdentificationRoot)
        {
            _apiConfigRoot = apiConfigRoot;
            _errorLogProcessRoot = errorLogProcessRoot;
            _applicationIdentificationRoot = applicationIdentificationRoot;
        }

        public ErrorData HandleException(Exception e, HttpContext currentContext)
        {
            if (e is ApiExceptionRoot)
            {
                return HandleApiException(e as ApiExceptionRoot, currentContext);
            }

            LogExceptionInDb(e, currentContext);
            return new ErrorData
            {
                DisplayMessage = e.Message + (e.InnerException == null ? "" : "In->" + e.InnerException?.Message),
                ApiErrorType = ApiErrorTypeSM.FrameworkException_Log
            };
        }

        protected ErrorData HandleApiException(ApiExceptionRoot apiException, HttpContext currentContext)
        {
            ErrorData errorData = new ErrorData();
            errorData.DisplayMessage = string.IsNullOrWhiteSpace(apiException.DisplayMessage) ? apiException.Message : apiException.DisplayMessage;
            errorData.ApiErrorType = apiException.ExceptionType;
            if (apiException.IsToLogInDb || _apiConfigRoot.EnableLogForNoLog)
            {
                LogExceptionInDb(apiException, currentContext);
            }

            return errorData;
        }

        protected async Task LogExceptionInDb(Exception ex, HttpContext currentContext)
        {
            if (ex.Data.Contains("LogToGhDbCompleted"))
            {
                return;
            }

            object reqobj = await currentContext.Request.ToCustomizedObjectToLog(addBody: true, addCookies: false);
            try
            {
                ErrorLogRoot errorLog = new ErrorLogRoot
                {
                    TracingId = currentContext.Request.GetTracingIdIfPresent().ToString(),
                    InnerException = ex.InnerException == null ? null : ex.InnerException.ToString(),
                    LogMessage = ex.Message.ToString(),
                    LogStackTrace = ex.StackTrace?.ToString(),
                    RequestObject = JsonConvert.SerializeObject(reqobj),
                    ResponseObject = null,
                    //CompanyCode = currentContext?.User?.GetCompanyCodeFromCurrentUserClaims(),
                    CreatedByApp = _applicationIdentificationRoot.ApplicationName,
                    UserRoleType = currentContext?.User?.GetUserRoleTypeFromCurrentUserClaims(),
                    LoginUserId = currentContext?.User?.Identity?.Name,
                    Caller = currentContext?.Request.GetValueFromHeaderOrQueryByKey("CallerName") ?? "Caller Name Not Present",
                    LogExceptionData = JsonConvert.SerializeObject(ex.Data)
                };
                if (await _errorLogProcessRoot.SaveErrorObjectInDb(errorLog))
                {
                    ex.Data.Add("LogToGhDbCompleted", 1);
                }
            }
            catch (Exception Ex)
            {
                File.AppendAllText(_apiConfigRoot.FileExceptionLogPath, $"Exception occured while logging Error IN DB - {Environment.NewLine} Message -  {Ex.Message} {Environment.NewLine} Stacktrace -  {Ex.StackTrace}");
            }
        }
    }
}
