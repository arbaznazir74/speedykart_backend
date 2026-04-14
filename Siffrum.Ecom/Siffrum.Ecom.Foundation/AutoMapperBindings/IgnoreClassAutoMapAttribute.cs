using Siffrum.Ecom.ServiceModels.Foundation.Base.CommonResponseRoot;

namespace Siffrum.Ecom.Foundation.AutoMapperBindings
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IgnoreClassAutoMapAttribute : AutoInjectRootAttribute
    {
    }
}
