using System.ComponentModel.DataAnnotations;

namespace Siffrum.Ecom.ServiceModels.AppUser
{
    public class EmailSM
    {
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
