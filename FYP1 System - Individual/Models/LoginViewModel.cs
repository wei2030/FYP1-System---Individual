using System.ComponentModel.DataAnnotations;

namespace FYP1_System___Individual.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
