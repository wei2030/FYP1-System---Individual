using System.ComponentModel.DataAnnotations;

namespace FYP1_System___Individual.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
