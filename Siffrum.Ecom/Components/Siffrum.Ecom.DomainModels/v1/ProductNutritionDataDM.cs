using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_nutrition_data")]
    public class ProductNutritionDataDM : SiffrumDomainModelBase<long>
    {

        #region Product Mapping

        [ForeignKey(nameof(ProductVariant))]
        [Required]
        [Column("product_variant_id")]
        public long ProductVariantId { get; set; }

        public virtual ProductVariantDM? ProductVariant { get; set; }

        #endregion

        #region Nutrition Values

        [Column("serve_size")]
        public string? ServeSize { get; set; }

        [Column("calories")]
        public decimal? Calories { get; set; }

        [Column("proteins")]
        public decimal? Proteins { get; set; }

        [Column("carbohydrates")]
        public decimal? Carbohydrates { get; set; }

        [Column("fats")]
        public decimal? Fats { get; set; }

        [Column("sugars")]
        public decimal? Sugars { get; set; }

        [Column("fiber")]
        public decimal? Fiber { get; set; }

        [Column("sodium")]
        public decimal? Sodium { get; set; }

        #endregion

        #region Ingredients JSON

        // Stored as JSON in DB
        [Column("ingredients_json")]
        public string? IngredientsJson { get; set; }

        #endregion
    }
}
