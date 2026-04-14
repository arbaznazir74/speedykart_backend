using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.BAL.Foundation.Web;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderDeliveryController : ApiControllerRoot
    {
        private readonly OrderDeliveryProcess _deliveryProcess;

        public OrderDeliveryController(OrderDeliveryProcess deliveryProcess)
        {
            _deliveryProcess = deliveryProcess;
        }

        #region Assign Delivery

        [HttpPost("assign")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliverySM>>> AssignDelivery(
            [FromBody] ApiRequest<DeliverySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Request data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _deliveryProcess.AssignDelivery(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("accept-order")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliverySM>>> AcceptOrder(
            [FromBody] ApiRequest<DeliverySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Request data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            innerReq.DeliveryBoyId = deliveryBoyId;
            var response = await _deliveryProcess.AcceptOrder(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        } 
        
        [HttpPost("deliver-order")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> DeliverOrder(
            long orderId = 0,
            [FromBody] ApiRequest<DeliverOrderRequestSM> apiRequest = null)
        {
            // Support orderId from query param (?orderId=X) or request body (reqData.orderId)
            var effectiveOrderId = orderId > 0 ? orderId : (apiRequest?.ReqData?.OrderId ?? 0);

            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            if (effectiveOrderId <= 0)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse("Order ID is required"));
            }

            var response = await _deliveryProcess.DeliverOrder(effectiveOrderId, deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #region Delivery Status Update

        [HttpPut("status")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliveryStatusHistorySM>>>> UpdateStatus(
            long deliveryId,
            DeliveryStatusSM status)
        {
            var response = await _deliveryProcess.UpdateDeliveryStatus(
                deliveryId,
                (DeliveryStatusDM)status);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("status-by-order")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliveryStatusHistorySM>>>> UpdateStatusByOrder(
            long orderId,
            DeliveryStatusSM status)
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _deliveryProcess.UpdateDeliveryStatusByOrder(
                orderId, deliveryBoyId, (DeliveryStatusDM)status);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #region Delivery Tracking

        [HttpPost("tracking")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryTrackingSM>>> UpdateLocation(
            [FromBody] ApiRequest<DeliveryTrackingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Tracking data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _deliveryProcess.UpdateLocation(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpGet("tracking/latest")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User, DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryTrackingSM>>> GetLatestLocation(long deliveryId)
        {
            var response = await _deliveryProcess.GetLatestLocation(deliveryId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #region User Tracking

        [HttpGet("tracking/order")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<DeliveryTrackingSM>>>> GetUsersOrderTracking(long orderId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if(userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryProcess.GetUsersOrderTracking(userId, orderId);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("user/order/status")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<DeliveryStatusHistorySM>>>> GetDeliveryStatuses(long orderId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryProcess.GetDeliveryStatuses(orderId, userId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("user/order/deliveryboy")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> GetDeliveryBoyDetails(long orderId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryProcess.GetDeliveryBoyDetails(orderId, userId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order/status")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliveryStatusHistorySM>>>> GetDeliveryStatusesOfOrderId(long orderId)
        {
            
            var response = await _deliveryProcess.GetDeliveryStatusesOfOrderId(orderId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #region Delivery Boy Orders

        [HttpPost("deliveryboy/orders")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliverySM>>>> GetDeliveryBoyOrders(
            long deliveryBoyId,int skip, int top,
            [FromBody] ApiRequest<DeliveryBoyOrderRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Request data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _deliveryProcess.GetDeliveryBoyOrders(deliveryBoyId, innerReq, skip, top);

            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpPost("deliveryboy/orders/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetDeliveryBoyOrdersCount(
            long deliveryBoyId,
            [FromBody] ApiRequest<DeliveryBoyOrderRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Request data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _deliveryProcess.GetDeliveryBoyOrdersCount(deliveryBoyId, innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpPost("mine/orders")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliverySM>>>> GetMineDeliveryBoyOrders(int skip, int top,
            [FromBody] ApiRequest<DeliveryBoyOrderRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Request data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _deliveryProcess.GetDeliveryBoyOrders(deliveryBoyId, innerReq, skip, top);

            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpPost("mine/orders/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMineDeliveryBoyOrdersCount(
            [FromBody] ApiRequest<DeliveryBoyOrderRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Request data not formed",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _deliveryProcess.GetDeliveryBoyOrdersCount(deliveryBoyId, innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("mine/all-orders")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliverySM>>>> GetMineAllDeliveryBoyOrders(int skip, int top)
        {
            
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _deliveryProcess.GetAllDeliveryBoyOrders(deliveryBoyId, skip, top);

            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpPost("mine/all-orders/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMineAllDeliveryBoyOrdersCount()
        {
            
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _deliveryProcess.GetAllDeliveryBoyOrdersCount(deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/available-orders")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetAvailableOrders(int skip = 0, int top = 20)
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _deliveryProcess.GetAvailableOrdersForDeliveryBoy(deliveryBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/available-orders/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAvailableOrdersCount()
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _deliveryProcess.GetAvailableOrdersCountForDeliveryBoy(deliveryBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #region Delivery Boy Order Details

        [HttpGet("deliveryboy/order-details")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyOrderDetailsSM>>> GetOrderDetails(
            long deliveryBoyId,long orderId)
        {
            
            var response = await _deliveryProcess.OrderDetailsForDeliveryBoy(deliveryBoyId, orderId);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("deliveryboy/mine/order-details")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyOrderDetailsSM>>> GetOrderDetails(
            long orderId)
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryProcess.OrderDetailsForDeliveryBoy(deliveryBoyId, orderId);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        /*[HttpGet("deliveryboy/mine/earnings")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyEarningSM>>> GetDeliveryBoyEarnings()
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryProcess.GetDeliveryBoyEarnings(deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }*/

        [HttpGet("admin/order-lifecycle/{orderId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<AdminOrderLifecycleSM>>> GetOrderLifecycle(long orderId)
        {
            var response = await _deliveryProcess.GetOrderLifecycle(orderId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("deliveryboy/earnings/tip/{deliveryBoyId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin,SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyEarningSM>>> GetDeliveryBoyEarningsById(long deliveryBoyId)
        {
            
            var response = await _deliveryProcess.GetDeliveryBoyEarningsByTip(deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("deliveryboy/earnings/delivery-charge/{deliveryBoyId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin,SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyEarningSM>>> GetDeliveryBoyEarningsByDeliveryCharges(long deliveryBoyId)
        {
            
            var response = await _deliveryProcess.GetDeliveryBoyEarningsByDeliveryCharges(deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/earnings/tip")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyEarningSM>>> GetMineEarningsById()
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _deliveryProcess.GetDeliveryBoyEarningsByTip(deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/earnings/delivery-charge")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyEarningSM>>> GetMineEarningsByDeliveryCharges()
        {
            var deliveryBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (deliveryBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _deliveryProcess.GetDeliveryBoyEarningsByDeliveryCharges(deliveryBoyId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}