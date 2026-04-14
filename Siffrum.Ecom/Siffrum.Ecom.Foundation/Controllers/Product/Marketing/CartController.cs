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

namespace Siffrum.Ecom.Foundation.Controllers.Product
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CartController : ApiControllerWithOdataRoot<CartSM>
    {
        private readonly CartProcess _cartProcess;

        public CartController(CartProcess process)
            : base(process)
        {
            _cartProcess = process;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CartSM>>>> GetAsOdata(
            ODataQueryOptions<CartSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        #endregion

        #region ADD TO CART

        [HttpPost("add")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<CombineCartSM>>> AddToCart(
            [FromBody] ApiRequest<CartRequestSM> apiRequest)
        {
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

            var response = await _cartProcess.AddToCart(userId, innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE QUANTITY

        [HttpPut("update")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<CombineCartSM>>> UpdateQuantity(
            [FromBody] ApiRequest<CartRequestSM> apiRequest)
        {
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

            var response = await _cartProcess.UpdateCartItemQuantity(
                userId,
                innerReq.ProductVariantId,
                innerReq.Quantity,
                innerReq.PlatformType);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region REMOVE ITEM

        [HttpDelete("remove")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<CombineCartSM>>> RemoveFromCart(
            [FromBody] ApiRequest<RemoveCartItemRequestSM> apiRequest)
        {
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

            var response = await _cartProcess.RemoveFromCart(
                userId,
                innerReq.ProductVariantId,
                innerReq.PlatformType);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("mine")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<CombineCartSM>>> GetMineCart()
        {
            
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _cartProcess.BuildCombineCartResponse(userId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region CLEAR CART

        [HttpDelete("clear")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<CombineCartSM>>> ClearCart(
            PlatformTypeSM platform)
        {
            
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _cartProcess.ClearCart(
                userId,
                platform);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}