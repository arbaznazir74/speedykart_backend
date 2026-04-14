using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Marketing;
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
    public class BannerController : ApiControllerWithOdataRoot<BannerSM>
    {
        private readonly BannerProcess _bannerProcess;

        public BannerController(BannerProcess process)
            : base(process)
        {
            _bannerProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BannerSM>>>> GetAsOdata(
            ODataQueryOptions<BannerSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region GET

        [HttpGet]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<BannerSM>>>> GetAll(int skip, int top)
        {
            var response = await _bannerProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCount()
        {
            var response = await _bannerProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
            [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BannerSM>>> GetById(long id)
        {
            var response = await _bannerProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("default/{bannerType}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BannerSM>>> GetDefaultByType(BannerTypeSM bannerType, PlatformTypeSM platform)
        {
            var response = await _bannerProcess.GetByBannerTypeAndDefaultAsync(bannerType, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("type/{bannerType}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<BannerSM>>>> GetAllByType(
            BannerTypeSM bannerType, PlatformTypeSM platform,
            int skip,
            int top)
        {
            var response = await _bannerProcess.GetAllByBannerType(bannerType,platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("type/count/{bannerType}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllByTypeCount(
            BannerTypeSM bannerType, PlatformTypeSM platform)
        {
            var response = await _bannerProcess.GetAllByBannerTypeCount(bannerType, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("type/product-banner/platform")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<BannerSM>>>> GetAllByProductBannerType(
            PlatformTypeSM platform,
            int skip,
            int top)
        {
            var response = await _bannerProcess.GetAllByProductBannerType(platform, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("type/product-banner/platform/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllByProductBannerTypeCount(
            PlatformTypeSM platform)
        {
            var response = await _bannerProcess.GetAllByProductBannerTypeCount(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region CREATE

        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Create(
            [FromBody] ApiRequest<BannerSM> apiRequest)
        {
            #region Check Request
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            #endregion

            var response = await _bannerProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BannerSM>>> Update(
            long id,
            [FromBody] ApiRequest<BannerSM> apiRequest)
        {
            #region Check Request
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            #endregion

            var response = await _bannerProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPut("default-status/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateDefaultStatus(
            long id,
            bool isDefault)
        {
            var response = await _bannerProcess.UpdateDefaultStatusAsync(id, isDefault);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var response = await _bannerProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Product Banner

        [HttpGet("products/hot-box/{bannerId}")]
        [Authorize(
             AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
             Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserHotBoxProductSM>>>> GetAllHotBoxProductsInBanner(long bannerId, int skip, int top)
        {
            var response = await _bannerProcess.GetAllHotBoxProductsInBanner(bannerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("products/hot-box/count/{bannerId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllProductsInBannerCount(long bannerId)
        {
            var response = await _bannerProcess.GetAllHotBoxProductsInBannerCount(bannerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("products/speedy-mart/{bannerId}")]
        [Authorize(
             AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
             Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<UserSpeedyMartProductSM>>>> GetAllSpeedyMartProductsInBanner(long bannerId, int skip, int top)
        {
            var response = await _bannerProcess.GetAllSpeedyMartProductsInBanner(bannerId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("products/speedy-mart/count/{bannerId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllSpeedyMartProductsInBannerCount(long bannerId, PlatformTypeSM platform)
        {
            var response = await _bannerProcess.GetAllSpeedyMartProductsInBannerCount(bannerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #region Add Product To Banner

        [HttpPost("add-product")] //Add Product To Banner
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<ProductBannerSM>>> AddProductToBanner([FromBody] ApiRequest<ProductBannerSM> apiRequest)
        {
            #region Check Request
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            #endregion
            var response = await _bannerProcess.AddProductsToBanner(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Add Product To Banner

        #region Delete Product From Banner

        [HttpDelete("delete-product/{productBannerId}")] 
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteProductFromBanner(long productBannerId)
        {
            var response = await _bannerProcess.DeleteProductFromBanner(productBannerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion Delete Product From Banner

        #endregion Product Banner
    }
}
