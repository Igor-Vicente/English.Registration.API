using System.ComponentModel.DataAnnotations;

namespace Languages.Registration.API.ViewModels
{
    public class RefreshTokenViewModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
