using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.Product.ProductControllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductTagController : ApiControllerWithOdataRoot<TagSM>
    {
        private readonly ProductTagProcess _productTagProcess;

        public ProductTagController(ProductTagProcess productTagProcess)
            : base(productTagProcess)
        {
            _productTagProcess = productTagProcess;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TagSM>>>> GetAsOdata(
            ODataQueryOptions<TagSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region READ

        #region Get All
        [HttpGet]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<TagSM>>>> GetAllTags(
            int skip, int top)
        {
            var response = await _productTagProcess.GetAllTags(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllTagsCount()
        {
            var response = await _productTagProcess.GetAllTagsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("product-tags")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<TagSM>>>> GetAllProductTags(
            PlatformTypeSM platform, int skip, int top)
        {
            var response = await _productTagProcess.GetAllTagsWithProducts(platform,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("product-tags/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllProductTagsCount(PlatformTypeSM platform)
        {
            var response = await _productTagProcess.GetAllTagsWithProductsCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("hotbox/products/{tagId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserHotBoxProductSM>>>> GetAllHotBoxProductsInTag(
            int tagId, int skip, int top)
        {
            var response = await _productTagProcess.GetHotBoxProductsInTag(tagId,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("hotbox/products/count/{tagId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllHotBoxProductsInTagCount(long tagId)
        {
            var response = await _productTagProcess.GetHotBoxProductsInTagCount(tagId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("speedymart/products/{tagId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserSpeedyMartProductSM>>>> GetSpeedyMartProductsInTag(
            int tagId, int skip, int top)
        {
            var response = await _productTagProcess.GetSpeedyMartProductsInTag(tagId,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("speedymart/products/count/{tagId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSpeedyMartTagsCount(long tagId)
        {
            var response = await _productTagProcess.GetSpeedyMartProductsInTagCount(tagId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Id
        [HttpGet("id/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<TagSM>>> GetTagById(long id)
        {
            var response = await _productTagProcess.GetTagById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #endregion

        #region CREATE
        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<TagSM>>> AddTag(
            [FromBody] ApiRequest<TagSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productTagProcess.AddTag(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region UPDATE
        [HttpPut]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<TagSM>>> UpdateTag(
            [FromBody] ApiRequest<TagSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _productTagProcess.UpdateTag(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region DELETE
        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteTag(long id)
        {
            var response = await _productTagProcess.DeleteTag(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion

        #region Product Variant Tags

        [HttpGet("variant/{productVariantId}")] // Get Product Variant Tags By Product Variant Id
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<ProductTagSM>>>> GetProductVariantTagsByProductVariantId(long productVariantId)
        {
            var response = await _productTagProcess.GetByProductVariantId(productVariantId);
            return ModelConverter.FormNewSuccessResponse(response);
        }


        [HttpPost("variant")] 
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<ProductTagSM>>> AddProductVariantTagsByProductVariantId([FromBody] ApiRequest<ProductTagSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var response = await _productTagProcess.AddProductTag(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpDelete("variant/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteProductVariantTag(long id)
        {
            var response = await _productTagProcess.DeleteProductTag(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Product Variant Tags



    }
}
