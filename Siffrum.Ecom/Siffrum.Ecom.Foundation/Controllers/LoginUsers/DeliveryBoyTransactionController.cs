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
    public class DeliveryBoyTransactionController : ApiControllerWithOdataRoot<DeliveryBoyTransactionsSM>
    {
        private readonly DeliveryBoyTransactionProcess _process;
        public DeliveryBoyTransactionController(DeliveryBoyTransactionProcess process)
            : base(process)
        {
            _process = process;
        }

        [HttpGet]
        [Route("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DeliveryBoyTransactionsSM>>>> GetAsOdata(ODataQueryOptions<DeliveryBoyTransactionsSM> oDataOptions)
        {
            //oDataOptions.Filter = new FilterQueryOption();
            //TODO: validate inputs here probably 
            //if (oDataOptions.Filter == null)
            //    oDataOptions.Filter. = "$filter=organisationUnitId%20eq%20" + 10 + ",";
            var retList = await GetAsEntitiesOdata(oDataOptions);

            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        #region CREATE

        [HttpPost]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> Create(
            [FromBody] ApiRequest<DeliveryBoyTransactionsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _process.CreateAsync(innerReq);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region READ

        [HttpGet]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyTransactionsSM>>>> GetAll(
            int skip = 0,
            int top = 10)
        {
            var response = await _process.GetAll( skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        [HttpGet("delivery-boy")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyTransactionsSM>>>> GetAllByDeliveryBoyId(long deliveryBoyId,
            int skip = 0,
            int top = 10)
        {
            var response = await _process.GetAllByDeliveryBoyId(deliveryBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<List<DeliveryBoyTransactionsSM>>>> GetAllMine(
            int skip = 0,
            int top = 10)
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            }
            var response = await _process.GetAllByDeliveryBoyId(dBoyId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin,DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyTransactionsSM>>> GetById(long id)
        {
            var response = await _process.GetById(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCount()
        {
            var response = await _process.GetCount();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("delivery-boy/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetCountByDeliveryBoyId(long deliveryBoyId)
        {
            var response = await _process.GetCountByDeliveryBoyId(deliveryBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/count")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
           Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMIneCount()
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if(dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            }
            var response = await _process.GetCountByDeliveryBoyId(dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("total")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyPaymentsSM>>> GetTotalPaid(long deliveryBoyId)
        {
            var response = await _process.GetTotalPaidAmount(deliveryBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/total")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "DeliveryBoy")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyPaymentsSM>>> GetMineTotalPaid()
        {
            var dBoyId = User.GetUserRecordIdFromCurrentUserClaims();
            if (dBoyId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstants.DisplayMessagesRoot.Display_Id_NotFound));

            }
            var response = await _process.GetTotalPaidAmount(dBoyId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        #endregion

        #region UPDATE

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeliveryBoyTransactionsSM>>> Update(
            long id,
            [FromBody] ApiRequest<DeliveryBoyTransactionsSM> apiRequest)
        {
            var innerReq = apiRequest?.ReqData;

            if (innerReq == null)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _process.UpdateAsync(id, innerReq);
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
