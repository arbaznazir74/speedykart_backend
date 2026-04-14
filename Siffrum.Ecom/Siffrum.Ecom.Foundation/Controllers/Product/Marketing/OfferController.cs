using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
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
    public class OfferController : ApiControllerWithOdataRoot<OffersAndCouponsSM>
    {
        private readonly OffersAndCouponsProcess _offerProcess;

        public OfferController(OffersAndCouponsProcess process)
            : base(process)
        {
            _offerProcess = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OffersAndCouponsSM>>>> GetAsOdata(
            ODataQueryOptions<OffersAndCouponsSM> oDataOptions)
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
            [FromBody] ApiRequest<OffersAndCouponsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _offerProcess.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region Get All and Count

        #region GET ALL (ADMIN)

        [HttpGet()]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<List<OffersAndCouponsSM>>>> GetAllOffersForAdmin(
            int skip, int top)
        {
            var response = await _offerProcess.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllOffersForAdminCount()
        {
            var response = await _offerProcess.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #endregion Get All and Count

        #region READ BY ID

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User, Seller, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<OffersAndCouponsSM>>> GetOfferById(long id)
        {
            var response = await _offerProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<OffersAndCouponsSM>>> UpdateOffer(
            long id,
            [FromBody] ApiRequest<OffersAndCouponsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _offerProcess.UpdateAsync(id, innerReq);
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
            var response = await _offerProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region User Offer Handler

        [HttpPost("handle")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateOfferData(
            long offerId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _offerProcess.AddOrIncrementOffer(userId, offerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("is-offer-valid")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> IsOfferValid(
            long offerId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _offerProcess.IsOfferValid(userId, offerId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion User Offer Handler

    }
}
