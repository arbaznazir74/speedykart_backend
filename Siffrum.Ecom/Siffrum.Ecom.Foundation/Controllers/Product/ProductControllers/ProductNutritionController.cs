using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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
    public class ProductNutritionController
        : ApiControllerWithOdataRoot<ProductNutritionDataSM>
    {
        private readonly ProductNutritionProcess _productNutritionProcess;

        public ProductNutritionController(ProductNutritionProcess process)
            : base(process)
        {
            _productNutritionProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductNutritionDataSM>>>> GetAsOdata(
            ODataQueryOptions<ProductNutritionDataSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion


        #region ADD / UPDATE NUTRITION

        [HttpPost("{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AddOrUpdate(
            long productVariantId,
            [FromBody] ApiRequest<ProductNutritionDataSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var result = await _productNutritionProcess
                .AddOrUpdateAsync(productVariantId, innerReq);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region GET BY PRODUCT VARIANT

        [HttpGet("variant/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductNutritionDataSM>>> GetByVariant(
            long productVariantId)
        {
            var result = await _productNutritionProcess
                .GetByVariantIdAsync(productVariantId);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductNutritionDataSM>>> GetById(
            long id)
        {
            var result = await _productNutritionProcess
                .GetByIdAsync(id);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region DELETE NUTRITION

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var result = await _productNutritionProcess.DeleteAsync(id);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region ADMIN GET ALL

        [HttpGet("admin")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductNutritionDataSM>>>> GetAllForAdmin(
            [FromQuery] int skip = 0,
            [FromQuery] int top = 10)
        {
            var result = await _productNutritionProcess.GetAllForAdmin(skip, top);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region ADMIN COUNT

        [HttpGet("admin/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllForAdminCount()
        {
            var result = await _productNutritionProcess.GetAllForAdminCount();

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion
    }
}
