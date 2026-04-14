namespace Siffrum.Ecom.ServiceModels.v1
{
    public class InvoiceExtendedSM
    {
        public OrderSM OrderDetails { get; set; }
        public InvoiceSM InvoiceDetails { get; set; }

        public UserSM UserDetails { get; set; }

        public UserAddressSM AddressDetails { get; set; }
    }
}
