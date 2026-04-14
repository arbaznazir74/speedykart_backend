using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ProductSpecificationValueSM
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public long SpecificationFilterId { get; set; }
    }
}