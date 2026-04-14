using System.Collections.Generic;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class HotBoxCartItemSM
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long ProductVariantId { get; set; }

        public UserHotBoxProductSM? HotBoxProductDetails { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public List<SelectedToppingItem>? SelectedToppings { get; set; }
        public List<SelectedAddonItem>? SelectedAddons { get; set; }
        public decimal ToppingsTotal { get; set; }
        public decimal AddonsTotal { get; set; }
    }
}
