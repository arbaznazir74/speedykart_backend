using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductUnitController : ControllerBase
    {
        private readonly ProductUnitProcess _productUnitProcess;

        public ProductUnitController(ProductUnitProcess process)
        {
            _productUnitProcess = process;
        }

        #region GET

        #region Get By Product Variant Id

        [HttpGet("variant/{productVariantId}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller, User")]
        public async Task<ActionResult<ApiResponse<ProductUnitSM>>> GetByProductVariantId(long productVariantId)
        {
            var response = await _productUnitProcess.GetByProductVariantId(productVariantId);

            /*if (response == null)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    "Product unit not found",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }*/

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Id

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductUnitSM>>> GetById(long id)
        {
            var response = await _productUnitProcess.GetByPId(id);

            if (response == null)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    "Product unit not found",
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #endregion GET

        #region ADD

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductUnitSM>>> Add(
            [FromBody] ApiRequest<ProductUnitSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productUnitProcess.AddProductUnit(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Delete(long id)
        {
            var result = await _productUnitProcess.DeleteProductUnit(id);

            return ModelConverter.FormNewSuccessResponse(
                new BoolResponseRoot(result, "Product unit deleted successfully"));
        }

        #endregion
    }
}