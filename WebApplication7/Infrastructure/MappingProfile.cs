using AutoMapper;
using WebApplication7.BLL.Models.Author;
using WebApplication7.BLL.Models.Comment;
using WebApplication7.BLL.Models.Article;
using WebApplication7.DAL.Queries.Article;
using WebApplication7.DAL.Queries.Author;
using WebApplication7.DAL.Queries.Comment;
using WebApplication7.DAL.Enteties;
using WebApplication7.ViewModels.Account;
using WebApplication7.ViewModels.Article;
using WebApplication7.ViewModels.Tag;

namespace WebApplication7.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<UpdateAuthorRequest, UpdateAuthorQuery>();
            CreateMap<UpdateCommentRequest, UpdateCommentQuery>();
            CreateMap<UpdateArticleRequest, UpdateArticleQuery>();
            CreateMap<RegisterViewModel, Author>();
            CreateMap<ArticleViewModel, Article>();
            CreateMap<Article, ArticleViewModel>();
            CreateMap<TagViewModel, Tag>();
            CreateMap<Tag, TagViewModel>();
        }
    }
}
