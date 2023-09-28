using Microsoft.AspNetCore.Mvc;

namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Comments")]
    public class CommentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
