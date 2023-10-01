﻿namespace WebApplication7.DAL.Enteties
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; init; } = DateTime.Now;

        // rel
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
