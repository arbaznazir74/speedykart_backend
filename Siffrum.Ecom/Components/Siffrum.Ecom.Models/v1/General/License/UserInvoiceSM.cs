using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.General.License
{
    public class UserInvoiceSM : CoreVisionServiceModelBase<int>
    {
        public string StripeInvoiceId { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime ExpiryDateUTC { get; set; }
        public double DiscountInPercentage { get; set; }
        public decimal ActualPaidPrice { get; set; }
        public string Currency { get; set; }
        public int UserLicenseDetailsId { get; set; }
        public string StripeCustomerId { get; set; }
        public long AmountDue { get; set; }
        public long AmountRemaining { get; set; }
    }
}
