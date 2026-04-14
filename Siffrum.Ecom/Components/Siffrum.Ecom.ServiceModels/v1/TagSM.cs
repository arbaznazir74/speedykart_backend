using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class TagSM : SiffrumServiceModelBase<long>
    {
        public string Name { get; set; } = string.Empty;
    }
}
