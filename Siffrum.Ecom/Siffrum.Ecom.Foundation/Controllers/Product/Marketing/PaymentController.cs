using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.BAL.Product;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.Foundation.Controllers.Base;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.v1;
using System.Text;

namespace Siffrum.Ecom.Foundation.Controllers.Product.Marketing
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ApiControllerRoot
    {
        #region Properties
        private readonly PaymentProcess _paymentProcess;
        private readonly APIConfiguration _apiConfiguration;

        #endregion Properties

        #region Constructor
        public PaymentController(PaymentProcess paymentProcess, APIConfiguration apiConfiguration)
        {
            _paymentProcess = paymentProcess;
            _apiConfiguration = apiConfiguration;
        }
        #endregion Constructor

        #region Stripe Methods

        #region Create Checkout Session
        /// <summary>
        /// Create Checkout Session ID for priceId
        /// </summary>
        /// <param name="apiRequest"></param>
        /// <returns></returns>
        [HttpPost("mine/checkout")]
        [Authorize(
            AuthenticationSchemes = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema,
            Roles = "User")]
        public async Task<ActionResult<ApiResponse<OrderSM>>> CreateCheckoutSession(long orderId)
        {
            var userId = User.GetUserRecordIdFromCurrentUserClaims();
            if (userId <= 0)
            {
                return NotFound(ModelConverter.FormNewErrorResponse(DomainConstantsRoot.DisplayMessagesRoot.Display_Id_NotFound));
            }
            var response = await _paymentProcess.CreateOrderAndGeneratePaymentLink(orderId, userId);
            return ModelConverter.FormNewSuccessResponse(response);
        }
        #endregion Create Checkout Session

        #region Webhook

        /// <summary>
        /// Razorpay webhook endpoint
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            Request.EnableBuffering(); // important

            string payload;
            using (var reader = new StreamReader(
                Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true))
            {
                payload = await reader.ReadToEndAsync();
                Request.Body.Position = 0; // reset for safety
            }

            if (string.IsNullOrWhiteSpace(payload))
                return BadRequest("Empty payload received.");

            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signature))
                return BadRequest("Missing signature header.");

            await _paymentProcess.HandleWebhookAsync(payload, signature);

            return Ok();
        }


        #endregion Webhook
        #endregion Stripe Methods
    }
}

    