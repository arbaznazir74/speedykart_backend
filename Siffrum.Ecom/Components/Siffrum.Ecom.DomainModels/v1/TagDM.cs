using Microsoft.EntityFrameworkCore;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("tags")]
    [Index(nameof(Name), IsUnique = true)]
    public class TagDM : SiffrumDomainModelBase<long>
    { 
        [Required]
        [Column("name")]
        [MaxLength(191)]
        public string Name { get; set; } = string.Empty;
        public ICollection<ProductTagDM> ProductTags { get; set; }
    }
}
