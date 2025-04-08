using FluentValidation;
using MongoDB.Bson;

namespace Languages.Registration.API.Models
{
    public class Module : Entity
    {
        public string Title { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<Lesson> Lessons { get; set; }

        public Module(string title, int priority, IEnumerable<Lesson> lessons)
        {
            Title = title;
            Priority = priority;
            Lessons = lessons;
            CreatedAt = DateTime.Now;
        }

        public override bool IsValid()
        {
            ValidationResult = new ModuleValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }

    public class Lesson
    {
        public ObjectId Id { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbUrl { get; set; }
        public string Content { get; set; }

        public Lesson(string title, int priority, string videoUrl, string thumbUrl, string content)
        {
            Id = ObjectId.GenerateNewId();
            Title = title;
            Priority = priority;
            VideoUrl = videoUrl;
            ThumbUrl = thumbUrl;
            Content = content;
        }
    }

    public class ModuleValidator : AbstractValidator<Module>
    {
        public ModuleValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Lessons)
                .NotEmpty();

            RuleForEach(x => x.Lessons).ChildRules(lesson =>
            {
                lesson.RuleFor(l => l.Title)
                    .NotEmpty()
                    .MaximumLength(60);

                lesson.RuleFor(l => l.VideoUrl)
                    .NotEmpty()
                    .MaximumLength(200);

                lesson.RuleFor(l => l.ThumbUrl)
                    .NotEmpty()
                    .MaximumLength(200);
            });
        }
    }
}
