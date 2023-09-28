using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Tag;

namespace WebApplication7.DAL.Repositories
{
    public interface ITagRepository
    {
        public Task AddTag(Tag tag);
        public Task UpdateTag(Tag tag, UpdateTagQuery updateTagQuery);
        public Task DeleteTag(Tag tag);
        public Task<Tag> GetTagById(int id);
        public Task<Tag[]> GetAll();
        public Task<Tag> GetTagByContent(string content);
    }
}
