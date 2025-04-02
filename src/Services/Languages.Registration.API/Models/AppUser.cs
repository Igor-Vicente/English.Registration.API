using FluentValidation;
using Languages.Registration.API.Enums;
using MongoDB.Bson;

namespace Languages.Registration.API.Models
{
    public class AppUser : Entity
    {
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public Idiom Idiom { get; set; }
        public string AboutMe { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime LastAccess { get; set; }

        public AppUser(string name, DateOnly birthDate, Idiom idiom, string aboutMe, string imageUrl, string city, ObjectId? id = null)
            : base(id)
        {
            Name = name;
            BirthDate = birthDate;
            Idiom = idiom;
            AboutMe = aboutMe;
            ImageUrl = imageUrl;
            City = city;
            CreatedAt = DateTime.Now;
            LastAccess = DateTime.Now;
        }

        public override bool IsValid()
        {
            ValidationResult = new UserValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class UserValidator : AbstractValidator<AppUser>
    {
        public UserValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Campo Id é obrigatório")
                .NotEqual(ObjectId.Empty)
                .WithMessage("Id inválido");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Campo nome é obrigatório")
                .MaximumLength(100)
                .WithMessage("Nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.AboutMe)
                .NotEmpty()
                .WithMessage("Campo descrição sobre mim é obrigatório")
                .MaximumLength(1000)
                .WithMessage("Minha descrição deve ter no máximo 1000 caracteres");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("Campo cidade é obrigatório")
                .MaximumLength(100)
                .WithMessage("Cidade deve ter no máximo 100 caracteres");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("Imagem é obrigatório");

            RuleFor(x => x.BirthDate)
             .Must(date => date < DateOnly.FromDateTime(DateTime.Now))
             .WithMessage("Data inválida");

            RuleFor(x => x.DeletedAt)
                .GreaterThan(x => x.CreatedAt)
                .WithMessage("Data de exclusão deve ser posterior à data de criação");

            RuleFor(x => x.Idiom)
                .IsInEnum()
                .WithMessage("Idioma inválido");
        }
    }
}
