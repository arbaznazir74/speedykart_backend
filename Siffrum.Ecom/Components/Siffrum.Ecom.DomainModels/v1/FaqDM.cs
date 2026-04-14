using Siffrum.Ecom.DomainModels.Enums;
using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("faqs")]
    public class FaqDM : SiffrumDomainModelBase<long>
    {
        [Required]
        [Column("module")]
        public FaqModuleDM Module { get; set; }

        [Required]
        [Column("question")]
        public string Question { get; set; }

        [Required]
        [Column("answer")]
        public string Answer { get; set; }

    }
}
