namespace Siffrum.Ecom.ServiceModels.Foundation.Base
{
    public abstract class ServiceModelRoot<T> : BaseServiceModelRoot
    {
        public T Id { get; set; }        
        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
