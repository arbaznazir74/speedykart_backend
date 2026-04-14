using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("categories")]
    public class CategoryDM : SiffrumDomainModelBase<long>
    {

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } = null!;

        [Column("slug")]
        [MaxLength(191)]
        public string? Slug { get; set; }

        [Column("subtitle")]
        public string? Subtitle { get; set; }
        [Required]
        [Column("image")]
        public string Image { get; set; }

        [Column("web_image")]
        public string? WebImage { get; set; }

        [Column("status")]
        public StatusDM Status { get; set; }
        [Column("is_system")]
        public bool IsSystem { get; set; }
        [Column("sort_order")]
        public int SortOrder { get; set; }
        [Column("level")]
        public int Level { get; set; } = 1;

        [Column("platform")]
        public PlatformTypeDM? Platform { get; set; }
        [Column("timings")]
        public CategoryTimingDM? Timings { get; set; } = CategoryTimingDM.None;

        [ForeignKey(nameof(ParentCategory))]
        [Column("parent_category_id")]
        public long? ParentCategoryId { get; set; }
        public CategoryDM? ParentCategory { get; set; }

        [Column("meta_title")]
        [MaxLength(191)]
        public string? MetaTitle { get; set; }

        [Column("meta_keywords")]
        public string? MetaKeywords { get; set; }

        [Column("schema_markup")]
        public string? SchemaMarkup { get; set; }

        [Column("meta_description")]
        public string? MetaDescription { get; set; }
        
        [Column("suggested_by_seller_id")]
        public long? SuggestedBySellerId { get; set; }

        public ICollection<ComboCategoryDM> ComboCategories { get; set; }
        public ICollection<ProductDM> Products { get; set; }
        public ICollection<CategorySpecificationDM> CategorySpecifications { get; set; }
    }
}
