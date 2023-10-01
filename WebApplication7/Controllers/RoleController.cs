using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Repositories;

namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Role")]
    public class RoleController : Controller
    {
        readonly IRoleRepository _roleRepository;
        readonly IAuthorRepository _authorRepository;
        public RoleController(IRoleRepository roleRepository, IAuthorRepository authorRepository)
        {
            _roleRepository = roleRepository;
            _authorRepository = authorRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ShowAvailableRoles()
        {
            var roles = await _roleRepository.GetAll();
            if (roles != null)
            {
                return StatusCode(200, roles);
            }
            return BadRequest($"Нет доступных ролей!");
        }

        /// <summary>
        /// ввиду неправильного заполнения таблицы, 
        /// индексация начинается с трёх
        /// </summary>
        [HttpGet]
        [Route("/[controller]/{id}")]
        public async Task<IActionResult> ShowTheRole([FromRoute] int id)
        {
            var role = await _roleRepository.GetRoleById(id);

            if (role != null)
            {
                return StatusCode(200, role);
            }

            return BadRequest($"Роли с id={id} не существует!");
        }

        /// <summary>
        /// просмотр своих ролей
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("/[controller]/GetMyRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var author = await _authorRepository.GetAuthorByEmail(HttpContext.User.Claims.First().Value);
            var roles = await _authorRepository.GetAuthorsRoles(author);
            return Ok(roles);
        }
    }
}