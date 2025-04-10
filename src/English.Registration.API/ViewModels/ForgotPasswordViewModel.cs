using System.ComponentModel.DataAnnotations;

namespace English.Registration.API.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
