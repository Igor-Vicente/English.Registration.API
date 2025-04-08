namespace Languages.Registration.API.ViewModels
{
    public class ResponseModuleViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<ResponseLessonViewModel> LessonsViewModel { get; set; }
    }

    public class ResponseLessonViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbUrl { get; set; }
        public string Content { get; set; }
    }
}
