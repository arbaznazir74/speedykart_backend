using Siffrum.Ecom.DomainModels.Foundation.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Siffrum.Ecom.DomainModels.v1
{
    [Table("delivery-instructions")]
    public class DeliveryInstructionsDM : SiffrumDomainModelBase<long>
    {

        [Column("audio-path")]
        public string? AudioPath { get; set; }

        [Column("avoid_calling")]
        public bool AvoidCalling { get; set; }

        [Column("dont_ring_bell")]
        public bool DontRingBell { get; set; }

        [Column("leave_with_guard")]
        public bool LeaveWithGuard { get; set; }

        [Column("leave_at_door")]
        public bool LeaveAtDoor { get; set; }

        [Column("beware_of_dogs")]
        public bool BewareOfDogs { get; set; }

        [Column("additional_notes")]
        public string? AdditionalNotes { get; set; }

        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public long UserId { get; set; }
        public virtual UserDM User { get; set; }

    }
}
