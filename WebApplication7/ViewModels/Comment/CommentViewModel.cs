namespace WebApplication7.ViewModels.Comment
{
    public class CommentViewModel : CreateCommentViewModel
    {
        public DateTime CreatedDate { get; set; }
        public string Article { get; set; }
        public string Author { get; set; }
    }
}