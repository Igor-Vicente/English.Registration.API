using Languages.Registration.API.Enums;

namespace Languages.Registration.API.ViewModels
{
    public struct ResponseUserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public Idiom Idiom { get; set; }
        public string AboutMe { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime LastAccess { get; set; }
    }
}
