namespace WebApplication7.DAL.Queries.Comment
{
    public static class CommentsContentConverter
    {
        public static string Convert(Enteties.Comment c, UpdateCommentQuery ucq) => c.Content = ucq.NewContent;
    }
}