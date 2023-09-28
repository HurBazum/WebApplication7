using WebApplication7.DAL.Enteties;
using Microsoft.EntityFrameworkCore;
using WebApplication7.DAL.Queries.Tag;

namespace WebApplication7.DAL.Repositories
{
    public class TagRepository : ITagRepository
    {
        readonly BlogContext _blogContext;
        public TagRepository(BlogContext blogContext) => _blogContext = blogContext;
        public async Task AddTag(Tag tag)
        {
            var entry = _blogContext.Tags.Entry(tag);
            if(entry.State == EntityState.Detached)
            {
                await _blogContext.Tags.AddAsync(tag);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task DeleteTag(Tag tag)
        {
            var entry = _blogContext.Tags.Entry(tag);
            if(entry.State == EntityState.Detached)
            {
                _blogContext.Tags.Remove(tag);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task<Tag[]> GetAll() => await _blogContext.Tags.ToArrayAsync();

        public async Task<Tag> GetTagById(int id) => await _blogContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        public async Task<Tag> GetTagByContent(string content) => await _blogContext.Tags.FirstOrDefaultAsync(t => t.Content == content);

        public async Task UpdateTag(Tag tag, UpdateTagQuery updateTagQuery)
        {
            tag = TagConverter.Convert(tag, updateTagQuery);
            var entry = _blogContext.Tags.Entry(tag);
            if(entry.State == EntityState.Detached)
            {
                _blogContext.Tags.Update(tag);
                await _blogContext.SaveChangesAsync();
            }
        }
    }
}
