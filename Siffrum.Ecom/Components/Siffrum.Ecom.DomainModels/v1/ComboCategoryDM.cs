using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("combo_categories")]
    public class ComboCategoryDM
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [ForeignKey(nameof(Category))]
        [Required]
        [Column("category_id")]
        public long CategoryId { get; set; }
        public virtual CategoryDM Category { get; set; }

        [ForeignKey(nameof(ComboProduct))]
        [Required]
        [Column("combo_product_id")]
        public long ComboProductId { get; set; }
        public virtual ComboProductDM ComboProduct { get; set; }
    }
}