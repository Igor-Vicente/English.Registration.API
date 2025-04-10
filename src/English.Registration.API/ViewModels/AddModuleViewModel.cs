namespace English.Registration.API.ViewModels
{
    public struct AddModuleViewModel
    {
        public string Title { get; set; }
        public int Priority { get; set; }
        public AddLessonViewModel[] LessonsViewModel { get; set; }
    }

    public class AddLessonViewModel
    {
        public string Title { get; set; }
        public int Priority { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbUrl { get; set; }
        public string Content { get; set; }
    }
}
