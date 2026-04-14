using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard;
using Siffrum.Ecom.ServiceModels.v1.General;

namespace Siffrum.Ecom.Foundation.Controllers.LoginUsers
{
    [ApiController]
    [Route("api/v1/[controller]")]    
    public class SellerController : ApiControllerWithOdataRoot<SellerSM>
    {
        private readonly SellerProcess _sellerProcess;
        private readonly SellerDashboardProcess _sellerDashboardProcess;
        private readonly OrderProcess _orderProcess;
        private readonly SellerDeliveryBoyProcess _sellerDeliveryBoyProcess;
        private readonly OrderDeliveryProcess _orderDeliveryProcess;
        public SellerController(SellerProcess process, SellerDashboardProcess sellerDashboardProcess,
            OrderProcess orderProcess, SellerDeliveryBoyProcess sellerDeliveryBoyProcess,
            OrderDeliveryProcess orderDeliveryProcess)
            : base(process)
        { 
            _sellerProcess = process;
            _sellerDashboardProcess = sellerDashboardProcess;
            _orderProcess = orderProcess;
            _sellerDeliveryBoyProcess = sellerDeliveryBoyProcess;
            _orderDeliveryProcess = orderDeliveryProcess;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SellerSM>>>> GetAsOdata(ODataQueryOptions<SellerSM> oDataOptions)
        {
            //oDataOptions.Filter = new FilterQueryOption();
            //TODO: validate inputs here probably 
            //if (oDataOptions.Filter == null)
            //    oDataOptions.Filter. = "$filter=organisationUnitId%20eq%20" + 10 + ",";
            var retList = await GetAsEntitiesOdata(oDataOptions);

            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        [HttpGet("")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SellerSM>>>> GetAllSellersByAdmins(int skip, int top)
        {
            #region Check Request           

            #endregion Check Request

            var response = await _sellerProcess.GetAll(skip, top);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSellersCount()
        {
            #region Check Request           

            #endregion Check Request

            var response = await _sellerProcess.GetAllSellersCount();
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("stores")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<List<StoreSM>>>> GetStores()
        {
            var response = await _sellerProcess.GetStoresForUser();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("search")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> GetAllBySearch(string? searchString)
        {           

            var response = await _sellerProcess.SearchSellers(searchString);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RegisterSeller([FromBody] ApiRequest<SellerSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request
            var addedSM = await _sellerProcess.RegisterSeller(innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPost("admin-create")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AdminCreateSeller([FromBody] ApiRequest<SellerSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var result = await _sellerProcess.RegisterSeller(innerReq, isAdminCreated: true);
            if (result != null)
                return ModelConverter.FormNewSuccessResponse(result);
            else
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
        }

        [HttpPost("assign-devicetoken")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AssignDeviceTokenToDeliveryBoy([FromBody] ApiRequest<DeviceTokenSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request
            var addedSM = await _sellerProcess.AssignDeviceTokenToDeliveryBoy(sellerId, innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> VerifyEmail([FromBody] ApiRequest<VerifyEmailRequestSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request
            var addedSM = await _sellerProcess.VerifyEmailRequest(innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ForgotPassword([FromBody] ApiRequest<ForgotPasswordSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request
            var response = await _sellerProcess.SendResetPasswordLink(innerReq);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ResetPassword([FromBody] ApiRequest<ResetPasswordRequestSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Check Request
            var response = await _sellerProcess.UpdatePassword(innerReq);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPost("update-password")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdatePassword([FromBody] ApiRequest<UpdatePasswordRequestSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if(userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _sellerProcess.ChangePassword(userId, innerReq);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPost("set-password")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> SetPassword([FromBody] ApiRequest<SetPasswordRequestSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request

            var response = await _sellerProcess.SetPassord(userId, innerReq);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SellerSM>>> UpdateSeller(long id, [FromBody] ApiRequest<SellerSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            
            #endregion Check Request
            var response = await _sellerProcess.UpdateAsync(id, innerReq, false, false);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        
        [HttpPut("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<SellerSM>>> UpdateMineSeller([FromBody] ApiRequest<SellerSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if(userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _sellerProcess.UpdateAsync(userId, innerReq, false, true);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("mine/orders")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<OrderItemSM>>>> GetMineOrders(int skip, int top)
        {
            #region Check Request

            
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _orderProcess.GetSellerOrdersItemsAsync(userId, skip, top);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        
        [HttpGet("mine/order/{orderItemId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<OrderItemExtendedDetailsSM>>> GetMineOrders(long orderItemId)
        {
            #region Check Request

            
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _orderProcess.GetOrdersItemByOrderItemIdWithStatusDetails(orderItemId);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("mine/orders/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMineOrdersCount()
        {
            #region Check Request
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _orderProcess.GetSellerOrdersItemsCountAsync(userId);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("mine/seller-orders")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<OrderSM>>>> GetMySellerOrders(int skip, int top)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _orderProcess.GetSellerOrders(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/seller-orders/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMySellerOrdersCount()
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _orderProcess.GetSellerOrdersCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine/order/{orderId}/accept")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> AcceptOrder(long orderId, int preparationTimeInMinutes = 0)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _orderProcess.SellerAcceptOrder(orderId, sellerId, preparationTimeInMinutes);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine/order/{orderId}/cancel")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> CancelOrder(long orderId)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _orderProcess.SellerCancelOrder(orderId, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("dashboard")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<SellerDashboardSM>>> SellerDashboard()
        {
            #region Check Request

           
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _sellerDashboardProcess.GetSellerDashboard(userId);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<SellerSM>>> GetSeller()
        {
            #region Check Request
            
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if(userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _sellerProcess.GetMineAccountDetails(userId);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SellerSM>>> GetSellerByAdmin(long id)
        {
            #region Check Request           
            
            #endregion Check Request

            var response = await _sellerProcess.GetByIdAsync(id);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteSeller(long id)
        {
            #region Check Request           
            
            #endregion Check Request

            var response = await _sellerProcess.DeleteAsync(id);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        [HttpDelete("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteMineSeller()
        {
            #region Check Request           
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request

            var response = await _sellerProcess.DeleteAsync(userId);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        #region Seller Delivery Boys

        [HttpPost("mine/delivery-boy/register")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RegisterMyDeliveryBoy(
            [FromBody] ApiRequest<DeliveryBoySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.RegisterDeliveryBoy(sellerId, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/delivery-boys")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoySM>>>> GetMyDeliveryBoys(int skip = 0, int top = 50)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetMyDeliveryBoys(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/delivery-boys/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMyDeliveryBoysCount()
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetMyDeliveryBoysCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/delivery-boy/{dBoyId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> GetMyDeliveryBoyById(long dBoyId)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetMyDeliveryBoyById(sellerId, dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine/delivery-boy/{dBoyId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> UpdateMyDeliveryBoy(long dBoyId,
            [FromBody] ApiRequest<DeliveryBoySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.UpdateDeliveryBoy(sellerId, dBoyId, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine/delivery-boy/{dBoyId}/toggle")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ToggleDeliveryBoy(long dBoyId, bool activate)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.ToggleDeliveryBoyStatus(sellerId, dBoyId, activate);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/delivery-boy/{dBoyId}/stats")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyStatsSM>>> GetMyDeliveryBoyStats(long dBoyId)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetMyDeliveryBoyStats(sellerId, dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine/order/{orderId}/assign-delivery-boy/{dBoyId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeliverySM>>> ManualAssignDeliveryBoy(long orderId, long dBoyId)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            // Verify the order belongs to this seller
            var order = await _orderProcess.GetOrderByOrderId(orderId);
            if (order == null || order.SellerId != sellerId)
                return BadRequest(ModelConverter.FormNewErrorResponse("Order not found or does not belong to you", ApiErrorTypeSM.InvalidInputData_NoLog));

            // Verify the delivery boy belongs to this seller
            var dBoy = await _sellerDeliveryBoyProcess.GetMyDeliveryBoyById(sellerId, dBoyId);

            var deliverySM = new DeliverySM { OrderId = orderId, DeliveryBoyId = dBoyId };
            var response = await _orderDeliveryProcess.AssignDelivery(deliverySM);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Seller COD / Cash Collection

        [HttpGet("mine/cod-summary")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyCodSummarySM>>>> GetCodSummary(DateTime? date = null)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetCodSummary(sellerId, date);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/cod-orders/{dBoyId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<CodOrderDetailSM>>>> GetCodOrders(long dBoyId, int skip = 0, int top = 50)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetCodOrdersForDeliveryBoy(sellerId, dBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("mine/cash-collect")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<CashCollectionSM>>> MarkCashCollected(
            [FromBody] ApiRequest<MarkCashCollectedSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.MarkCashCollected(sellerId, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/cash-collections/{dBoyId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<CashCollectionSM>>>> GetCashCollections(long dBoyId, int skip = 0, int top = 50)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetCashCollections(sellerId, dBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Seller - Order Deliveries

        [HttpGet("mine/order-deliveries")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<SellerOrderDeliverySM>>>> GetMyOrderDeliveries(int skip = 0, int top = 50)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _orderDeliveryProcess.GetSellerOrderDeliveries(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/order-deliveries/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMyOrderDeliveriesCount()
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _orderDeliveryProcess.GetSellerOrderDeliveriesCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Delivery Boy - Cash Ledger

        [HttpGet("delivery-boy/cash-ledger")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyCashLedgerSM>>> GetMyCashLedger()
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (dBoyId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerDeliveryBoyProcess.GetMyCashLedger(dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Admin - Seller Approval

        [HttpGet("pending")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SellerSM>>>> GetPendingSellers(int skip = 0, int top = 20)
        {
            var response = await _sellerProcess.GetPendingSellers(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("pending/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetPendingSellersCount()
        {
            var response = await _sellerProcess.GetPendingSellersCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{sellerId}/approve")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ApproveSeller(long sellerId)
        {
            var response = await _sellerProcess.ApproveSeller(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{sellerId}/reject")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RejectSeller(long sellerId, [FromQuery] string? reason)
        {
            var response = await _sellerProcess.RejectSeller(sellerId, reason);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Store Location

        [HttpPut("mine/location")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RequestLocationChange([FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _sellerProcess.RequestLocationChange(sellerId, latitude, longitude);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{sellerId}/location/approve")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ApproveLocationChange(long sellerId)
        {
            var response = await _sellerProcess.ApproveLocationChange(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{sellerId}/location/reject")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RejectLocationChange(long sellerId)
        {
            var response = await _sellerProcess.RejectLocationChange(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("location/pending")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SellerSM>>>> GetSellersWithPendingLocation(int skip = 0, int top = 20)
        {
            var response = await _sellerProcess.GetSellersWithPendingLocation(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("location/pending/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetSellersWithPendingLocationCount()
        {
            var response = await _sellerProcess.GetSellersWithPendingLocationCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{sellerId}/location")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AdminSetLocation(long sellerId, [FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            var response = await _sellerProcess.AdminSetLocation(sellerId, latitude, longitude);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}
