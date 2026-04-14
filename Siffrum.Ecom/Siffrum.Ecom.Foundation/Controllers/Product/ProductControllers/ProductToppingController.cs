using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductToppingController : ApiControllerRoot
    {
        private readonly ProductToppingProcess _productToppingProcess;

        public ProductToppingController(ProductToppingProcess productToppingProcess)
        {
            _productToppingProcess = productToppingProcess;
        }

        #region CREATE
        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AddToppingToProduct(
            [FromBody] ApiRequest<ProductToppingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productToppingProcess.AddToppingToProduct(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region READ
        [HttpGet("product/{productId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller, SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<List<ProductToppingSM>>>> GetByProductId(long productId)
        {
            var response = await _productToppingProcess.GetByProductId(productId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("by-topping/{toppingId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductToppingSM>>>> GetByToppingId(long toppingId)
        {
            var response = await _productToppingProcess.GetByToppingId(toppingId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region UPDATE
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateProductTopping(
            long id, [FromBody] ApiRequest<ProductToppingSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productToppingProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> RemoveFromProduct(long id)
        {
            var response = await _productToppingProcess.RemoveFromProduct(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion
    }
}
