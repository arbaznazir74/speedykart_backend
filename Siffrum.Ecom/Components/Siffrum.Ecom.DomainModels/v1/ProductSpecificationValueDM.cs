using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("product_specification_value")]
    public class ProductSpecificationValueDM
    {
        [Column("id")]
        public long Id { get; set; }
        [Column("value")]
        public string Value { get; set; }

        [ForeignKey(nameof(ProductSpecificationFilterDM))]
        [Column("specification_filter_id")]

        public long SpecificationFilterId { get; set; }

        public ProductSpecificationFilterDM ProductSpecificationFilter { get; set; }
        public ICollection<ProductSpecificationFilterValueDM> ProductSpecificationFilterValue { get; set; }
    }
}