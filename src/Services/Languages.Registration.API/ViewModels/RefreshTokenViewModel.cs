using System.ComponentModel.DataAnnotations;

namespace Languages.Registration.API.ViewModels
{
    public struct RefreshTokenViewModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
