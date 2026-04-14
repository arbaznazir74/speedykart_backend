using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class DeliveryInstructionsSM : SiffrumServiceModelBase<long>
    {
        public long UserId { get; set; }

        public string? AudioBase64 { get; set; }

        public bool AvoidCalling { get; set; }

        public bool DontRingBell { get; set; }

        public bool LeaveWithGuard { get; set; }

        public bool LeaveAtDoor { get; set; }

        public bool BewareOfDogs { get; set; }

        public string? AdditionalNotes { get; set; }
    }
}
