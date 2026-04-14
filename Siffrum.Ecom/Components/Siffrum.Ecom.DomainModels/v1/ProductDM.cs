using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("products")]
    [Index(nameof(SellerId), nameof(Slug), IsUnique = true)]
    public class ProductDM : SiffrumDomainModelBase<long>
    {

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; }                 

        [Required]
        [Column("slug")]
        [MaxLength(191)]
        public string Slug { get; set; }    

        // Relationships
        [ForeignKey(nameof(Seller))]
        [Required]
        [Column("seller_id")]
        public long SellerId { get; set; }
        public virtual SellerDM? Seller { get; set; }
        [ForeignKey(nameof(Category))]
        [Required]
        [Column("category_id")]
        public long CategoryId { get; set; }
        public virtual CategoryDM? Category { get; set; }
        [ForeignKey(nameof(Brand))]
        [Column("brand_id")]
        public long? BrandId { get; set; }
        public virtual BrandDM? Brand { get; set; }
        
        [Column("tax_percentage")]
        public decimal? TaxPercentage { get; set; }

        [Column("tags")]
        [MaxLength(500)]
        public string? Tags { get; set; }
        public ICollection<ProductVariantDM> ProductVariants { get; set; } 

    }
}
