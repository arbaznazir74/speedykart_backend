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

namespace Siffrum.Ecom.Foundation.Controllers.Product.Marketing
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromotionalContentController : ApiControllerWithOdataRoot<PromotionalContentSM>
    {
        private readonly PromotionalContentProcess _pcProcess;

        public PromotionalContentController(PromotionalContentProcess process)
            : base(process)
        {
            _pcProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PromotionalContentSM>>>> GetAsOdata(
            ODataQueryOptions<PromotionalContentSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE

        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateOffer(
            [FromBody] ApiRequest<PromotionalContentSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _pcProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get All and Count

        #region GET ALL (ADMIN)

        [HttpGet()]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<PromotionalContentSM>>>> GetAllOffersForAdmin(
            int skip, int top)
        {
            var response = await _pcProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllOffersForAdminCount()
        {
            var response = await _pcProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("platform")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<PromotionalContentSM>>>> GetAllByPlatform(PlatformTypeSM platform,
            int skip, int top)
        {
            var response = await _pcProcess.GetAllByPlatform(platform,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("platform/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllByPlatformCount(PlatformTypeSM platform)
        {
            var response = await _pcProcess.GetAllCountByPlatform(platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("location")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<List<PromotionalContentSM>>>> GetAllByDisplayLocation(
           PromotionDisplayLocationSM displayLocation, PlatformTypeSM platform, int skip, int top)
        {
            var response = await _pcProcess.GetAllByDisplayLocation(displayLocation, platform,skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("location/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User, Seller")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllByDisplayLocationCount(PromotionDisplayLocationSM displayLocation, PlatformTypeSM platform)
        {
            var response = await _pcProcess.GetAllByDisplayLocationCount(displayLocation, platform);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion


        #endregion Get All and Count

        #region READ BY ID

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User, Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<PromotionalContentSM>>> GetOfferById(long id)
        {
            var response = await _pcProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<PromotionalContentSM>>> UpdateOffer(
            long id,
            [FromBody] ApiRequest<PromotionalContentSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _pcProcess.UpdateAsync(id, innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion        

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteOffer(long id)
        {
            var response = await _pcProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

    }
}
