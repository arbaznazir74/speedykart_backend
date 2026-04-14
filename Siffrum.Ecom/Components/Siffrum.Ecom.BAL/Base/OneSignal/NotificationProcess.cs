using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.ServiceModels.AppUser.Login;
using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Enums;
using Siffrum.Ecom.ServiceModels.v1;
using System.Text;
using System.Text.Json;

namespace Siffrum.Ecom.BAL.Base.OneSignal
{
    public class NotificationProcess : SiffrumBalBase
    {
        private readonly APIConfiguration _apiConfiguration;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _baseUrl;
        private readonly string _appId;
        private readonly string _apiKey;

        public NotificationProcess(
            IMapper mapper,
            ApiDbContext context,
            APIConfiguration apiConfiguration,
            IHttpClientFactory httpClientFactory)
            : base(mapper, context)
        {
            _apiConfiguration = apiConfiguration;
            _httpClientFactory = httpClientFactory;

            _baseUrl = _apiConfiguration.OneSignalSettings.BaseUrl;
            _appId = _apiConfiguration.OneSignalSettings.AppId;
            _apiKey = _apiConfiguration.OneSignalSettings.RestApiKey;
        }


        #region Send Push To Single User

        public async Task<BoolResponseRoot> SendPushNotification(SendNotificationMessageSM request)
        {
            var subscription = await _apiDbContext.User
                .Where(x => x.Id == request.UserIds[0] &&
                            !string.IsNullOrEmpty(x.FcmId) &&
                            x.Status == StatusDM.Active)
                .Select(x => x.FcmId!)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(subscription))
                return new BoolResponseRoot(false, "User device not found or token is missing");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = new[] { subscription }
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        public async Task<BoolResponseRoot> SendPushNotificationByPlayerId(string playerId, SendNotificationMessageSM request)
        {           

            if (string.IsNullOrEmpty(playerId))
                return new BoolResponseRoot(false, "User device not found or token is missing");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = new[] { playerId }
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        public async Task<BoolResponseRoot> SendPushNotificationToUser(SendNotificationMessageSM request, string fcmId)
        {
            if (string.IsNullOrEmpty(fcmId))
                return new BoolResponseRoot(false, "No valid device token");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = new[] { fcmId }
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        #endregion

        #region Send Push To Multiple Users

        public async Task<BoolResponseRoot> SendBulkPushNotification(SendNotificationMessageSM request)
        {
            var subscriptions = await _apiDbContext.User
                .Where(x => request.UserIds.Contains(x.Id) &&
                            !string.IsNullOrEmpty(x.FcmId) &&
                            x.Status == StatusDM.Active)
                .Select(x => x.FcmId!)
                .ToListAsync();

            if (!subscriptions.Any())
                return new BoolResponseRoot(false, "No valid device tokens found");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = subscriptions
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        public async Task<BoolResponseRoot> SendBulkPushNotificationToAdmins(SendNotificationMessageSM request)
        {
            var subscriptions = await _apiDbContext.Admin
                .Where(x => !string.IsNullOrEmpty(x.FcmId) &&
                            x.Status == StatusDM.Active)
                .Select(x => x.FcmId!)
                .ToListAsync();

            if (!subscriptions.Any())
                return new BoolResponseRoot(false, "No valid device tokens found");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = subscriptions
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }      

        public async Task<BoolResponseRoot> SendBulkPushNotificationToSellerForOrder(SendNotificationMessageSM request)
        {
            var subscriptions = await _apiDbContext.Seller
                .Where(x => request.UserIds.Contains(x.Id) &&
                            !string.IsNullOrEmpty(x.FcmId))
                .Select(x => x.FcmId!)
                .ToListAsync();

            if (!subscriptions.Any())
                return new BoolResponseRoot(false, "No valid device tokens found");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = subscriptions
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        public async Task<BoolResponseRoot> SendBulkPushNotificationToDeliveryBoysForOrder(SendNotificationMessageSM request, string pincode)
        {
            var ids = await _apiDbContext.DeliveryBoyPincodes
                .Where(x => pincode.Contains(x.Pincode))
                .Select(x => x.DeliveryBoyId)
                .Distinct()
                .ToListAsync();
            var subscriptions = await _apiDbContext.DeliveryBoy
                .Where(x => ids.Contains(x.Id) &&
                            !string.IsNullOrEmpty(x.FcmId))
                .Select(x => x.FcmId!)
                .ToListAsync();

            if (!subscriptions.Any())
                return new BoolResponseRoot(false, "No valid device tokens found");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = subscriptions
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        public async Task<BoolResponseRoot> SendPushNotificationToSellerDeliveryBoys(SendNotificationMessageSM request, long sellerId)
        {
            var subscriptions = await _apiDbContext.DeliveryBoy
                .Where(x => x.SellerId == sellerId &&
                            x.Status == DeliveryBoyStatusDM.Active &&
                            x.IsAvailable == 1 &&
                            !string.IsNullOrEmpty(x.FcmId))
                .Select(x => x.FcmId!)
                .ToListAsync();

            if (!subscriptions.Any())
                return new BoolResponseRoot(false, "No available delivery boys found for this seller");

            object payload;
            if (request.AdditionalData != null && request.AdditionalData.Count > 0)
            {
                payload = new
                {
                    app_id = _appId,
                    target_channel = "push",
                    headings = new { en = request.Title },
                    contents = new { en = request.Message },
                    include_subscription_ids = subscriptions,
                    data = request.AdditionalData
                };
            }
            else
            {
                payload = new
                {
                    app_id = _appId,
                    target_channel = "push",
                    headings = new { en = request.Title },
                    contents = new { en = request.Message },
                    include_subscription_ids = subscriptions
                };
            }

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        #endregion

        #region Broadcast Push Notification

        public async Task<BoolResponseRoot> BroadcastPushNotification(SendNotificationMessageSM request)
        {
            var subscriptions = await _apiDbContext.User
                .Where(x => x.Status == StatusDM.Active)
                .Select(x => x.FcmId!)
                .ToListAsync();

            if (!subscriptions.Any())
                return new BoolResponseRoot(false, "No valid device tokens found");

            var payload = new
            {
                app_id = _appId,
                target_channel = "push",
                headings = new { en = request.Title },
                contents = new { en = request.Message },
                include_subscription_ids = subscriptions
            };

            return await SendRequest($"{_baseUrl}?c=push", payload);
        }

        #endregion

        #region Send OTP SMS

        public async Task<BoolResponseRoot> SendOtpSms(string phoneNumber, int otp)
        {
            try
            {
                var smsSettings = _apiConfiguration.SmsSettings;
                var client = _httpClientFactory.CreateClient();

                var url = $"{smsSettings.BaseUrl}/SendSMS";
                var otpMessage = $"{otp} is your otp to login to SpeedyKart. SpeedyKart never calls to ask for OTP. The OTP expires in 2 mins.-Speedykart";
                var payload = new
                {
                    userid = smsSettings.UserId,
                    pwd = smsSettings.Password,
                    mobile = phoneNumber,
                    sender = smsSettings.Sender,
                    msg = otpMessage,
                    msgtype = smsSettings.MsgType,
                    peid = smsSettings.PeId,
                    templateid = smsSettings.TemplateId
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new BoolResponseRoot(false, responseBody);

                // Deserialize response
                var smsResponse = JsonSerializer.Deserialize<List<SmsResponseSM>>(responseBody);

                var message = smsResponse?.FirstOrDefault()?.Response;

                string messageId = "";

                if (!string.IsNullOrEmpty(message) && message.Contains("Message ID:"))
                {
                    messageId = message.Split("Message ID:").Last().Trim();
                }

                return new BoolResponseRoot(true, messageId);
            }
            catch (Exception ex)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    ex.Message,
                    "Failed to send OTP SMS");
            }
        }

        public async Task<BoolResponseRoot> GetSmsDeliveryStatus(string messageId)
        {
            try
            {
                var smsSettings = _apiConfiguration.SmsSettings;
                var client = _httpClientFactory.CreateClient();

                var url = $"{smsSettings.BaseUrl}/GetDelivery";

                var payload = new
                {
                    userId = smsSettings.UserId,
                    pwd = smsSettings.Password,
                    msgId = messageId
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new BoolResponseRoot(false, responseBody);

                var smsResponse = JsonSerializer.Deserialize<List<SmsResponseSM>>(responseBody);

                var message = smsResponse?.FirstOrDefault()?.Response ?? "";

                if (message.Contains("Delivery Status : Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    return new BoolResponseRoot(true, message);
                }

                return new BoolResponseRoot(false, message);
            }
            catch (Exception ex)
            {
                throw new SiffrumException(
                    ApiErrorTypeSM.InvalidInputData_NoLog,
                    ex.Message,
                    "Failed to fetch SMS delivery status");
            }
        }

        #endregion

        #region Core Request Sender

        private async Task<BoolResponseRoot> SendRequest(string url, object payload)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("Authorization", $"Key {_apiKey}");

                request.Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new BoolResponseRoot(false, $"OneSignal API Error: {error}");
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                return new BoolResponseRoot(true, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                throw new SiffrumException(ApiErrorTypeSM.InvalidInputData_NoLog, 
                    $"Message: {ex.Message}, InnerException: {ex?.InnerException}, StackTrace: {ex?.StackTrace}",
                    "Something went wrong while sending notification, Please try again later");
            }
        }

        #endregion

        #region Generate OTP

        public int GenerateOtp()
        {
            return Random.Shared.Next(100000, 999999);
        }

        #endregion
    }
}