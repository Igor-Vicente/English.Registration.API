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

        public static ResponseUserViewModel? ToResponseAppUser(this AppUser appUser)
        {
            return appUser == null ? null : new ResponseUserViewModel()
            {
                Id = appUser.Id.ToString(),
                Name = appUser.Name,
                AboutMe = appUser.AboutMe,
                BirthDate = appUser.BirthDate,
                Idiom = appUser.Idiom,
                City = appUser.City,
                ImageUrl = appUser.ImageUrl,
                CreatedAt = appUser.CreatedAt,
                DeletedAt = appUser.DeletedAt,
                LastAccess = appUser.LastAccess,
            };
        }
    }
}
