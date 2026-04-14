using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ApiControllerWithOdataRoot<OrderSM>
    {
        private readonly OrderProcess _orderProcess;
        private readonly OrderComplaintProcess _complaintProcess;

        public OrderController(OrderProcess process, OrderComplaintProcess complaintProcess)
            : base(process)
        {
            _orderProcess = process;
            _complaintProcess = complaintProcess;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderSM>>>> GetAsOdata(
            ODataQueryOptions<OrderSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        #endregion

        #region USER - CREATE ORDER

        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> Create(
            [FromBody] ApiRequest<CreateOrderRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var orderSM = innerReq.Order;
            orderSM.UserId = userId;

            var response = await _orderProcess.CreateOrderAsync(
                orderSM,
                innerReq.OrderItems);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("is-product-servicable")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> IsProductServicable(long productVariantId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.IsProductOrderPossible(userId, productVariantId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("delivery-charges")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeliveryChargeResponseSM>>> GetDeliveryCharges()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.GetDeliveryCharges(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Id

        [HttpGet("order")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> GetOrderById(long orderId)
        {          

            var response = await _orderProcess.GetOrderByOrderId(orderId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order-item")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<OrderItemSM>>> GetOrdersItemByOrderItemId(long orderItemId)
        {

            var response = await _orderProcess.GetOrdersItemByOrderItemId(orderItemId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("order-items")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderItemSM>>>> GetOrderItemsByOrderId(long orderId)
        {

            var response = await _orderProcess.GetOrdersItemByOrderId(orderId);
            return ModelConverter.FormNewSuccessResponse(response);
        }


        #endregion Get

        #region USER - GET MY ORDERS

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetMyOrders(
            int skip,
            int top)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.GetMyOrdersAsync(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMineOrdersCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.GetMyOrdersCountAsync(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("mine/is-first-order-applicable")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> IsMyFirstOrderApplicable()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.IsMyFirstOrderApplicable(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("user-orders")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetUserOrdersByOrderTye(
            PaymentStatusSM paymentStatus,
            long userId,
            int skip,
            int top)
        {           

            var response = await _orderProcess.GetUserOrdersByOrderTye(paymentStatus, userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("user-orders/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetUserOrdersByOrderTyeCount(PaymentStatusSM paymentStatus, long userId)
        {       
            var response = await _orderProcess.GetUserOrdersByOrderTyeCount(paymentStatus,userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("seller-orders")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderItemSM>>>> GetSellerOrdersItemsAsync(
            long sellerId,
            int skip,
            int top)
        {           

            var response = await _orderProcess.GetSellerOrdersItemsAsync(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller-orders/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSellerOrdersItemsCountAsync( long sellerId)
        {       
            var response = await _orderProcess.GetSellerOrdersItemsCountAsync(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller-earnings")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SellerEarningsSM>>> GetSellerEarnings(long sellerId)
        {
            var response = await _orderProcess.GetSellerEarningsAsync(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order-items/{orderId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<List<OrderItemDetailSM>>>> GetOrderItems(
            int orderId,
            int skip,
            int top)
        {
            long userId = 0;
            var isSuperAdmin = false;
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if(role == RoleTypeSM.SuperAdmin.ToString() || role == RoleTypeSM.SystemAdmin.ToString()
                || role == RoleTypeSM.Seller.ToString())
            {
                isSuperAdmin = true;
            }
            else
            {
                userId = User.GetUserRecordIdFromCurrentUserClaims();
                if (userId <= 0)
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(
                        DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
                }
            }
            

            var response = await _orderProcess.GetOrderItemsDetailAsync(userId, orderId, isSuperAdmin, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order-items/count/{orderId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin,User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetOrderItemsCount(
            int orderId)
        {
            long userId = 0;
            var isSuperAdmin = false;
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (role == RoleTypeSM.SuperAdmin.ToString() || role == RoleTypeSM.SystemAdmin.ToString())
            {
                isSuperAdmin = true;
            }
            else
            {
                userId = User.GetUserRecordIdFromCurrentUserClaims();
                if (userId <= 0)
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(
                        DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
                }
            }


            var response = await _orderProcess.GetOrdersItemsCountAsync(userId, orderId, isSuperAdmin);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> GetMyOrderById(long id)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.GetMyOrderByIdAsync(id, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region ADMIN - GET ALL

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetAll(
            int skip,
            int top)
        {
            var response = await _orderProcess.GetAllAsync(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCount()
        {
            var response = await _orderProcess.GetCountAsync();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("search")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> Search(long? id, PaymentStatusSM? paymentStatus, OrderStatusSM? orderStatus,
            int skip,
            int top)
        {
            var response = await _orderProcess.SearchOrder(id, paymentStatus, orderStatus,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("search/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> SearchCount(long? id, PaymentStatusSM? paymentStatus, OrderStatusSM? orderStatus)
        {
            var response = await _orderProcess.SearchOrderCount(id, paymentStatus, orderStatus);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("payment-status")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetAllByPaymentStatus(
            PaymentStatusSM status,
            int skip,
            int top)
        {
            var response = await _orderProcess.GetAllByPaymentStatusAsync(status, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("payment-status/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetByPaymentStatusCount(PaymentStatusSM status)
        {
            var response = await _orderProcess.GetByPaymentStatusCountAsync(status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order-status")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetAllByOrderStatus(
           OrderStatusSM status,
           int skip,
           int top)
        {
            var response = await _orderProcess.GetAllByOrderStatusAsync(status, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order-status/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetByOrderStatusCount(OrderStatusSM status)
        {
            var response = await _orderProcess.GetByOrderStatusCountAsync(status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> GetById(long id)
        {
            var response = await _orderProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region ADMIN - UPDATE STATUS

        [HttpPut("admin/status/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateOrderStatus(
            long id,
            OrderStatusSM status)
        {
            var response = await _orderProcess.UpdateOrderStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("admin/payment-status/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdatePaymentStatus(
            long id,
            PaymentStatusSM status)
        {
            var response = await _orderProcess.UpdatePaymentStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region USER - CANCEL

        [HttpPut("mine/cancel/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Cancel(long id)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _orderProcess.CancelOrderAsync(id, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region ADMIN - DELETE

        [HttpDelete("admin/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var response = await _orderProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Invoice

        [HttpGet("invoice/{orderId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<OrderInvoiceSM>>> GetInvoice(long orderId)
        {
            long userId = 0;
            var isSuperAdmin = false;
            var role = User.GetUserRoleTypeFromCurrentUserClaims();
            if (role == RoleTypeSM.SuperAdmin.ToString() || role == RoleTypeSM.SystemAdmin.ToString()
                || role == RoleTypeSM.Seller.ToString())
            {
                isSuperAdmin = true;
            }
            else
            {
                userId = User.GetUserRecordIdFromCurrentUserClaims();
                if (userId <= 0)
                {
                    return NotFound(ModelConverter.FormNewErrorResponse(
                        DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
                }
            }

            var response = await _orderProcess.GenerateInvoice(orderId, userId, isSuperAdmin);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Invoice

        #region Order Address

        [HttpGet("address/{orderId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<UserAddressSM>>> GetOrderAddressById(long orderId)
        {
            var response = await _orderProcess.GetOrderAddress(orderId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion Order Address

        #region Complaints

        // ── User: Submit complaint ──
        [HttpPost("complaint")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<OrderComplaintSM>>> SubmitComplaint(
            [FromBody] ApiRequest<OrderComplaintSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.SubmitComplaint(userId, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // ── User: My complaints ──
        [HttpGet("complaints/mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<OrderComplaintSM>>>> GetMyComplaints(
            int skip = 0, int top = 10)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.GetMyComplaints(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("complaints/mine/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMyComplaintsCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.GetMyComplaintsCount(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // ── Seller: Get complaints ──
        [HttpGet("complaints/seller")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<OrderComplaintDetailSM>>>> GetSellerComplaints(
            int skip = 0, int top = 10)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.GetSellerComplaints(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("complaints/seller/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSellerComplaintsCount()
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.GetSellerComplaintsCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("complaints/seller/{complaintId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<OrderComplaintDetailSM>>> GetSellerComplaintById(long complaintId)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.GetSellerComplaintById(sellerId, complaintId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // ── Seller: Reply ──
        [HttpPut("complaints/seller/{complaintId}/reply")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<OrderComplaintSM>>> ReplyToComplaint(
            long complaintId,
            [FromBody] ApiRequest<ComplaintReplySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _complaintProcess.ReplyToComplaint(sellerId, complaintId, innerReq.Reply, innerReq.Status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // ── Admin: All complaints ──
        [HttpGet("complaints")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<OrderComplaintDetailSM>>>> GetAllComplaints(
            int skip = 0, int top = 10)
        {
            var response = await _complaintProcess.GetAllComplaints(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("complaints/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllComplaintsCount()
        {
            var response = await _complaintProcess.GetAllComplaintsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Complaints

    }
}
