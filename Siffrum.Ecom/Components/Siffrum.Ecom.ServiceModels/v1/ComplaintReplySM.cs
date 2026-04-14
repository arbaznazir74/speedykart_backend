using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class ComplaintReplySM
    {
        public string Reply { get; set; }
        public ComplaintStatusSM Status { get; set; }
    }
}
