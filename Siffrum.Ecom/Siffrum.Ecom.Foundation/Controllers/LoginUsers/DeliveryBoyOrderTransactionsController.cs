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
    public class DeliveryBoyOrderTransactionsController
        : ApiControllerWithOdataRoot<DeliveryBoyOrderTransactionsSM>
    {
        private readonly DeliveryBoyOrderTransactionsProcess _process;

        public DeliveryBoyOrderTransactionsController(DeliveryBoyOrderTransactionsProcess process)
            : base(process)
        {
            _process = process;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DeliveryBoyOrderTransactionsSM>>>> GetAsOdata(
            ODataQueryOptions<DeliveryBoyOrderTransactionsSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        #endregion

        #region CREATE

        // 🔹 Delivery Boy Payment to Admin
        [HttpPost("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyOrderTransactionsSM>>> AddPayment(
            [FromBody] ApiRequest<DeliveryBoyOrderTransactionsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            innerReq.DeliveryBoyId = dBoyId;

            var response = await _process.AddPaymentAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyOrderTransactionsSM>>> CreatePaymentByAdmin(
            [FromBody] ApiRequest<DeliveryBoyOrderTransactionsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _process.AddPaymentAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyOrderTransactionsSM>>>> GetAll(
            int skip = 0, int top = 10)
        {
            var response = await _process.GetAllAsync(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("delivery-boy")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyOrderTransactionsSM>>>> GetByDeliveryBoyId(
            long deliveryBoyId, int skip = 0, int top = 10)
        {
            var response = await _process.GetByDeliveryBoyIdAsync(deliveryBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyOrderTransactionsSM>>>> GetMine(
            int skip = 0, int top = 10)
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();

            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _process.GetByDeliveryBoyIdAsync(dBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyOrderTransactionsSM>>> GetMineById(long id)
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();

            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _process.GetMineByIdAsync(id, dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCount()
        {
            var response = await _process.GetAllCountAsync();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("delivery-boy/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCountByDeliveryBoyId(long deliveryBoyId)
        {
            var response = await _process.GetByDeliveryBoyIdCountAsync(deliveryBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMineCount()
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();

            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _process.GetByDeliveryBoyIdCountAsync(dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // 🔥 Ledger Summary
        [HttpGet("summary")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyLedgerSummarySM>>> GetSummary(long deliveryBoyId)
        {
            var response = await _process.GetLedgerSummaryAsync(deliveryBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/summary")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyLedgerSummarySM>>> GetMineSummary()
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();

            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _process.GetLedgerSummaryAsync(dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyOrderTransactionsSM>>> Update(
            long id,
            [FromBody] ApiRequest<DeliveryBoyOrderTransactionsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            innerReq.Id = id;

            var response = await _process.UpdateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Delete(long id)
        {
            var response = await _process.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion
    }
}