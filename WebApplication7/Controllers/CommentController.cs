using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WebApplication7.DAL.Repositories;
using WebApplication7.ViewModels.Comment;
using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Queries.Comment;
using WebApplication7.BLL.Models.Comment;


namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Comments")]
    public class CommentController : Controller
    {
        ICommentRepository _commentRepository;
        IArticleRepository _articleRepository;
        IAuthorRepository _authorRepository;
        IMapper _mapper;
        public CommentController(ICommentRepository commentRepository, IAuthorRepository authorRepository, IArticleRepository articleRepository,IMapper mapper)
        {
            _commentRepository = commentRepository;
            _articleRepository = articleRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// для просмотра всех комментариев
        /// для Moderator
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Moderator")]
        [HttpGet]
        [Route("/AllComments")]
        public async Task<IActionResult> GetAllComments()
        {
            var allComments = await _commentRepository.GetAll();
            return Ok(allComments);
        }

        /// <summary>
        /// получение одного комментария по его идентификатору
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("/{id}")]
        public async Task<IActionResult> GetOneComment([FromRoute]int id)
        {
            var comment = await _commentRepository.GetCommentById(id);

            if(comment == null)
            {
                return BadRequest($"Комментария с id={id} не существует!");
            }
            var result = _mapper.Map<CommentViewModel>(comment);

            // устанавливаем авторство по логину
            result.Author = _authorRepository.GetAuthorById(comment.AuthorId).Result.Login;

            // устанавливаем название статьи
            result.Article = _articleRepository.GetArticleById(comment.ArticleId).Result.Title;

            return Ok(result);
        }

        /// <summary>
        /// удаление комментария по его идентификатору
        /// </summary>
        [Authorize(Roles = "User, Moderator")]
        [HttpDelete]
        [Route("/Delete_Comment/{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute]int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            var currentUser = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);
            var isModerator = HttpContext.User.Claims.ToList().Any(claim => claim.Value == "Moderator");

            if(currentUser.Id != comment.AuthorId && isModerator == false)
            {
                return BadRequest("Вам нельзя удалять чужие комментарии");
            }

            await _commentRepository.DeleteComment(comment);

            return Ok($"Комментарий с id={id} был успешно удалён!");
        }

        /// <summary>
        /// добавление комментария к статье
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route("/Articles/{id}")]
        public async Task<IActionResult> AddComment([FromRoute] int id, [FromBody]CreateCommentViewModel ccvw)
        {
            var currentUser = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);
            var article = await _articleRepository.GetArticleById(id);

            if(article == null)
            {
                return BadRequest($"Статьи с таким id={id} не существует!");
            }

            var comment = _mapper.Map<Comment>(ccvw);
            comment.AuthorId = currentUser.Id;
            comment.ArticleId = article.Id;
            await _commentRepository.AddComment(comment);

            return Ok($"Комментарий успешно добавлен к статье");
        }

        [Authorize(Roles = "User, Moderator")]
        [HttpPut]
        [Route("/ChangeComment/{id}")]
        public async Task<IActionResult> ChangeComment([FromRoute]int id, [FromBody]UpdateCommentRequest ucr)
        {
            var comment = await _commentRepository.GetCommentById(id);

            if(comment == null)
            {
                return BadRequest($"Комментарий с таким id={id} не существует!");
            }

            var currentUser = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);

            var isModerator = HttpContext.User.Claims.ToList().Any(claim => claim.Value == "Moderator");

            if (currentUser.Id != comment.AuthorId && isModerator == false)
            {
                return BadRequest("Вам нельзя изменять чужие комментарии");
            }

            await _commentRepository.UpdateComment(comment, _mapper.Map<UpdateCommentQuery>(ucr));

            return Ok("Комментарий успешно исправлен!");
        }
    }
}