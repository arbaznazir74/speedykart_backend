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
    public class AddOnController : ApiControllerWithOdataRoot<AddOnProductsSM>
    {
        private readonly AddOnProductProcess _addonProcess;

        public AddOnController(AddOnProductProcess process)
            : base(process)
        {
            _addonProcess = process;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AddOnProductsSM>>>> GetAsOdata(
            ODataQueryOptions<AddOnProductsSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        #endregion

        #region Get

        #region GetAll

        [HttpGet]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<AddOnProductsSM>>>> GetAll(int skip = 0, int top = 10)
        {
            var response = await _addonProcess.GetAllAddOns(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _addonProcess.GetAllAddOnsCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Id

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<AddOnProductsSM>>> GetById(long id)
        {
            var response = await _addonProcess.GetAddOnById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Main Product

        [HttpGet("product-variant/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<AddonProductResponseSM>>> GetByMainProduct(long productVariantId)
        {
            var response = await _addonProcess.GetByMainProduct(productVariantId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("product-variant/admin/{productVariantId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<AddOnProductsSM>>>> GetByAdminWithMainProduct(long productVariantId)
        {
            var response = await _addonProcess.GetAddOnByMainProductId(productVariantId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get By Product Id (product-level)

        [HttpGet("product/admin/{productId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<AddOnProductsSM>>>> GetByProduct(long productId)
        {
            var response = await _addonProcess.GetAddOnsByProductId(productId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #endregion

        #region Add

        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<AddOnProductsSM>>> Add(
            [FromBody] ApiRequest<AddOnProductsSM> apiRequest)
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

            var response = await _addonProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("by-product")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<AddOnProductsSM>>> AddByProduct(
            [FromBody] ApiRequest<AddOnProductsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _addonProcess.CreateByProductAsync(innerReq.MainProductId, innerReq.AddonProductId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Delete

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var response = await _addonProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}