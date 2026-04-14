using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StoreHoursController : ControllerBase
    {
        private readonly StoreHoursProcess _storeHoursProcess;

        public StoreHoursController(StoreHoursProcess storeHoursProcess)
        {
            _storeHoursProcess = storeHoursProcess;
        }

        // Seller: Get own store hours
        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<StoreHoursSM>>>> GetMyStoreHours()
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var result = await _storeHoursProcess.GetStoreHours(sellerId);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        // Seller: Set/update store hours (full week or partial)
        [HttpPut("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<StoreHoursSM>>>> UpsertMyStoreHours(
            [FromBody] ApiRequest<List<StoreHoursSM>> apiRequest)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));

            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));

            var result = await _storeHoursProcess.UpsertStoreHours(sellerId, innerReq);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        // User/App: Check if a store is currently open
        [HttpGet("availability/{sellerId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<StoreAvailabilitySM>>> CheckAvailability(long sellerId)
        {
            var result = await _storeHoursProcess.CheckStoreAvailability(sellerId);
            return ModelConverter.FormNewSuccessResponse(result);
        }

        // Admin: Get any seller's store hours
        [HttpGet("{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<StoreHoursSM>>>> GetSellerStoreHours(long sellerId)
        {
            var result = await _storeHoursProcess.GetStoreHours(sellerId);
            return ModelConverter.FormNewSuccessResponse(result);
        }
    }
}
