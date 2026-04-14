namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryBoyOrderDetailsSM
    {
        public string UserMobile { get; set; }
        public OrderSM OrderDetails { get; set; }
        public List<OrderItemSM> OrderItems { get; set; }

        public DeliverySM DeliveryDetails { get; set; }
    }
}
