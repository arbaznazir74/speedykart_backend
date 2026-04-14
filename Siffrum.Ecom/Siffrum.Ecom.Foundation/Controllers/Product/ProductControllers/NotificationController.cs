using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Base.OneSignal;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class NotificationController : ApiControllerRoot
    {
        private readonly NotificationProcess _notificationProcess;

        public NotificationController(NotificationProcess process)
        {
            _notificationProcess = process;
        }

        #region Add

        [HttpPost("bulk")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> SendBulkNotification(
            [FromBody] ApiRequest<SendNotificationMessageSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _notificationProcess.SendBulkPushNotification(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("single")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> SendSingleNotification(
            [FromBody] ApiRequest<SendNotificationMessageSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _notificationProcess.SendPushNotification(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("single/{playerId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> SendSingleNotificationByPlayerId(
            string playerId,
            [FromBody] ApiRequest<SendNotificationMessageSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _notificationProcess.SendPushNotificationByPlayerId(playerId,innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("broadcast")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Broadcast(            
            [FromBody] ApiRequest<SendNotificationMessageSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _notificationProcess.BroadcastPushNotification(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        /*[HttpPost("sms")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> SMS(            
            [FromBody] ApiRequest<SendNotificationMessageSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _notificationProcess.SendOtpSms("+917006636038", 123456);
            return ModelConverter.FormNewSuccessResponse(response);
        }*/

        #endregion
    }
}