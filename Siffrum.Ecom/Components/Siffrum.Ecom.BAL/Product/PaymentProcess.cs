using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using Siffrum.Ecom.BAL.Base.OneSignal;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.v1;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using System.Security.Cryptography;
using System.Text;

namespace Siffrum.Ecom.BAL.Product
{
    public class PaymentProcess : SiffrumBalBase
    {
        private readonly APIConfiguration _apiConfiguration;
        private readonly RazorpayClient _razorpayClient;
        private readonly NotificationProcess _notificationProcess;

        public PaymentProcess(
            IMapper mapper,NotificationProcess notificationProcess,
            ApiDbContext context,
            APIConfiguration apiConfiguration)
            : base(mapper, context)
        {
            _notificationProcess = notificationProcess;
            _apiConfiguration = apiConfiguration;
            _razorpayClient = new RazorpayClient(
               _apiConfiguration.RazorpaySettings.KeyId,
               _apiConfiguration.RazorpaySettings.KeySecret);
        }

        #region Create Payment Link

        public async Task<OrderSM> CreateOrderAndGeneratePaymentLink(long orderId, long userId)
        {
            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            // 1️⃣ Fetch Order
            var order = await _apiDbContext.Order
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            
            if (order == null)
                throw new Exception("Order not found for this user.");

            if (order.PaymentStatus == PaymentStatusDM.Paid)
                throw new Exception("Order already paid.");

            // 2️⃣ Fetch User separately using userId
            var user = await _apiDbContext.User
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");
            //var recieptId = $"Reciet#{Guid.NewGuid().ToString("N")}";
            var receiptId = $"Receipt#{Guid.NewGuid():N}";
            // 3️⃣ Ensure Razorpay Customer exists
            if (string.IsNullOrWhiteSpace(user.PaymentId))
            {
                var customerOptions = new Dictionary<string, object>
                {
                    { "name", user.Name ?? "Customer" },
                    { "email", user.Email ?? "" },
                    { "contact", user.Mobile }
                };                
                var customer = _razorpayClient.Customer.Create(customerOptions);
                user.PaymentId = customer["id"]?.ToString();
                _apiDbContext.User.Update(user);
                await _apiDbContext.SaveChangesAsync();
                
            }

            // 4️⃣ Create Razorpay Order (if not created already)
            if (string.IsNullOrWhiteSpace(order.RazorpayOrderId))
            {
                var orderOptions = new Dictionary<string, object>
                {
                    { "amount", Convert.ToInt64(order.Amount * 100) },
                    { "currency", order.Currency },
                    { "receipt", receiptId.ToString() ?? $"order_{order.Id}" },
                    { "payment_capture", 1 }
                };

                var razorOrder = _razorpayClient.Order.Create(orderOptions);

                order.RazorpayOrderId = razorOrder["id"]?.ToString();
                order.PaymentStatus = PaymentStatusDM.Pending;
                order.Receipt = receiptId;
                _apiDbContext.Order.Update(order);
            }

            // 5️⃣ Create Payment Link using Razorpay CustomerId
            var paymentLinkOptions = new Dictionary<string, object>
            {
                { "amount", Convert.ToInt64(order.Amount * 100) },
                { "currency", order.Currency },
                { "customer_id", user.PaymentId },
                { "reference_id", receiptId.ToString() },
                { "description", $"Payment for Order #{order.Receipt}" },
                //{ "order_id", order.RazorpayOrderId },
                { "callback_url", "https://google.com/payment-status" },
                { "callback_method", "get" },
                { "notify", new Dictionary<string, object> {
                { "sms", true },
                { "email", true }
                }
                }
            };
            var paymentLink = _razorpayClient.PaymentLink.Create(paymentLinkOptions);
            var paymentId = paymentLink["id"]?.ToString();

            order.RazorpayPaymentId = paymentId;
            await _apiDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return _mapper.Map<OrderSM>(order);
            //return paymentLink["short_url"]?.ToString() ?? "";
        }

        #endregion

        #region Webhook Handler

        public async Task HandleWebhookAsync(string payload, string signature)
        {
            ValidateWebhookSignature(payload, signature);

            var json = JObject.Parse(payload);
            var eventType = json["event"]?.ToString();

            switch (eventType)
            {
                case "payment.captured":
                    await HandlePaymentCaptured(json);
                    break;

                case "payment.failed":
                    await HandlePaymentFailed(json);
                    break;

                case "refund.processed":
                    await HandleRefundProcessed(json);
                    break;
            }
        }

        /*private async Task HandlePaymentCaptured(JObject json)
        {
            var entity = json["payload"]?["payment"]?["entity"];
            if (entity == null)
                return;

            var razorpayPaymentId = entity["id"]?.ToString();
            var razorpayOrderId = entity["order_id"]?.ToString();
            var description = entity["description"]?.ToString(); // "#SMYy2iW9VEOyhV"

            if (!decimal.TryParse(entity["amount"]?.ToString(), out var amountInPaise))
                return;

            var amountPaid = amountInPaise / 100m;

            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                OrderDM order = null;

                // 1️⃣ First try OrderId (if you created order API)
                if (!string.IsNullOrWhiteSpace(razorpayOrderId))
                {
                    order = await _apiDbContext.Order
                        .FirstOrDefaultAsync(o => o.RazorpayOrderId == razorpayOrderId);
                }

                // 2️⃣ If not found → Use PaymentLinkId from description
                if (order == null && !string.IsNullOrWhiteSpace(description))
                {
                    var paymentLinkId = "plink_" + description.TrimStart('#');

                    order = await _apiDbContext.Order
                        .FirstOrDefaultAsync(o => o.RazorpayPaymentId == paymentLinkId);
                }

                if (order == null)
                    return; // No matching order

                // 🔁 Idempotency
                if (order.PaymentStatus == PaymentStatusDM.Paid)
                    return;

                // ✅ Update order
                order.RazorpayPaymentId = razorpayPaymentId;
                order.PaidAmount = amountPaid;
                order.DueAmount = order.Amount - amountPaid;
                order.PaymentStatus = PaymentStatusDM.Paid;
                order.OrderStatus = OrderStatusDM.Processing;

                var invoice = await _apiDbContext.Invoice
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (invoice != null)
                {
                    invoice.PaymentStatus = PaymentStatusDM.Paid;
                    invoice.OrderStatus = OrderStatusDM.Processing;
                }

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(
                   ApiErrorTypeSM.Fatal_Log,
                   $"Failed to update payment detaills of order with RazorPayOrderId: {razorpayOrderId}, Message: {ex.Message}, InnerException: {ex.InnerException?.Message}, StackTrace: {ex.StackTrace}",
                   "Failed to update payment details for your order. Please contact support team."
                   );
            }
        }*/

        private async Task HandlePaymentCaptured(JObject json)
        {
            var entity = json["payload"]?["payment"]?["entity"];
            if (entity == null)
                return;

            var razorpayPaymentId = entity["id"]?.ToString();
            var razorpayOrderId = entity["order_id"]?.ToString();
            var description = entity["description"]?.ToString();

            if (!decimal.TryParse(entity["amount"]?.ToString(), out var amountInPaise))
                return;

            var amountPaid = amountInPaise / 100m;

            OrderDM order = null;

            try
            {
                // Find Order
                if (!string.IsNullOrWhiteSpace(razorpayOrderId))
                {
                    order = await _apiDbContext.Order
                        .FirstOrDefaultAsync(o => o.RazorpayOrderId == razorpayOrderId);
                }

                if (order == null && !string.IsNullOrWhiteSpace(description))
                {
                    var paymentLinkId = "plink_" + description.TrimStart('#');

                    order = await _apiDbContext.Order
                        .FirstOrDefaultAsync(o => o.RazorpayPaymentId == paymentLinkId);
                }

                if (order == null)
                    return;

                // Idempotency
                if (order.PaymentStatus == PaymentStatusDM.Paid)
                    return;

                await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

                // Update order payment
                order.RazorpayPaymentId = razorpayPaymentId;
                order.PaidAmount = amountPaid;
                order.DueAmount = order.Amount - amountPaid;
                order.PaymentStatus = PaymentStatusDM.Paid;
                order.OrderStatus = OrderStatusDM.Processing;
                order.UpdatedAt = DateTime.UtcNow;

                var invoice = await _apiDbContext.Invoice
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (invoice != null)
                {
                    invoice.PaymentStatus = PaymentStatusDM.Paid;
                    invoice.OrderStatus = OrderStatusDM.Processing;
                    invoice.UpdatedAt = DateTime.UtcNow;
                }

                // Stock already deducted at order creation time (CreateOrderAsync)

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                await SendOrderNotificationsAsync(order.Id);
            }
            catch (Exception ex)
            {
                // ⚠️ Payment captured but order processing failed
                if (order != null)
                {
                    try
                    {
                        order.PaymentStatus = PaymentStatusDM.Flagged;
                        order.UpdatedAt = DateTime.UtcNow;

                        var invoice = await _apiDbContext.Invoice
                            .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                        if (invoice != null)
                            invoice.PaymentStatus = PaymentStatusDM.Flagged;

                        await _apiDbContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        throw new SiffrumException(
                            ApiErrorTypeSM.Fatal_Log,
                            $"Webhook processing failed. Payload: {json}, Error: {e.Message}, InnerException: {ex.InnerException?.Message}, StackTrace: {ex.StackTrace}",
                            "Something went wrong while updating your payment status"
                        );
                    }
                }

                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Payment captured but order processing failed. RazorpayOrderId: {razorpayOrderId}, Error: {ex.Message}",
                    "Payment received but order processing failed. Support team has been notified."
                );
            }
        }

        private async Task HandlePaymentFailed(JObject json)
        {
            var entity = json["payload"]?["payment"]?["entity"];
            if (entity == null)
                return;

            var razorpayOrderId = entity["order_id"]?.ToString();
            var description = entity["description"]?.ToString();
            var failureReason = entity["error_description"]?.ToString();
            var razorpayPaymentId = entity["id"]?.ToString();

            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                OrderDM order = null;

                // 🔹 1. Try RazorpayOrderId
                if (!string.IsNullOrWhiteSpace(razorpayOrderId))
                {
                    order = await _apiDbContext.Order
                        .FirstOrDefaultAsync(o => o.RazorpayOrderId == razorpayOrderId);
                }

                // 🔹 2. Fallback using PaymentLinkId
                if (order == null && !string.IsNullOrWhiteSpace(description))
                {
                    var paymentLinkId = "plink_" + description.TrimStart('#');

                    order = await _apiDbContext.Order
                        .FirstOrDefaultAsync(o => o.RazorpayPaymentId == paymentLinkId);
                }

                if (order == null)
                    return;

                // 🔁 Do not override Paid
                if (order.PaymentStatus == PaymentStatusDM.Paid)
                    return;

                order.PaymentStatus = PaymentStatusDM.Failed;
                order.RazorpayPaymentId = razorpayPaymentId;
                order.FailureReason = failureReason;
                var invoice = await _apiDbContext.Invoice
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (invoice != null)
                    invoice.PaymentStatus = PaymentStatusDM.Failed;

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Failed to update failed payment for RazorPayOrderId: {razorpayOrderId}, Message: {ex.Message}",
                    "Failed to update payment failure details. Please contact support."
                );
            }
        }

        private async Task HandleRefundProcessed(JObject json)
        {
            var entity = json["payload"]?["refund"]?["entity"];
            if (entity == null)
                return;

            var razorpayPaymentId = entity["payment_id"]?.ToString();

            if (!decimal.TryParse(entity["amount"]?.ToString(), out var refundAmountInPaise))
                return;

            var refundAmount = refundAmountInPaise / 100m;

            await using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _apiDbContext.Order
                    .FirstOrDefaultAsync(o => o.RazorpayPaymentId == razorpayPaymentId);

                if (order == null)
                    return;

                // 🔁 Prevent duplicate refund processing
                if (order.PaymentStatus == PaymentStatusDM.Refunded)
                    return;

                order.RefundAmount += refundAmount;

                if (order.RefundAmount >= order.PaidAmount)
                {
                    order.PaymentStatus = PaymentStatusDM.Refunded;
                    order.OrderStatus = OrderStatusDM.Cancelled;
                }
                else
                {
                    order.PaymentStatus = PaymentStatusDM.PartiallyRefunded;
                }

                var invoice = await _apiDbContext.Invoice
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (invoice != null)
                {
                    invoice.PaymentStatus = order.PaymentStatus;
                }

                await _apiDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new SiffrumException(
                    ApiErrorTypeSM.Fatal_Log,
                    $"Failed to update refund for RazorPayPaymentId: {razorpayPaymentId}, Message: {ex.Message}",
                    "Failed to update refund details. Please contact support."
                );
            }
        }

        #endregion

        #region Signature Validation

        private void ValidateWebhookSignature(string payload, string razorpaySignature)
        {
            var secret = _apiConfiguration.RazorpaySettings.WebhookSecret;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (!SecureEquals(generatedSignature, razorpaySignature))
                throw new Exception("Invalid Razorpay webhook signature.");
        }

        private bool SecureEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;

            var result = 0;
            for (int i = 0; i < a.Length; i++)
                result |= a[i] ^ b[i];

            return result == 0;
        }

        #endregion

        #region Send Notiication on Payment Done

        private async Task SendOrderNotificationsAsync(long orderId)
        {
            try
            {
                var order = await _apiDbContext.Order
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return;

                // Notify seller only; delivery boys get notified when seller accepts
                if (order.SellerId.HasValue && order.SellerId.Value > 0)
                {
                    var sellerNotification = new SendNotificationMessageSM()
                    {
                        UserIds = new List<long> { order.SellerId.Value },
                        Title = "New Order Received",
                        Message = $"You have received a new order (ID: {order.Id}). Please accept or reject it."
                    };
                    await _notificationProcess.SendBulkPushNotificationToSellerForOrder(sellerNotification);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification Error: {ex.Message}");
            }
        }

        #endregion Send Notiication on Payment Done
    }
}