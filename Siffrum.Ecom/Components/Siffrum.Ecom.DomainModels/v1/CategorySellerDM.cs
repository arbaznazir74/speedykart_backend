using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("category_sellers")]
    public class CategorySellerDM : SiffrumDomainModelBase<long>
    {
        [Column("category_id")]
        public long CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public CategoryDM Category { get; set; }

        [Column("seller_id")]
        public long SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public SellerDM Seller { get; set; }
    }
}
