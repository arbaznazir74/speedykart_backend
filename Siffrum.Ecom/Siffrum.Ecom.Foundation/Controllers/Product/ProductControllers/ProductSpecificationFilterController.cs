using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class ProductSpecificationFilterController : ApiControllerRoot
    {
        private readonly ProductSpecificationFilterProcess _specificationFilterProcess;

        public ProductSpecificationFilterController(
            ProductSpecificationFilterProcess process)
        {
            _specificationFilterProcess = process;
        }       

        #region CREATE

        [HttpPost("{categoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateSpecification(
            long categoryId,
            [FromBody] ApiRequest<ProductSpecificationFilterSM> apiRequest)
        {
            var req = apiRequest?.ReqData;
            if (req == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _specificationFilterProcess
                .CreateAsync(req, categoryId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("values/{filterId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateSpecificationValues(
            long filterId,
            [FromBody] ApiRequest<List<ProductSpecificationValueSM>> apiRequest)
        {
            var req = apiRequest?.ReqData;
            if (req == null || !req.Any())
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _specificationFilterProcess
                .CreateSpecificationValuesAsync(req, filterId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationFilterSM>>> GetById(long id)
        {
            var response = await _specificationFilterProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("value/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationValueSM>>> GetFilterValueById(long id)
        {
            var response = await _specificationFilterProcess.GetFilterValueByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("category/{categoryId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductSpecificationFilterSM>>>> GetByCategory(
            long categoryId)
        {
            var response = await _specificationFilterProcess
                .GetByCategoryIdAsync(categoryId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationFilterSM>>> UpdateFilter(
            long id,
            [FromBody] ApiRequest<ProductSpecificationFilterSM> apiRequest)
        {
            var req = apiRequest?.ReqData;
            if (req == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _specificationFilterProcess
                .UpdateFilterAsync(id, req);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("value/{valueId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationValueSM>>> UpdateValue(
            long valueId,
            [FromBody] ApiRequest<ProductSpecificationValueSM> apiRequest)
        {
            var req = apiRequest?.ReqData;
            if (req == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _specificationFilterProcess
                .UpdateValueAsync(valueId, req);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteFilter(long id)
        {
            var response = await _specificationFilterProcess.DeleteFilterAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("value/{valueId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteValue(long valueId)
        {
            var response = await _specificationFilterProcess.DeleteValueAsync(valueId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #region Product Specification Filter Value

        [HttpGet("product/filter/{productFilterId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductSpecificationFilterValueSM>>> GetProductSpecificationFiltersByIdAsync(
            long productFilterId)
        {
            var response = await _specificationFilterProcess
                .GetProductSpecificationFiltersValueByIdAsync(productFilterId);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("product/filters/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductSpecificationFilterValueSM>>>> GetProductSpecificationFiltersAsync(
            long productVariantId)
        {
            var response = await _specificationFilterProcess
                .GetProductSpecificationFiltersAsync(productVariantId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("product/filters/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductSpecificationFilterValueSM>>>> GetProductSpecificationFilterAsync(
            long productVariantId,
            [FromBody] ApiRequest<List<ProductSpecificationFilterValueSM>> apiRequest)
        {
            #region Request Data

            var req = apiRequest?.ReqData;
            if (req == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Request Data
            var response = await _specificationFilterProcess
                .AddProductSpecificationFiltersAsync(productVariantId, req);

            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpPut("product/filters/{productFilterId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductSpecificationFilterValueSM>>>> UpdateProductSpecificationFilterAsync(
            long productFilterId,
            [FromBody] ApiRequest<ProductSpecificationFilterValueSM> apiRequest)
        {
            #region Request Data

            var req = apiRequest?.ReqData;
            if (req == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            #endregion Request Data
            var response = await _specificationFilterProcess
                .UpdateProductSpecificationFilterAsync(productFilterId, req);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("product/filters/{productFilterId}")]
        [Authorize(
           AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteProductSpecificationFilterAsync(long productFilterId)
        {
            var response = await _specificationFilterProcess.DeleteProductSpecificationFilterAsync(productFilterId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Product Specification Filter Value
    }
}
