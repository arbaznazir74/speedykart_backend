using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserSupportRequestSM : SiffrumServiceModelBase<long>
    {
        public long UserId { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }


        public string? AdminResponse { get; set; }

        public bool IsResolved { get; set; }
    }
}