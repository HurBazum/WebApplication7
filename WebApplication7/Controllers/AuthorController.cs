using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Repositories;
using WebApplication7.BLL.Models.Author;
using AutoMapper;
using WebApplication7.DAL.Queries.Author;
using WebApplication7.ViewModels.Account;

namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Author")]
    public class AuthorController : Controller
    {
        readonly IAuthorRepository _authorRepository;
        readonly IRoleRepository _roleRepository;
        readonly IArticleRepository _articleRepository;
        readonly ICommentRepository _commentRepository;
        readonly IMapper _mapper;
        public AuthorController(IAuthorRepository authorRepository, IRoleRepository roleRepository, IArticleRepository articleRepository, ICommentRepository commentRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _roleRepository = roleRepository;
            _articleRepository = articleRepository;
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("/Register")]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel registerViewModel)
        {
            if(registerViewModel.FirstName == null)
            {
                return BadRequest("Поле FirstName необходимо заполнить!");
            }
            if(registerViewModel.LastName == null)
            {
                return BadRequest("Поле LastName необходимо заполнить!");
            }
            if(registerViewModel.Email == null)
            {
                return BadRequest("Поле Email необходимо заполнить!");
            }
            if(registerViewModel.Login == null)
            {
                return BadRequest("Поле Login необходимо заполнить!");
            }

            //var existsAuthor = _authorRepository.GetAuthorByEmail(registerViewModel.Email);

            //if(existsAuthor != null) 
            //{
            //    return BadRequest($"Автор с емейлом:\'{registerViewModel.Email}\' уже существует!");
            //}

            var author = _mapper.Map<Author>(registerViewModel);

            // default role "User"
            var role = await _roleRepository.GetRoleById(1);

            author.Roles.Add(role);

            if(_authorRepository.AddAuthor(author).IsCompletedSuccessfully)
            {
                return Ok();
            }

            return StatusCode(415, "");
        }

        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> UpdateAuthor([FromRoute]int id, [FromBody]UpdateAuthorRequest uar)
        { 
            var author = await _authorRepository.GetAuthorById(id);
            if(author == null)
            {
                return BadRequest($"Автор с id = {id} уже существует!");
            }


            if(_authorRepository.GetAll().Result.Any(a => a.Email == uar.NewEmail))
            {
                return BadRequest($"Автор с емейлом:\'{uar.NewEmail}\' уже существует!");
            }

            UpdateAuthorQuery updateAuthorQuery = _mapper.Map<UpdateAuthorQuery>(uar);

            await _authorRepository.UpdateAuthor(author, updateAuthorQuery);
            var newAuthor = await _authorRepository.GetAuthorById(id);  
            return Ok(newAuthor);
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAuthor([FromRoute]int id)
        {
            var author = await _authorRepository.GetAuthorById(id);
            if (author == null)
            {
                return BadRequest($"author with id={id} doesn't exists!");
            }

            //var authorComments = _commentRepository.Equals(author);

            return Ok($"{author} was deleted!");
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel lvm)
        {
            if(lvm.Email == null)
            {
                return BadRequest($"Заполните емэйл!");
            }
            if(lvm.Password == null)
            {
                return BadRequest($"Заполните пароль!");
            }

            var author = await _authorRepository.GetAuthorByEmail(lvm.Email);

            if(author == null)
            {
                return NotFound($"Пользователь с емейлом \'{lvm.Email}\' не существует!");
            }
            if(author.Password != lvm.Password)
            {
                return BadRequest($"Пароли не совпадают!");
            }

            return Ok($"Здравствуйте, {author.FirstName}!");
        }
    }
}
