using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Article;

namespace WebApplication7.DAL.Repositories
{
    public interface IArticleRepository
    {
        public Task AddArticle(Article article);
        public Task UpdateArticle(Article article, UpdateArticleQuery updateArticleQuery);
        public Task DeleteArticle(Article article);
        public Task<Article> GetArticleById(int id);
        public Task<Article[]> GetArticleByName(string name);
        public Task<Article[]> GetAll();
        public Task AddTag(Article article, Tag tag);
    }
}