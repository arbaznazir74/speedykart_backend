using AutoMapper.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.DomainModels.Enums;
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
    public class InvoiceController : ApiControllerWithOdataRoot<InvoiceSM>
    {
        private readonly InvoiceProcess _invoiceProcess;

        public InvoiceController(InvoiceProcess invoiceProcess)
            : base(invoiceProcess)
        {
            _invoiceProcess = invoiceProcess;
        }

        #region ODATA

        [HttpGet("odata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceSM>>>> GetAsOdata(
            ODataQueryOptions<InvoiceSM> oDataOptions)
        {
            var retList = await GetAsEntitiesOdata(oDataOptions);
            return Ok(ModelConverter.FormNewSuccessResponse(retList));
        }

        #endregion

        // =========================================================
        // CREATE (Only from backend after payment success)
        // =========================================================

        [HttpPost]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<InvoiceSM>>> CreateInvoice(
            long orderId) // pass OrderId
        {
            if (orderId == null || orderId <= 0)
            {
                return BadRequest(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_ReqDataNotFormed,
                    ApiErrorTypeSM.InvalidInputData_NoLog));
            }

            var response = await _invoiceProcess.CreateInvoiceFromOrderAsync(orderId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // =========================================================
        // READ - ADMIN
        // =========================================================

        [HttpGet]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<List<InvoiceSM>>>> GetAllInvoices(int skip, int top)
        {
            var response = await _invoiceProcess.GetAllAsync(skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetInvoicesCount()
        {
            var response = await _invoiceProcess.GetCountAsync();
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<InvoiceExtendedSM>>> GetInvoiceById(long id)
        {
            var response = await _invoiceProcess.GetByIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("order/{orderId}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<InvoiceExtendedSM>>> GetInvoiceByOrderId(long id)
        {
            var response = await _invoiceProcess.GetByOrderIdAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // =========================================================
        // READ - USER (My Invoices)
        // =========================================================

        [HttpGet("mine")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<InvoiceSM>>>> GetMyInvoices(int skip, int top)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _invoiceProcess.GetMyInvoicesAsync(userId, skip, top);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/count")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<IntResponseRoot>>> GetMyInvoicesCount()
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _invoiceProcess.GetMyInvoicesCountAsync(userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        [HttpGet("mine/{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<InvoiceExtendedSM>>> GetMyInvoiceById(long id)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(
                    DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }

            var response = await _invoiceProcess.GetMyInvoiceByIdAsync(id, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // =========================================================
        // UPDATE PAYMENT STATUS
        // =========================================================

        [HttpPut("{id}/payment-status")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdatePaymentStatus(
            long id,
           PaymentStatusSM status)
        {           

            var response = await _invoiceProcess.UpdatePaymentStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // =========================================================
        // UPDATE ORDER STATUS
        // =========================================================

        [HttpPut("{id}/order-status")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<BoolResponseRoot>>> UpdateOrderStatus(
            long id,
            OrderStatusSM status)
        {           

            var response = await _invoiceProcess.UpdateOrderStatusAsync(id, status);
            return ModelConverter.FormNewSuccessResponse(response);
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpDelete("{id}")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "SuperAdmin, SystemAdmin")]
        public async Task<ActionResult<ApiResponse<DeleteResponseRoot>>> DeleteInvoice(long id)
        {
            var response = await _invoiceProcess.DeleteAsync(id);
            return ModelConverter.FormNewSuccessResponse(response);
        }
    }
}