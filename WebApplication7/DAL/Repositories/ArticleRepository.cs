using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Article;
using Microsoft.EntityFrameworkCore;

namespace WebApplication7.DAL.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        readonly BlogContext _blogContext;
        public ArticleRepository(BlogContext blogContext) => _blogContext = blogContext;
        public async Task AddArticle(Article article)
        {
            var entry = _blogContext.Articles.Entry(article);
            if(entry.State == EntityState.Detached)
            {
                await _blogContext.AddAsync(article);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task DeleteArticle(Article article)
        {
            var entry = _blogContext.Articles.Entry(article);
            if(entry.State == EntityState.Detached)
            {
                _blogContext.Articles.Remove(article);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task<Article[]> GetAll() => await _blogContext.Articles.ToArrayAsync();

        public async Task<Article> GetArticleById(int id) => await _blogContext.Articles.FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Article[]> GetArticleByName(string name) => await _blogContext.Articles.Where(a => a.Title == name).ToArrayAsync();

        public async Task UpdateArticle(Article article, UpdateArticleQuery updateArticleQuery)
        {
            article = ArticleConverter.Convert(article, updateArticleQuery);
            var entry = _blogContext.Articles.Entry(article);
            if(entry.State == EntityState.Detached)
            {
                _blogContext.Articles.Update(article);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task AddTag(Article article, Tag tag)
        {
            article.Tags.Add(tag);
            var entry = _blogContext.Articles.Entry(article);
            if(entry.State == EntityState.Detached)
            {
                _blogContext.Articles.Update(article);
                await _blogContext.SaveChangesAsync();
            }
        }
    }
}
