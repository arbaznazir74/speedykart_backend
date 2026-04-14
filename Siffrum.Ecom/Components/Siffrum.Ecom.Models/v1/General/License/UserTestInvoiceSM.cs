using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.v1.General.License
{
    public class UserTestInvoiceSM : CoreVisionServiceModelBase<int>
    {
        public double DiscountInPercentage { get; set; }
        public decimal ActualPaidPrice { get; set; }
        public long RemainingAmount { get; set; }
        public int UserTestLicenseDetailsId { get; set; }
    }
}
