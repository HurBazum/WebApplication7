using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Comment;

namespace WebApplication7.DAL.Repositories
{
    public interface ICommentRepository
    {
        public Task AddComment(Comment comment);
        public Task DeleteComment(Comment comment);
        public Task<Comment> GetCommentById(int id);
        public Task<Comment[]> GetCommentByAuthor(Author author);
        public Task<Comment[]> GetCommentByArticle(Article article);
        public Task<Comment[]> GetAll();
        public Task UpdateComment(Comment comment, UpdateCommentQuery updateCommentQuery);
    }
}
