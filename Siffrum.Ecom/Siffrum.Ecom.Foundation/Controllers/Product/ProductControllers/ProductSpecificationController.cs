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
    public class ProductSpecificationController
        : ApiControllerWithOdataRoot<ProductSpecificationSM>
    {
        private readonly ProductSpecificationProcess _productSpecificationProcess;

        public ProductSpecificationController(ProductSpecificationProcess process)
            : base(process)
        {
            _productSpecificationProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSpecificationSM>>>> GetAsOdata(
            ODataQueryOptions<ProductSpecificationSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion


        #region ADD SPECIFICATIONS TO VARIANT

        [HttpPost("variant/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> AddSpecifications(
            long productVariantId,
            [FromBody] ApiRequest<List<ProductSpecificationSM>> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var result = await _productSpecificationProcess
                .AddSpecificationsAsync(productVariantId, innerReq);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region GET BY PRODUCT VARIANT

        [HttpGet("variant/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductSpecificationSM>>>> GetByVariant(
            long productVariantId)
        {
            var result = await _productSpecificationProcess
                .GetByVariantIdAsync(productVariantId);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationSM>>> GetById(
            long id)
        {
            var result = await _productSpecificationProcess
                .GetByIdAsync(id);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region UPDATE SPECIFICATION

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationSM>>> Update(
            long id,
            [FromBody] ApiRequest<ProductSpecificationSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var result = await _productSpecificationProcess.UpdateAsync(id, innerReq);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region DELETE SPECIFICATION

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var result = await _productSpecificationProcess.DeleteAsync(id);

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion


        #region ADMIN GET ALL

        [HttpGet("admin")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<ProductSpecificationSM>>>> GetAllForAdmin(
            [FromQuery] int skip = 0,
            [FromQuery] int top = 10)
        {
            var result = await _productSpecificationProcess.GetAllForAdmin(skip, top);

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
            var result = await _productSpecificationProcess.GetAllForAdminCount();

            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion
    }
}
