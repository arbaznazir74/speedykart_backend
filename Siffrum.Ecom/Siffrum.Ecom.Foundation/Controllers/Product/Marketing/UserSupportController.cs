using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Marketing;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using System.Text;

namespace Siffrum.Ecom.Foundation.Controllers.Product.Marketing
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserSupportController : ApiControllerWithOdataRoot<UserSupportRequestSM>
    {
        private readonly UserSupportProcess _process;

        public UserSupportController(UserSupportProcess process)
            : base(process)
        {
            _process = process;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserSupportRequestSM>>>> GetAsOdata(
            ODataQueryOptions<UserSupportRequestSM> oDataOptions)
        {
            var result = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(result));
        }

        #endregion

        #region CREATE

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Create(
            [FromBody] ApiRequest<UserSupportRequestSM> apiRequest)
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
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }
            innerReq.UserId = userId;

            var response = await _process.CreateAsync(innerReq);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<UserSupportRequestSM>>>> GetAll(int skip = 0, int top = 50)
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

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<UserSupportRequestSM>>>> GetByUser(int skip = 0, int top = 50)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
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
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _process.GetByUserCount(userId);

            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<UserSupportRequestSM>>> GetById(long id)
        {
            var response = await _process.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region RESOLVE (Admin)

        [HttpPut("resolve/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<UserSupportRequestSM>>> Resolve(
            long id,
            [FromQuery] string adminResponse)
        {
            var response = await _process.ResolveRequest(id, adminResponse);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region REPLY THREAD

        [HttpPost("{supportRequestId}/reply")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<UserSupportReplySM>>> AdminReply(
            long supportRequestId,
            [FromBody] ApiRequest<UserSupportReplySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null || string.IsNullOrWhiteSpace(innerReq.Message))
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));

            var adminId = User.GetUserRecordIdFromCurrentUserClaims();
            var response = await _process.AddReply(supportRequestId, innerReq.Message, "Admin", adminId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("{supportRequestId}/reply/user")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<UserSupportReplySM>>> UserReply(
            long supportRequestId,
            [FromBody] ApiRequest<UserSupportReplySM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;
            if (innerReq == null || string.IsNullOrWhiteSpace(innerReq.Message))
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));

            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            var response = await _process.AddReply(supportRequestId, innerReq.Message, "User", userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{supportRequestId}/replies")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<List<UserSupportReplySM>>>> GetReplies(
            long supportRequestId, int skip = 0, int top = 50)
        {
            var response = await _process.GetReplies(supportRequestId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{supportRequestId}/replies/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin, User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetRepliesCount(long supportRequestId)
        {
            var response = await _process.GetRepliesCount(supportRequestId);
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