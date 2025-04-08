using Languages.Registration.API.Enums;
using Languages.Registration.API.Models;
using Languages.Registration.API.ViewModels;
using MongoDB.Bson;

namespace Languages.Registration.API.Configuration
{
    public static class ViewModelExtentions
    {
        public static AppUser ToNewAppUser(this RegisterAppUserViewModel model, ObjectId oid)
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
                Coordinates = appUser.Location == null ?
                    null : new Coordinates(appUser.Location.Coordinates[1], appUser.Location.Coordinates[0]),
            };
        }
        public static IEnumerable<ResponseUserViewModel> ToResponseAppUser(this IEnumerable<AppUser> appUsers)
        {
            var model = new List<ResponseUserViewModel>();

            foreach (var appUser in appUsers)
            {
                var user = appUser.ToResponseAppUser();
                if (user != null)
                    model.Add(user);
            }

            return model;
        }
        public static Module ToNewModule(this AddModuleViewModel model)
        {
            return new Module(model.Title, model.Priority, model.LessonsViewModel.ToNewLesson());
        }
        public static Lesson ToNewLesson(this AddLessonViewModel model)
        {
            return new Lesson(model.Title, model.Priority, model.VideoUrl, model.ThumbUrl, model.Content);
        }
        public static IEnumerable<Lesson> ToNewLesson(this AddLessonViewModel[] model)
        {
            var lessons = new List<Lesson>();

            foreach (var lesson in model)
            {
                lessons.Add(lesson.ToNewLesson());
            }

            return lessons;
        }
        public static ResponseModuleViewModel ToResponseModule(this Module module)
        {
            var response = new ResponseModuleViewModel()
            {
                Id = module.Id.ToString(),
                CreatedAt = module.CreatedAt,
                Title = module.Title,
                LessonsViewModel = module.Lessons.ToResponseLesson(),
                Priority = module.Priority,
            };

            return response;
        }
        public static IEnumerable<ResponseModuleViewModel> ToResponseModule(this IEnumerable<Module> modules)
        {
            var model = new List<ResponseModuleViewModel>();

            foreach (var module in modules)
            {
                model.Add(module.ToResponseModule());
            }

            return model;
        }
        public static ResponseLessonViewModel ToResponseLesson(this Lesson lesson)
        {
            return new ResponseLessonViewModel()
            {
                Id = lesson.Id.ToString(),
                Content = lesson.Content,
                Priority = lesson.Priority,
                ThumbUrl = lesson.ThumbUrl,
                Title = lesson.Title,
                VideoUrl = lesson.VideoUrl
            };
        }
        public static IEnumerable<ResponseLessonViewModel> ToResponseLesson(this IEnumerable<Lesson> lessons)
        {
            var model = new List<ResponseLessonViewModel>();

            foreach (var lesson in lessons)
            {
                model.Add(lesson.ToResponseLesson());
            }

            return model;
        }
    }
}
