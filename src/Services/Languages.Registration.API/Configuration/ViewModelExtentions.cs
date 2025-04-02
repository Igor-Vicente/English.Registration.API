using Languages.Registration.API.Enums;
using Languages.Registration.API.Models;
using Languages.Registration.API.ViewModels;
using MongoDB.Bson;

namespace Languages.Registration.API.Configuration
{
    public static class ViewModelExtentions
    {
        public static AppUser ToAppUser(this RegisterAppUserViewModel model, ObjectId oid)
        {
            return new AppUser(model.Name, model.BirthDate, Idiom.English, model.AboutMe, model.ImageUrl, model.City, oid);
        }

        public static ResponseUserViewModel ToResponseAppUser(this AppUser applicationUser)
        {
            return new ResponseUserViewModel()
            {
                Id = applicationUser.Id.ToString(),
                Name = applicationUser.Name,
                AboutMe = applicationUser.AboutMe,
                BirthDate = applicationUser.BirthDate,
                Idiom = applicationUser.Idiom,
                City = applicationUser.City,
                ImageUrl = applicationUser.ImageUrl,
                CreatedAt = applicationUser.CreatedAt,
                DeletedAt = applicationUser.DeletedAt,
                LastAccess = applicationUser.LastAccess,
            };
        }
    }
}
