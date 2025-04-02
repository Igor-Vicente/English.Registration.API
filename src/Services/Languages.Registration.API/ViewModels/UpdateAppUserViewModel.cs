namespace Languages.Registration.API.ViewModels
{
    public struct UpdateAppUserViewModel
    {
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public string AboutMe { get; set; }
        public string City { get; set; }
    }
}
