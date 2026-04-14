using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class FaqSM : SiffrumServiceModelBase<long>
    {
        public FaqModuleSM Module { get; set; }

        public string Question { get; set; }

        public string Answer { get; set; }
    }
}
