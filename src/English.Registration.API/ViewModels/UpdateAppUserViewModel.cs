namespace English.Registration.API.ViewModels
{
    public class UpdateAppUserViewModel
    {
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public string AboutMe { get; set; }
        public string City { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
    }
}
