using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.LoginUsers;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;

namespace Siffrum.Ecom.Foundation.Controllers.LoginUsers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DeliveryInstructionController : ApiControllerWithOdataRoot<DeliveryInstructionsSM>
    {
        private readonly DeliveryInstuctionProcess _process;
        public DeliveryInstructionController(DeliveryInstuctionProcess process)
            : base(process)
        {
            _process = process;
        }

        #region ODATA
        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DeliveryInstructionsSM>>>> GetAsOdata(
            ODataQueryOptions<DeliveryInstructionsSM> options)
        {
            var data = await GetAsEntitiesOdata(options);
            return Ok(ModelConverter.FormNewSuccessResponse(data));
        }
        #endregion

        #region ADMIN GET

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryAvailabiltySM>>>> GetAll(int skip, int top)
        {
            var data = await _process.GetAllDeliveryInstructions(skip, top);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetAllCount()
        {
            var data = await _process.GetAllCount();
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryInstructionsSM>>> GetById(long id)
        {
            var data = await _process.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion

        #region GET MINE

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeliveryInstructionsSM>>> GetMine()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();

            if (userId <= 0)
            {
                return NotFound(
                    ModelConverter.FormNewErrorResponse(
                        DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.GetByUserId(userId);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpGet("user/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "DeliveryBoy, SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryInstructionsSM>>> GetUserDeliveryInstruction(long id)
        {          

            var data = await _process.GetByUserId(id);
            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion

        #region CREATE

        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeliveryInstructionsSM>>> Create(
            [FromBody] ApiRequest<DeliveryInstructionsSM> request)
        {
            var innerReq = request?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }
            var userId = User.GetUserRecordIdFromCurrentUserClaims();

            if (userId <= 0)
            {
                return NotFound(
                    ModelConverter.FormNewErrorResponse(
                        DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.CreateAsync(userId, innerReq);

            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion

        #region UPDATE

        [HttpPut("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeliveryInstructionsSM>>> UpdateMine(
            [FromBody] ApiRequest<DeliveryInstructionsSM> request)
        {
            var innerReq = request?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var userId = User.GetUserRecordIdFromCurrentUserClaims();

            if (userId <= 0)
            {
                return NotFound(
                    ModelConverter.FormNewErrorResponse(
                        DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.UpdateMine(userId, innerReq);

            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryInstructionsSM>>> Update(
            long id,
            [FromBody] ApiRequest<DeliveryInstructionsSM> request)
        {
            var innerReq = request?.ReqData;
            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_ReqDataNotFormed, ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var data = await _process.UpdateAsync(id, innerReq);

            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion

        #region DELETE

        [HttpDelete("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "User")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteMine()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();

            if (userId <= 0)
            {
                return NotFound(
                    ModelConverter.FormNewErrorResponse(
                        DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var data = await _process.DeleteMine(userId);

            return ModelConverter.FormNewSuccessResponse(data);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin,SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> Delete(long id)
        {
            var data = await _process.DeleteAsync(id);

            return ModelConverter.FormNewSuccessResponse(data);
        }

        #endregion
    }
}
