

using CoreVisionServiceModels.Enums;
using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.General.License
{
    public class UserLicenseDetailsSM : CoreVisionServiceModelBase<int>
    {
        public string StripeCustomerId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string StripeProductId { get; set; }
        public string StripePriceId { get; set; }
        public string ProductName { get; set; }
        public string SubscriptionPlanName { get; set; }
        public int ValidityInDays { get; set; }
        public double DiscountInPercentage { get; set; }
        public decimal ActualPaidPrice { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public bool IsSuspended { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelAt { get; set; }
        public DateTime? CancelledOn { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime ExpiryDateUTC { get; set; }
        public int ClientUserId { get; set; }
        public int? LicenseTypeId { get; set; }
        public PaymentMethodSM PaymentMethod { get; set; }
        public List<UserInvoiceSM>? UserInvoices { get; set; }
    }
}
