namespace Siffrum.Ecom.ServiceModels.Foundation.Base
{
    public class BaseServiceModelRoot
    {
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        protected BaseServiceModelRoot()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
