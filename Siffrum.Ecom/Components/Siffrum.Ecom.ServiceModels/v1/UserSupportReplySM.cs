using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserSupportReplySM : SiffrumServiceModelBase<long>
    {
        public long SupportRequestId { get; set; }
        public string Message { get; set; }
        public string SenderRole { get; set; }
        public long SenderId { get; set; }
    }
}
