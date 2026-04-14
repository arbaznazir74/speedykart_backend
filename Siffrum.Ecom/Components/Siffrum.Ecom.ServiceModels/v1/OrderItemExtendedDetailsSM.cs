using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OrderItemExtendedDetailsSM
    {
        public OrderItemSM OrderItem { get; set; }
        public PaymentStatusSM PaymentStatus { get; set; }
        public OrderStatusSM OrderStatus { get; set; }
    }
}
