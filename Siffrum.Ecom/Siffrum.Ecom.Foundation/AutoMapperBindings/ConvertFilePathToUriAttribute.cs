using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;

namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ConvertFilePathToUriAttribute : AutoInjectRootAttribute
    {
        public string? SourcePropertyName { get; set; }
    }
}
