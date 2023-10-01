using System.ComponentModel.DataAnnotations;

namespace WebApplication7.ViewModels.Article
{
    public class ArticleViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}