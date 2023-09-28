using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Repositories;
using WebApplication7.DAL.Enteties;
using WebApplication7.ViewModels.Article;
using WebApplication7.ViewModels.Tag;

namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Articles")]
    public class ArticleController : Controller
    {
        readonly IArticleRepository _articleRepository;
        readonly IAuthorRepository _authorRepository;
        readonly ITagRepository _tagRepository;
        readonly IMapper _mapper;
        public ArticleController(IArticleRepository articleRepository, IAuthorRepository authorRepository, ITagRepository tagRepository, IMapper mapper)
        {
            _articleRepository = articleRepository;
            _authorRepository = authorRepository;
            _tagRepository = tagRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllArticles()
        {
            var articles = _articleRepository.GetAll();
            return Ok(articles);
        }

        [HttpPost]
        [Route("/{id}")]
        public async Task<IActionResult> WriteArticle([FromBody]ArticleViewModel articleViewModel, [FromRoute]int id)
        {
            var author = await _authorRepository.GetAuthorById(id);

            if(author == null)
            {
                return BadRequest($"Автора с id={id} не существует!");
            }
            if(articleViewModel.Title == null)
            {
                return BadRequest("Необходимо заполнить поле \'заголовок\'!");
            }
            if(articleViewModel.Content == null)
            {
                return BadRequest("Статья не может состоять из 0 символов!");
            }

            var article = _mapper.Map<Article>(articleViewModel);
            article.AuthorId = author.Id;
            article.Author = author;

            try
            {
                await _articleRepository.AddArticle(article);
                return Ok($"Статья \'{article.Title}\' успешно добавлена!");
            }
            catch
            {
                return StatusCode(401, "Какая-то ошибка!");
            }
        }

        /// <summary>
        /// в процессе. . .
        /// </summary>
        [HttpPatch]
        [Route("/{id}/Tags")]
        public async Task<IActionResult> SetArticlesTags([FromRoute]int id, [FromBody]TagViewModel tagViewModel)
        {
            var article = await _articleRepository.GetArticleById(id);
            if(article == null)
            {
                return BadRequest($"Статьи с id={id} не существует!");
            }

            var tag = await _tagRepository.GetTagByContent(tagViewModel.Content);

            if(tag == null)
            {
                return NotFound($"Такого тега не существует!");
            }

            try
            {
                article.Tags.Add(tag);
                Article article2 = new();
                await _articleRepository.AddTag(article, tag);
                return Ok($"Тег \'{tag.Content}\' {article2.Tags.Count}  успешно добавлен к статье \'{article.Title}\'!");
            }
            catch
            {
                return BadRequest("Какая-то ошибка!");
            }
        }
    }
}
