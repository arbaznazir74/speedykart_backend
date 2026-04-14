using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class AddonProductResponseSM : SiffrumServiceModelBase<long>
    {
        public long MainProductId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string? NetworkImage { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int Stock { get; set; }

        public int AllowedQuantity { get; set; }

        public bool IsCodAllowed { get; set; }

        public long? CategoryId { get; set; }
        public ProductIndicatorSM ProductIndicator { get; set; }
        public List<AddonCategorySM> Categories { get; set; } = new();
    }
}
