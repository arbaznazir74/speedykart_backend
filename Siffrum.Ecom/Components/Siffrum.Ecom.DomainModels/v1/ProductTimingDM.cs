using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_timings")]
    public class ProductTimingDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("seller_id")]
        public long SellerId { get; set; }

        [ForeignKey(nameof(SellerId))]
        public SellerDM Seller { get; set; }

        [Required]
        [Column("product_id")]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public ProductDM Product { get; set; }

        [Required]
        [Column("category_id")]
        public long CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public CategoryDM Category { get; set; }

        [Required]
        [Column("start_hour")]
        public int StartHour { get; set; }

        [Required]
        [Column("start_minute")]
        public int StartMinute { get; set; }

        [Required]
        [Column("end_hour")]
        public int EndHour { get; set; }

        [Required]
        [Column("end_minute")]
        public int EndMinute { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
