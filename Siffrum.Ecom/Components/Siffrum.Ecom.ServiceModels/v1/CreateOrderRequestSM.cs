namespace Siffrum.Ecom.ServiceModels.v1
{
    public class CreateOrderRequestSM
    {
        public OrderSM Order { get; set; }
        public List<OrderItemSM> OrderItems { get; set; } 
    }
}
