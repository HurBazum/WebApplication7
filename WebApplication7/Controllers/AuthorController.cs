using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Enteties;
using WebApplication7.DAL.Repositories;
using WebApplication7.BLL.Models.Author;
using AutoMapper;
using WebApplication7.DAL.Queries.Author;
using WebApplication7.ViewModels.Account;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

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
        public async Task<IActionResult> Register([FromBody] RegisterViewModel registerViewModel)
        {
            var author = _mapper.Map<Author>(registerViewModel);

            if (await _authorRepository.GetAuthorByEmail(registerViewModel.Email) != null)
            {
                return BadRequest($"Пользователь с емейлом \'{registerViewModel.Email}\' уже зарегистрирован!");
            }
            // default role "User" == 3, неправильно добавлял данные в таблицу, поэтому
            // индексация теперь начинается с 3х
            var role = await _roleRepository.GetRoleById(3);

            author.Roles.Add(role);

            if (_authorRepository.AddAuthor(author).IsCompletedSuccessfully)
            {
                //return Ok($"Добро пожаловать, {author.FirstName}");
                return RedirectToAction("Login", new LoginViewModel { Email = author.Email, Password = author.Password });
            }

            return StatusCode(415, "");
        }



        /// <summary>
        /// изменять можно только свой профиль,
        /// админ может изменять чужие,
        /// св-ва UpdateAuthorRequest могут принимать значения null,
        /// для этого надо надо вместо значения ввести nullБ
        /// пока нет 
        /// </summary>
        [Authorize(Roles = "User, Admin")]
        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> UpdateAuthor([FromRoute] int id, [FromBody] UpdateAuthorRequest uar)
        {
            var emailIsNotAvailable = default(bool);
            // получаем емейл текущего пользователя по емейлу из утверждений
            var currentUserEmail = HttpContext.User.Claims.FirstOrDefault().Value;

            // устанавливаем, является ли данный пользователь администратором
            bool adminRole = HttpContext.User.Claims.Any(c => c.Value == "Admin");

            // текущий пользователь
            var currentAuthor = await _authorRepository.GetAuthorByEmail(currentUserEmail);

            var author = await _authorRepository.GetAuthorById(id);

            if (author == null)
            {
                return BadRequest($"Автор с id = {id} не существует!");
            }

            // если пользователь не является пользователем, которого хотят изменить,
            // при этом также не обладает ролью Admin
            if (currentAuthor != author && adminRole != true)
            {
                return BadRequest("Невозможно изменить не свой профиль!");
            }

            if (uar.NewEmail != null)
            {
                emailIsNotAvailable = _authorRepository.GetAll().Result.Any(a => a.Email == uar.NewEmail);
            }
            // проверка на уникальность емейла
            if (emailIsNotAvailable == true)
            {
                return BadRequest($"Автор с емейлом:\'{uar.NewEmail}\' уже существует!");
            }

            // 
            //if (uar.NewEmail != null && emailIsNotAvailable == false)
            //{
            //    HttpContext.User.Claims.First().WriteTo(HttpContext.User.Identities.ToList()[0].Value.Replace(currentUserEmail, uar.NewEmail);
            //}

            UpdateAuthorQuery updateAuthorQuery = _mapper.Map<UpdateAuthorQuery>(uar);

            await _authorRepository.UpdateAuthor(author, updateAuthorQuery);
            var newAuthor = await _authorRepository.GetAuthorById(id);
            return Ok(newAuthor);
        }

        /// <summary>
        /// возможно, ещё сделать можно доступным для User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAuthor([FromRoute] int id)
        {
            var author = await _authorRepository.GetAuthorById(id);

            if (author == null)
            {
                return BadRequest($"Автор с id={id} не существует!");
            }

            if (_authorRepository.DeleteAuthor(author).IsCompletedSuccessfully)
            {
                return Ok($"Автор с id={id} был удалён!");
            }
            else
            {
                Debug.WriteLine("yt dsikj");
                return StatusCode(401, "");
            }
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel lvm)
        {
            if (lvm.Email == null)
            {
                return BadRequest($"Заполните емэйл!");
            }
            if (lvm.Password == null)
            {
                return BadRequest($"Заполните пароль!");
            }

            var author = await _authorRepository.GetAuthorByEmail(lvm.Email);

            if (author == null)
            {
                return NotFound($"Пользователь с емейлом \'{lvm.Email}\' не существует!");
            }
            if (author.Password != lvm.Password)
            {
                return BadRequest($"Пароли не совпадают!");
            }

            // записываем емейл пользователя в утверждения
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, author.Email)
            };

            // получаем роли пользователя, 
            // добавляем их названия в утверждения
            var roles = await _authorRepository.GetAuthorsRoles(author);

            foreach (var role in roles)
            {
                claims.Add(new(ClaimsIdentity.DefaultRoleClaimType, role.Name));
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                claims,
                "AppCockie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(claimsIdentity));

            return Ok($"Здравствуйте, {author.FirstName}!");
        }

        [Authorize]
        [HttpGet]
        [Route("/Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return Ok($"Пока!");
        }

        // добавление администратора и модератора
        [HttpPost]
        [Route("/Create_Admin")]
        public async Task<IActionResult> AddAdmin([FromBody] RegisterViewModel registerViewModel)
        {
            var author = _mapper.Map<Author>(registerViewModel);

            // добавляем роли User и Admin
            author.Roles.Add(await _roleRepository.GetRoleById(3));
            author.Roles.Add(await _roleRepository.GetRoleById(5));

            if (_authorRepository.AddAuthor(author).IsCompletedSuccessfully)
            {
                return Ok($"Добро пожаловать, {author.FirstName}");
                //return RedirectToAction("Login", "Author", new LoginViewModel { Email = author.Email, Password = author.Password });
            }

            return StatusCode(415, "");
        }

        [HttpPost]
        [Route("/Create_Moderator")]
        public async Task<IActionResult> AddModerator([FromBody] RegisterViewModel registerViewModel)
        {
            var author = _mapper.Map<Author>(registerViewModel);

            // добавляем роли User и Moderator
            author.Roles.Add(await _roleRepository.GetRoleById(3));
            author.Roles.Add(await _roleRepository.GetRoleById(4));

            if(_authorRepository.AddAuthor(author).IsCompletedSuccessfully)
            {
                return Ok($"Добро пожаловать, {author.FirstName}");
                //return RedirectToAction("Login", "Author", new LoginViewModel { Email = author.Email, Password = author.Password });
            }

            return StatusCode(415, "");
        }

        #region test
        /// <summary>
        /// т.к. такая проверка выполняется в трёх методах, 
        /// для создания авторов с различными наборами ролей
        /// был сделан этот
        /// ОТКЛЮЧЕН
        /// </summary>
        string MessageBadRegister(RegisterViewModel registerViewModel)
        {
            string nullProperty = string.Empty;
            if (registerViewModel.FirstName == null)
            {
                nullProperty += "FirstName";
            }
            if (registerViewModel.LastName == null)
            {
                nullProperty += "LastName";
            }
            if (registerViewModel.Email == null)
            {
                nullProperty += "Email";
            }
            if (registerViewModel.Login == null)
            {
                nullProperty += "Login";
            }
            return nullProperty;
        }
        #endregion
    }
}