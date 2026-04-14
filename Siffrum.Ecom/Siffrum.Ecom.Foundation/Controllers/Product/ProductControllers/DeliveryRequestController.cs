using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Delivery;
using Siffrum.Ecom.BAL.Foundation.Web;
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
    public class DeliveryRequestController : ApiControllerWithOdataRoot<DeliveryRequestSM>
    {
        private readonly DeliveryRequestProcess _process;

        public DeliveryRequestController(DeliveryRequestProcess process)
            : base(process)
        {
            _process = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DeliveryRequestSM>>>> GetAsOdata(
            ODataQueryOptions<DeliveryRequestSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }
        #endregion

        #region CREATE

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> CreateRequest(
            [FromBody] ApiRequest<DeliveryRequestSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            // 🔒 Ensure logged-in user is used
            innerReq.UserId = userId;

            var response = await _process.CreateAsync(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestSM>>>> GetAll(int skip = 0, int top = 50)
        {
            var response = await _process.GetAll(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var response = await _process.GetAllCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }
        
        [HttpGet("platform")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestSM>>>> GetAllByPlatform(PlatformTypeSM platformType, int skip = 0, int top = 50)
        {
            var response = await _process.GetAllByPlatform(platformType, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("platform/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllByPlatformCount(PlatformTypeSM platformType)
        {
            var response = await _process.GetAllByPlatformTypeCount(platformType);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestSM>>>> GetByUser(int skip = 0, int top = 50)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _process.GetByUser(userId, skip, top);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetUserCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }


            var response = await _process.GetByUserCount(userId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestSM>>> GetById(long id)
        {
            var response = await _process.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE (Resolve Request - Admin Only)

        [HttpPut("resolve/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestSM>>> ResolveRequest(
            long id,
            [FromQuery] string remarks,
            [FromQuery] bool isApproved)
        {
            var response = await _process.ResolveRequest(id, remarks, isApproved);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var response = await _process.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }

}
