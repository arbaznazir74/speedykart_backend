using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using Siffrum.Ecom.DomainModels.Enums;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("brands")]
    public class BrandDM : SiffrumDomainModelBase<long>
    {

        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Column("status")]
        [MaxLength(191)]
        public StatusDM Status { get; set; }
        
        public ICollection<ProductDM> Products { get; set; }

        
    }
}
