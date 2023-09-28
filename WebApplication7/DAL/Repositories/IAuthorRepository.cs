using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Author;

namespace WebApplication7.DAL.Repositories
{
    public interface IAuthorRepository
    {
        public Task AddAuthor(Author author);
        public Task DeleteAuthor(Author author);
        public Task<Author> GetAuthorById(int id);
        public Task<Author> GetAuthorByEmail(string email);
        public Task<Author[]> GetAll();
        public Task UpdateAuthor(Author author, UpdateAuthorQuery updateAuthorQuery);
    }
}