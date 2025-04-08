using Languages.Registration.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace Languages.Registration.API.ViewModels
{
    public class ResponseUserViewModel
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
        public Coordinates? Coordinates { get; set; }
    }

    public class Coordinates
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }
        public Coordinates() { }
        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
