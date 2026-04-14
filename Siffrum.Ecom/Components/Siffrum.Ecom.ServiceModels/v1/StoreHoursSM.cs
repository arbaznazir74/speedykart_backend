using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class StoreHoursSM : SiffrumServiceModelBase<long>
    {
        public long SellerId { get; set; }

        public short DayOfWeek { get; set; } // 0=Sunday, 1=Monday ... 6=Saturday

        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public bool IsClosed { get; set; }
    }
}
