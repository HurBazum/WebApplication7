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
        /// получение одного комментария по его идентификатору
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("/{id}")]
        public async Task<IActionResult> GetOneComment([FromRoute]int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            return Ok(_mapper.Map<CommentViewModel>(comment));
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

        [Authorize]
        [HttpPut]
        [Route("/ChangeComment/{id}")]
        public async Task<IActionResult> ChangeComment([FromRoute]int id, [FromBody]UpdateCommentRequest ucr)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if(comment == null)
            {
                return BadRequest($"Комментарий с таким id={id} не существует!");
            }

            await _commentRepository.UpdateComment(comment, _mapper.Map<UpdateCommentQuery>(ucr));
            return Ok("Комментарий успешно исправлен!");
        }
    }
}
