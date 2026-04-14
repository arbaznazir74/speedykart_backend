using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Marketing;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.Category
{
    [ApiController]
    [Route("api/v1/[controller]")]
   

    public class DeliveryPlacesController : ApiControllerWithOdataRoot<DeliveryPlacesSM>
    {
        private readonly DeliveryPlacesProcess _deliveryPlacesProcess;

        public DeliveryPlacesController(DeliveryPlacesProcess process)
            : base(process)
        {
            _deliveryPlacesProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DeliveryPlacesSM>>>> GetAsOdata(
            ODataQueryOptions<DeliveryPlacesSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateDeliveryPlace(long sellerId,
            [FromBody] ApiRequest<DeliveryPlacesSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            innerReq.SellerId = sellerId;
            var response = await _deliveryPlacesProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("seller")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateMineDeliveryPlace(
            [FromBody] ApiRequest<DeliveryPlacesSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            innerReq.SellerId = userId;
            var response = await _deliveryPlacesProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get All and Count

        #region GET ALL (ADMIN)

        [HttpGet()]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryPlacesSM>>>> GetAll(
            int skip, int top)
        {
            var response = await _deliveryPlacesProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _deliveryPlacesProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<List<DeliveryPlacesSM>>>> GetAllForUser(
            int skip, int top)
        {
            var response = await _deliveryPlacesProcess.GetAllForUser(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("user/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCountForUser()
        {
            var response = await _deliveryPlacesProcess.GetCountForUser();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<DeliveryPlacesSM>>>> GetAllMine(
            int skip, int top)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryPlacesProcess.GetAllSellerPlaces(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllMineCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryPlacesProcess.GetSellerPlacesCount(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller/{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryPlacesSM>>>> GetAllSellerPlaces(
            long sellerId,int skip, int top)
        {
            
            var response = await _deliveryPlacesProcess.GetAllSellerPlaces(sellerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("seller/count/{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSellerPlacesCount(long sellerId)
        {
           
            var response = await _deliveryPlacesProcess.GetSellerPlacesCount(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #endregion Get All and Count

        #region READ BY ID

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryPlacesSM>>> GetById(long id)
        {
            var response = await _deliveryPlacesProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryPlacesSM>>> UpdateAsync(
            long id,
            [FromBody] ApiRequest<DeliveryPlacesSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _deliveryPlacesProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("seller/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeliveryPlacesSM>>> UpdateMineAsync(
            long id,
            [FromBody] ApiRequest<DeliveryPlacesSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryPlacesProcess.UpdateMineAsync(userId, id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("status/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateStatus(
            long id,StatusSM status)
        {            
            var response = await _deliveryPlacesProcess.UpdateStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("seller/status/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateMineStatus(
            long id, StatusSM status)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryPlacesProcess.UpdateMineStatusAsync(userId, id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion        

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]

        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteAsync(long id)
        {            
            var response = await _deliveryPlacesProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpDelete("seller/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]

        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteMineAsync(long id)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _deliveryPlacesProcess.DeleteMineAsync(userId, id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

    }
}
