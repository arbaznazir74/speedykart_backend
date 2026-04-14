namespace CoreVisionServiceModels.Foundation.Base
{
    public class BaseServiceModelRoot
    {
        public DateTime CreatedOnUTC { get; set; }

        public DateTime? LastModifiedOnUTC { get; set; }

        protected BaseServiceModelRoot()
        {
            CreatedOnUTC = DateTime.UtcNow;
        }
    }
}
