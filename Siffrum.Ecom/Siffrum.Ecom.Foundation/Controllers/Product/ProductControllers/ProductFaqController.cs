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
    public class ProductFaqController : ApiControllerWithOdataRoot<ProductFaqSM>
    {
        private readonly ProductFaqProcess _productFaqProcess;

        public ProductFaqController(ProductFaqProcess productFaqProcess)
            : base(productFaqProcess)
        {
            _productFaqProcess = productFaqProcess;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductFaqSM>>>> GetAsOdata(
            ODataQueryOptions<ProductFaqSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE

        #region Create List
        [HttpPost("bulk")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateProductFaqList(long productVariantId,
            [FromBody] ApiRequest<List<ProductFaqSM>> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productFaqProcess.CreateListAsync(productVariantId, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Create Single
        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateProductFaq(
            [FromBody] ApiRequest<ProductFaqSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productFaqProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #endregion

        #region READ

        #region Get All
        [HttpGet]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductFaqSM>>>> GetAllProductFaqs(
            int skip, int top)
        {
            var response = await _productFaqProcess.GetAllProductFaqs(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetProductFaqCount()
        {
            var response = await _productFaqProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Get By Product Variant
        [HttpGet("by-product-variant/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductFaqSM>>>> GetProductFaqsByProductVariantId(
            long productVariantId, int skip, int top)
        {
            var response = await _productFaqProcess
                .GetAllProductFaqsByProductVariantId(productVariantId, skip, top);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("by-product-variant/count/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetProductFaqCountByProductVariantId(
            long productVariantId)
        {
            var response = await _productFaqProcess.GetCountOfProductVariantId(productVariantId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Get By Id
        [HttpGet("id/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductFaqSM>>> GetProductFaqById(long id)
        {
            var response = await _productFaqProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #endregion

        #region UPDATE
        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")] //Todo: Handle separately for seller
        public async Task<ActionResult<ApiResponse<ProductFaqSM>>> UpdateProductFaq(
            long id, [FromBody] ApiRequest<ProductFaqSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productFaqProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")] //Todo: Handle separately for seller
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteProductFaq(long id)
        {
            var response = await _productFaqProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion
    }
}
