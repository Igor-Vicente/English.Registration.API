namespace Languages.Registration.API.ViewModels
{
    public class RegisterAppUserViewModel
    {
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public string AboutMe { get; set; }
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
    }
}
