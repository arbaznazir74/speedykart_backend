using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("nutrition_category")]
    public class NutritionCategoryDM
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
        [ForeignKey(nameof(Category))]
        [Required]
        [Column("category_id")]
        public long CategoryId { get; set; }
        public virtual CategoryDM Category { get; set; }
    }
}
