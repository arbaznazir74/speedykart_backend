using System.ComponentModel.DataAnnotations;

namespace CoreVisionServiceModels.AppUser
{
    public class EmailConfirmationSM
    {
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
