using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Repositories;
using WebApplication7.DAL.Enteties;
using WebApplication7.ViewModels.Article;
using WebApplication7.ViewModels.Tag;
using Microsoft.AspNetCore.Authorization;
using WebApplication7.BLL.Models.Article;
using WebApplication7.DAL.Queries.Article;
using System.Diagnostics;

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

        //[Authorize]
        [HttpGet]
        [Route("/All_Articles")]
        public async Task<IActionResult> GetAllArticles()
        {
            var articles = _mapper.Map<ArticleViewModel[]>(await _articleRepository.GetAll());

            return Ok(articles);
        }

        [Authorize]
        [HttpGet]
        [Route("/MyArticles")]
        public async Task<IActionResult> GetMyArticles()
        {
            var me = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);
            var myArticles = _mapper.Map<ArticleViewModel[]>(await _articleRepository.GetArticlesByAuthor(me));

            if(myArticles.Length == 0)
            {
                return BadRequest($"У вас пока нет ни одной статьи");
            }

            return Ok(myArticles);
        }

        [Authorize]
        [HttpGet]
        [Route("/Authors/{id}")]
        public async Task<IActionResult> GetAuthorsArticles([FromRoute]int id)
        {
            var author = await _authorRepository.GetAuthorById(id);

            if(author == null)
            {
                return BadRequest($"Пользователя с id={id} не существует!");
            }

            var articles = _mapper.Map<ArticleViewModel[]>(await _articleRepository.GetArticlesByAuthor(author));

            return Ok(articles);
        }

        [Authorize]
        [HttpPost]
        [Route("/AddArticle")]
        public async Task<IActionResult> WriteArticle([FromBody]ArticleViewModel articleViewModel)
        {
            var author = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);

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



        #region изменения статей

        // проверка на существование статьи
        // и возможность её изменения данным пользователем
        async Task<object> CheckRightsAndArticleValue(int id)
        {
            // т.к. статьи могут изменять модераторы и простые 
            // пользователи, необходимо проверить, просто
            // пользователь пытается изменить статью или модератор.
            // модератор может изменять любые статьи, пользователь -
            // только свои

            var article = _articleRepository.GetArticleById(id);
            var currentAuthor = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);
            var isModerator = _authorRepository.GetAuthorsRoles(currentAuthor).Result.Any(role => role.Name == "Moderator");

            if (article == null)
            {
                return BadRequest($"Статьи с id={id} не существует!");
            }

            if (article.Result.AuthorId != currentAuthor.Id && isModerator == false)
            {
                return BadRequest($"Вам нельзя изменять чужие статьи!");
            }

            return await article;
        }

        /// <summary>
        /// добавление тегов к статьям 
        /// </summary>
        [Authorize(Roles = "User, Moderator")]
        [HttpPatch]
        [Route("/{id}/Tags")]
        public async Task<IActionResult> SetArticlesTags([FromRoute]int id, [FromBody]TagViewModel tagViewModel)
        {

            // получаем результат проверки и создаём объект статьи,
            // на случай доступности изменения
            var result = CheckRightsAndArticleValue(id);

            Article article;

            if(result.Result is ActionResult)
            {
                // возвращаем BadRequest()
                return result.Result as ActionResult;
            }
            else
            {
                article = result.Result as Article;
            }

            var tag = await _tagRepository.GetTagByContent(tagViewModel.Content);

            if(tag == null)
            {
                return BadRequest($"Такого тега не существует!");
            }

            // получаем коллекцию тегов статьи, чтобы проверить
            // нет ли там уже такого тега, ибо добавлять можно
            // только разные теги
            var alreadyHasTag = _articleRepository.GetArticlesTags(article!).Result.Any(t => t.Content == tag.Content);

            if(alreadyHasTag)
            {
                return BadRequest("Эта статья уже имеет этот тег");
            }

            try
            {
                article!.Tags.Add(tag);
                await _articleRepository.AddTag(article, tag);
                return Ok($"Тег \'{tag.Content}\' успешно добавлен к статье \'{article.Title}\'!");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.InnerException!.ToString());
            }
            return StatusCode(500, "");
        }

        /// <summary>
        /// изменение существующей статьи
        /// </summary>
        [Authorize(Roles = "User, Moderator")]
        [HttpPut]
        [Route("/ArticlesUpdate/{id}")]
        public async Task<IActionResult> RewriteArticle([FromRoute]int id, [FromBody] UpdateArticleRequest uar)
        {
            var result = CheckRightsAndArticleValue(id);
            
            Article article;

            if(result.Result is ActionResult)
            {
                return result.Result as ActionResult;
            }
            else
            {
                article = result.Result as Article;
            }

            // 
            if(uar.NewTitle == null && uar.NewContent == null)
            {
                return BadRequest("Если что-то хочется изменить, нужно что-то изменить!");
            }

            await _articleRepository.UpdateArticle(article, _mapper.Map<UpdateArticleQuery>(uar));

            return Ok($"Статья \'{article.Title}\' успешно изменена!");
        }

        /// <summary>
        /// удаление статьи
        /// без CheckRightsAndArticleValue
        /// не знаю, хорошо ли его использовать
        /// </summary>
        [Authorize(Roles = "User, Moderator")]
        [HttpDelete]
        [Route("/DeleteArticle/{id}")]
        public async Task<IActionResult> DeleteArticle([FromRoute] int id)
        {
            var article = await _articleRepository.GetArticleById(id);
            if (article == null)
            {
                return BadRequest($"Статьи с id={id} не существует!");
            }

            // получаем утверждения текущего пользователя
            var currentUser = HttpContext.User.Claims;

            // проверяем модератор он, автор статьи или нет
            var isModerator = currentUser.FirstOrDefault(claim => claim.Value.Equals("Moderator"));
            if (isModerator == null || article.AuthorId == _authorRepository.GetAuthorByEmail(currentUser.First().Value).Result.Id)
            {
                return BadRequest("Вам нельзя удалять чужие статьи");
            }

            await _articleRepository.DeleteArticle(article);


            return Ok($"Статья \'{article.Title}\' успешно удалена!");
        }

        #endregion
    }
}
