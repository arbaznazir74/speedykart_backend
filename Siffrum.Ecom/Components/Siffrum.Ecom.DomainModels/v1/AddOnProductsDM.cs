using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("addon_products")]
    public class AddOnProductsDM : SiffrumDomainModelBase<long>
    {

        [ForeignKey(nameof(MainProduct))]

        [Column("main_product_id")]
        public long MainProductId { get; set; }
        public virtual ProductVariantDM MainProduct { get; set; }
        [ForeignKey(nameof(AddOnProduct))]
        [Column("addon_product_id")]
        public long AddonProductId { get; set; }
        public virtual ProductVariantDM AddOnProduct { get; set; }

        [ForeignKey(nameof(Category))]
        [Column("category_id")]
        public long CategoryId { get; set; }
        public virtual CategoryDM Category { get; set; }

    }
}
