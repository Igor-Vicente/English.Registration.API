using System.ComponentModel.DataAnnotations;

namespace Languages.Registration.API.ViewModels
{
    public struct SignUpViewModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 8)]
        public string Password { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [Compare("Password", ErrorMessage = "As senhas não correspondem")]
        public string ConfirmPassword { get; set; }
    }
}
