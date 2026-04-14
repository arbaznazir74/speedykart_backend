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
    public class ProductImagesController : ApiControllerWithOdataRoot<ProductImagesSM>
    {
        private readonly ProductImagesProcess _productImagesProcess;

        public ProductImagesController(ProductImagesProcess process)
            : base(process)
        {
            _productImagesProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductImagesSM>>>> GetAsOdata(
            ODataQueryOptions<ProductImagesSM> oDataOptions)
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
        public async Task<ActionResult<ApiResponse<List<ProductImagesSM>>>> CreateProductImagesList(long productVariantId,
            [FromBody] ApiRequest<List<ProductImagesSM>> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productImagesProcess.AddMultipleProductImagesAsync(productVariantId, innerReq);
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
        public async Task<ActionResult<ApiResponse<List<ProductImagesSM>>>> GetAllProductImages(
            int skip, int top)
        {
            var response = await _productImagesProcess.GetAllProductImages(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllProductImagesCount()
        {
            var response = await _productImagesProcess.GetAllProductImagesCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Get By Product Variant
        [HttpGet("by-product-variant/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductImagesSM>>>> GetProductImagesByProductVariantId(
            long productVariantId)
        {
            var response = await _productImagesProcess
                .GetProductImages(productVariantId);

            return ModelConverter.FormNewSuccessResponse(response);
        }       
        #endregion

        #region Get By Id
        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<ProductImagesSM>>> GetProductImageById(long id)
        {
            var response = await _productImagesProcess.GetById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #endregion

        #region UPDATE
        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductImagesSM>>> UpdateProductImage(
            long id, [FromBody] ApiRequest<ProductImagesSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productImagesProcess.UpdateProductImage(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteProductFaq(long id)
        {
            var response = await _productImagesProcess.DeleteProductImage(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion
    }
}
