using WebApplication7.DAL.Enteties;
namespace WebApplication7.DAL.Queries.Article
{
    public static class ArticleConverter
    {
        public static Enteties.Article Convert(Enteties.Article article, UpdateArticleQuery uaq)
        {
            article.Title = (!string.IsNullOrEmpty(uaq.NewTitle)) ? uaq.NewTitle : article.Title;
            article.Content = (!string.IsNullOrEmpty(uaq.NewContent)) ? uaq.NewContent : article.Content;
            return article;
        }
    }
}
