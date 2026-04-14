namespace CoreVisionServiceModels.Foundation.Base
{
    public abstract class ServiceModelRoot<T> : BaseServiceModelRoot
    {
        public T Id { get; set; }

        public string CreatedBy { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
