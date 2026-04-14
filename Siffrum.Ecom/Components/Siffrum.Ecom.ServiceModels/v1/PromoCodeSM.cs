using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class PromoCodeSM : SiffrumServiceModelBase<long>
    {
        public string Code { get; set; } // "SAVE10"

        public CouponTypeSM Type { get; set; }
        // Percentage or Flat

        public decimal DiscountValue { get; set; }
        // 10 (means 10% OR 10 currency depending on type)

        public decimal? MaxDiscountAmount { get; set; }
        // For percentage coupons (cap limit)

        public decimal? MinimumCartAmount { get; set; }

        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }

        public int? UsagePerUserLimit { get; set; }

        public bool IsActive { get; set; }

        public bool IsFirstOrderOnly { get; set; }

        public PlatformTypeSM PlatformType { get; set; }

    }
}
