using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("category_specifications")]
    public class CategorySpecificationDM
    {
        [Column("id")]
        public long Id { get; set; }
        [ForeignKey(nameof(Category))]
        [Required]
        [Column("category_id")]
        public long CategoryId { get; set; }
        public virtual CategoryDM Category { get; set; }

        [ForeignKey(nameof(Specification))]
        [Required]
        [Column("specificationId")]
        public long SpecificationId { get; set; }
        public virtual ProductSpecificationFilterDM Specification { get; set; }
    }
}
