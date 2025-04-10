using System.ComponentModel.DataAnnotations;

namespace English.Registration.API.ViewModels
{
    public class RefreshTokenViewModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
