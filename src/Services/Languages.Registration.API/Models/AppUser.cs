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
                .WithMessage("The 'Id' field is required.")
                .NotEqual(ObjectId.Empty)
                .WithMessage("Invalid Id.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The 'Name' field is required.")
                .MaximumLength(100)
                .WithMessage("The name must be at most 100 characters long.");

            RuleFor(x => x.AboutMe)
                .NotEmpty()
                .WithMessage("The 'About Me' field is required.")
                .MaximumLength(1000)
                .WithMessage("The 'About Me' section must be at most 1000 characters long.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("The 'City' field is required.")
                .MaximumLength(100)
                .WithMessage("The city name must be at most 100 characters long.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .WithMessage("An image is required.");

            RuleFor(x => x.BirthDate)
                .NotEqual(new DateOnly())
                .WithMessage("Please provide a valid birthdate.")
                .Must(date => date < DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("The birth date cannot be in the future.");

            RuleFor(x => x.DeletedAt)
                .GreaterThan(x => x.CreatedAt)
                .WithMessage("The deletion date must be later than the creation date.");

            RuleFor(x => x.Idiom)
                .IsInEnum()
                .WithMessage("Invalid language selection.");
        }
    }
}
