using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_specification_filter")]
    public class ProductSpecificationFilterDM
    {

        [Column("id")]
        public long Id { get; set; }
        [Column("name")]
        public string Name { get; set; }

        public ICollection<ProductSpecificationValueDM> SpecificationValues { get; set; }

        public ICollection<ProductSpecificationFilterValueDM> ProductSpecificationFilter { get; set; }
        public ICollection<CategorySpecificationDM> CategorySpecifications { get; set; }


    }
}
