

using CoreVisionServiceModels.Enums;
using CoreVisionServiceModels.Foundation.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CoreVisionServiceModels.v1.General.License
{
    public class UserTestLicenseDetailsSM : CoreVisionServiceModelBase<int>
    {
        public string? SubscriptionPlanName { get; set; }
        public int TestCountValidity { get; set; }
        public double DiscountInPercentage { get; set; }
        public decimal ActualPaidPrice { get; set; }
        public long RemainingAmount { get; set; }

        [DefaultValue(false)]
        public bool IsSuspended { get; set; }
        [DefaultValue(false)]
        public bool IsCancelled { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CancelAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? StartDateUTC { get; set; }
        public int? TestLicenseTypeId { get; set; }
        public int ClientUserId { get; set; }
        public PaymentMethodSM PaymentMethod { get; set; }
        public LicenseStatusSM LicenseStatus { get; set; }
    }
}
