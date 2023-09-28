using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Repositories;

namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Role")]
    public class RoleController : Controller
    {
        readonly IRoleRepository _roleRepository;
        public RoleController(IRoleRepository roleRepository) => _roleRepository = roleRepository;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepository.GetAll();
            if (roles != null)
            {
                return StatusCode(200, roles);
            }
            return BadRequest($"Нет доступных ролей!");
        }
    }
}