using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SellerSettingsController : ControllerBase
    {
        private readonly SellerSettingsProcess _sellerSettingsProcess;

        public SellerSettingsController(SellerSettingsProcess sellerSettingsProcess)
        {
            _sellerSettingsProcess = sellerSettingsProcess;
        }

        #region CREATE

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = " SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> CreateSellerSettings(
            [FromBody] ApiRequest<SellerSettingsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _sellerSettingsProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = " Seller")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> CreateMineSellerSettings(
            [FromBody] ApiRequest<SellerSettingsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var sId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            innerReq.SellerId = sId;
            var response = await _sellerSettingsProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ

        [HttpGet("{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> GetSellerSettings(long sellerId)
        {
            var response = await _sellerSettingsProcess.GetBySellerIdAsync(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> GetMineSellerSettings()
        {
            var sId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _sellerSettingsProcess.GetBySellerIdAsync(sId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("pincode/{pincode}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> SellerSettingsByPincode(string pincode)
        {
            var response = await _sellerSettingsProcess.SellerSettingsByPincode(pincode);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("sellerId")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> UpdateSellerSettings(long sellerId,
            [FromBody] ApiRequest<SellerSettingsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            innerReq.SellerId = sellerId;
            var response = await _sellerSettingsProcess.UpdateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<SellerSettingsSM>>> UpdateMineSellerSettings(
            [FromBody] ApiRequest<SellerSettingsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var sId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            innerReq.SellerId = sId;
            var response = await _sellerSettingsProcess.UpdateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteSellerSettings(long sellerId)
        {
            var response = await _sellerSettingsProcess.DeleteAsync(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpDelete("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteMineSellerSettings()
        {
            var sId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _sellerSettingsProcess.DeleteAsync(sId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}