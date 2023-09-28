﻿using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Comment;
using Microsoft.EntityFrameworkCore;

namespace WebApplication7.DAL.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        readonly BlogContext _blogContext;

        public CommentRepository(BlogContext blogContext) => _blogContext = blogContext;

        public async Task AddComment(Comment comment)
        {
            var entry = _blogContext.Comments.Entry(comment);
            if(entry.State == EntityState.Detached)
            {
                await _blogContext.AddAsync(entry);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task DeleteAuthor(Comment comment)
        {
            var entry = _blogContext.Comments.Entry(comment);
            if(entry.State == EntityState.Detached)
            {
                _blogContext.Comments.Remove(comment);
                await _blogContext.SaveChangesAsync();
            }
        }

        public async Task<Comment[]> GetAll() => await _blogContext.Comments.ToArrayAsync();

        public async Task<Comment[]> GetCommentByArticle(Article article) => await _blogContext.Comments.Include(a => a.Article).Where(c => c.ArticleId == article.Id).ToArrayAsync();

        public async Task<Comment[]> GetCommentByAuthor(Author author) => await _blogContext.Comments.Include(a => a.Author).Where(c => c.AuthorId == author.Id).ToArrayAsync();

        public async Task<Comment> GetCommentById(int id) => await _blogContext.Comments.FirstOrDefaultAsync(c => c.Id == id);

        public Task UpdateComment(Comment comment, UpdateCommentQuery updateCommentQuery)
        {
            throw new NotImplementedException();
        }
    }
}