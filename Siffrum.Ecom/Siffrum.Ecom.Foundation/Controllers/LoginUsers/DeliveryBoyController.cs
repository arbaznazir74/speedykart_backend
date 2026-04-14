using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using Siffrum.Ecom.ServiceModels.v1.General;

namespace Siffrum.Ecom.Foundation.Controllers.LoginUsers
{
    [ApiController]
    [Route("api/v1/[controller]")]    
    public class DeliveryBoyController : ApiControllerWithOdataRoot<DeliveryBoySM>
    {
        private readonly DeliveryBoyProcess _deliveryBoyProcess;
        public DeliveryBoyController(DeliveryBoyProcess process)
            : base(process)
        { 
            _deliveryBoyProcess = process;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DeliveryBoySM>>>> GetAsOdata(ODataQueryOptions<DeliveryBoySM> oDataOptions)
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
        public async Task<ActionResult<ApiResponse<List<DeliveryBoySM>>>> GetAllDeliveryBoysByAdmins(int skip, int top)
        {
            #region Check Request           

            #endregion Check Request

            var response = await _deliveryBoyProcess.GetAll(skip, top);
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
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllDeliveryBoysCount()
        {
            #region Check Request           

            #endregion Check Request

            var response = await _deliveryBoyProcess.GetAllDeliveryBoysCount();
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("type")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoySM>>>> GetAllDeliveryBoysByTypeByAdmins(DeliveryBoyPaymentTypeSM type,int skip, int top)
        {
            #region Check Request           

            #endregion Check Request

            var response = await _deliveryBoyProcess.GetAllByType(type,skip, top);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        [HttpGet("type/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllDeliveryBoysByTypeCount(DeliveryBoyPaymentTypeSM type)
        {
            #region Check Request           

            #endregion Check Request

            var response = await _deliveryBoyProcess.GetAllDeliveryBoysByTypeCount(type);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        [HttpGet("search")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> GetAllBySearch(string? searchString)
        {

            var response = await _deliveryBoyProcess.SearchDeliveryBoy(searchString);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("search/type")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> SearchDeliveryBoyByType(string? searchString, DeliveryBoyPaymentTypeSM? type)
        {

            var response = await _deliveryBoyProcess.SearchDeliveryBoyByType(type, searchString);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("register")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RegisterDeliveryBoy([FromBody] ApiRequest<DeliveryBoySM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var adminId = User.GetUserRecordIdFromCurrentUserClaims();
            if (adminId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request
            var addedSM = await _deliveryBoyProcess.RegisterDeliveryBoy(adminId, innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }
        [HttpPost("assign-devicetoken")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AssignDeviceTokenToDeliveryBoy([FromBody] ApiRequest<DeviceTokenSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            #endregion Check Request
            var addedSM = await _deliveryBoyProcess.AssignDeviceTokenToDeliveryBoy(dBoyId, innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        /*[HttpPost("verify-email")]
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

            var addedSM = await _deliveryBoyProcess.VerifyEmailRequest(innerReq);
            if (addedSM != null)
            {
                return ModelConverter.FormNewSuccessResponse(addedSM);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }*/

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

            var response = await _deliveryBoyProcess.SendResetPasswordLink(innerReq);
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

            var response = await _deliveryBoyProcess.UpdatePassword(innerReq);
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
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
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
            var response = await _deliveryBoyProcess.ChangePassword(userId, innerReq);
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
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
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

            var response = await _deliveryBoyProcess.SetPassord(userId, innerReq);
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
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> UpdateDeliveryBoy(long id, [FromBody] ApiRequest<DeliveryBoySM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            
            #endregion Check Request

            var response = await _deliveryBoyProcess.UpdateAsync(id, innerReq, false, false);
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
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> UpdateMineDeliveryBoy([FromBody] ApiRequest<DeliveryBoySM> apiRequest)
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

            var response = await _deliveryBoyProcess.UpdateAsync(userId, innerReq, false, true);
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
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> GetDeliveryBoy()
        {
            #region Check Request
            
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if(userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request
            var response = await _deliveryBoyProcess.GetMineAccountDetails(userId);
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
        public async Task<ActionResult<ApiResponse<DeliveryBoySM>>> GetDeliveryBoyByAdmin(long id)
        {
            #region Check Request           
            
            #endregion Check Request

            var response = await _deliveryBoyProcess.GetByIdAsync(id);
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
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteDeliveryBoy(long id)
        {
            #region Check Request           
            
            #endregion Check Request

            var response = await _deliveryBoyProcess.DeleteAsync(id);
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
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteMineDetails()
        {
            #region Check Request           
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            #endregion Check Request

            var response = await _deliveryBoyProcess.DeleteAsync(userId);
            if (response != null)
            {
                return ModelConverter.FormNewSuccessResponse(response);
            }
            else
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_PassedDataNotSaved, ApiErrorTypeSM.NoRecord_NoLog));
            }
        }

        #region Delivery Boy Pincodes


        [HttpPost("create-pincode")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyPincodesSM>>> CreatePincode([FromBody] ApiRequest<DeliveryBoyPincodesSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            #endregion Check Request

            var response = await _deliveryBoyProcess.AddDeliveryPincode(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
            
        }

        [HttpGet("pincode/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyPincodesSM>>> GetPincodeById(long id)
        {
            var response = await _deliveryBoyProcess.GetDeliveryBoyPincodeByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("pincodes/{deliveryBoyId}")] 
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyPincodesSM>>>> GetAllPincodesByDeliveryBoyId(long deliveryBoyId, int skip, int top)
        {
            var response = await _deliveryBoyProcess.GetAllPincodesByDeliveryBoyIdAsync(deliveryBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("pincodes/count/{deliveryBoyId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllPincodesByDeliveryBoyIdCount(long deliveryBoyId)
        {
            var response = await _deliveryBoyProcess.GetAllPincodesByDeliveryBoyIdAsyncCount(deliveryBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/pincodes")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyPincodesSM>>>> GetAllMinePincodesByDeliveryBoyId(int skip, int top)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryBoyProcess.GetAllPincodesByDeliveryBoyIdAsync(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("mine/pincodes/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllMinePincodesByDeliveryBoyIdCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryBoyProcess.GetAllPincodesByDeliveryBoyIdAsyncCount(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("pincode/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyPincodesSM>>> UpdateDeliveryBoyPincode(long id, [FromBody] ApiRequest<DeliveryBoyPincodesSM> apiRequest)
        {
            #region Check Request

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion

            var response = await _deliveryBoyProcess.UpdateDeliveryBoyPincodeAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("pincode/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteDeliveryBoyPincode(long id)
        {
            var response = await _deliveryBoyProcess.DeleteDeliveryBoyPincodeAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("all/pincode/{pincode}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoySM>>>> GetAllDeliveryBoysByPincode(string pincode, int skip, int top)
        {
            var response = await _deliveryBoyProcess.GetDeliveryBoyByPincodeAsync(pincode,skip,top);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("all/pincode/count/{pincode}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllDeliveryBoysByPincodeCount(string pincode)
        {
            var response = await _deliveryBoyProcess.GetDeliveryBoyByPincodeAsyncCount(pincode);
            return ModelConverter.FormNewSuccessResponse(response);
        }


        #endregion Delivery Boy Pincodes

        #region Admin - Assign to Seller

        [HttpPut("{id}/assign-seller")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AssignToSeller(long id, [FromQuery] long sellerId)
        {
            var response = await _deliveryBoyProcess.AssignToSeller(id, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{id}/unassign-seller")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UnassignFromSeller(long id)
        {
            var response = await _deliveryBoyProcess.UnassignFromSeller(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("{id}/payment-type")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ChangePaymentType(long id, [FromQuery] DeliveryBoyPaymentTypeSM paymentType)
        {
            var response = await _deliveryBoyProcess.ChangePaymentType(id, paymentType);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Toggle Availability (Online/Offline)

        [HttpPut("mine/toggle-availability")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ToggleAvailability()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _deliveryBoyProcess.ToggleAvailability(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine/set-availability")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> SetAvailability([FromQuery] bool isOnline)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _deliveryBoyProcess.SetAvailability(userId, isOnline);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Delivery Boy Stats

        [HttpGet("{id}/stats")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyStatsSM>>> GetDeliveryBoyStats(long id)
        {
            var response = await _deliveryBoyProcess.GetDeliveryBoyStats(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/stats")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyStatsSM>>> GetMyStats()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _deliveryBoyProcess.GetDeliveryBoyStats(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}
