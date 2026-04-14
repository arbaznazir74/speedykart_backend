using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.Foundation.Base
{
    public class DomainModelRootBase
    {
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        protected DomainModelRootBase()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
