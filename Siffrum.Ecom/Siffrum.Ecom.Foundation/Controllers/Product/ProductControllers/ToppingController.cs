using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ToppingController : ApiControllerRoot
    {
        private readonly ToppingProcess _toppingProcess;

        public ToppingController(ToppingProcess toppingProcess)
        {
            _toppingProcess = toppingProcess;
        }

        #region CREATE
        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateTopping(
            [FromBody] ApiRequest<ToppingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _toppingProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region READ
        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<List<ToppingSM>>>> GetAllToppings(int skip, int top)
        {
            var response = await _toppingProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetToppingsCount()
        {
            var response = await _toppingProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ToppingSM>>> GetToppingById(long id)
        {
            var response = await _toppingProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("active")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<List<ToppingSM>>>> GetAllActiveToppings()
        {
            var response = await _toppingProcess.GetAllActive();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("search")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<List<SearchResponseSM>>>> SearchToppings(
            string searchString, int skip = 0, int top = 50)
        {
            var response = await _toppingProcess.SearchToppings(searchString, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region SELLER
        [HttpPost("suggest")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ToppingSM>>> SellerSuggestTopping(
            [FromBody] ApiRequest<ToppingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null || string.IsNullOrWhiteSpace(innerReq.Name))
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Topping name is required",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var sellerId = innerReq.SuggestedBySellerId ?? 0;
            if (sellerId <= 0)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    "Seller ID is required",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _toppingProcess.SellerSuggestAsync(innerReq.Name, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("for-seller/{sellerId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<ToppingSM>>>> GetToppingsForSeller(long sellerId)
        {
            var response = await _toppingProcess.GetForSeller(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<ToppingSM>>> CreateForSeller(
            [FromBody] ApiRequest<ToppingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _toppingProcess.CreateForSellerAsync(innerReq, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<List<ToppingSM>>>> GetMineForSeller()
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _toppingProcess.GetForSeller(sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteForSeller(long id)
        {
            var sellerId = User.GetUserRecordIdFromCurrentUserClaims();
            if (sellerId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _toppingProcess.DeleteForSellerAsync(id, sellerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region ADMIN APPROVAL
        [HttpGet("pending")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ToppingSM>>>> GetPendingToppings()
        {
            var response = await _toppingProcess.GetPendingToppings();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("approve/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> ApproveTopping(long id)
        {
            var response = await _toppingProcess.ApproveToppingAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("reject/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> RejectTopping(long id)
        {
            var response = await _toppingProcess.RejectToppingAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region UPDATE
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ToppingSM>>> UpdateTopping(
            long id, [FromBody] ApiRequest<ToppingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _toppingProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("status/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateToppingStatus(
            long id, StatusDM status)
        {
            var response = await _toppingProcess.UpdateStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteTopping(long id)
        {
            var response = await _toppingProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion
    }
}
