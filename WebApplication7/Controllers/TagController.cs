using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Repositories;
using WebApplication7.DAL.Enteties;
using WebApplication7.ViewModels.Tag;

namespace WebApplication7.Controllers
{
    [ApiController]
    [Route("/Tags")]
    public class TagController : Controller
    {
        readonly ITagRepository _tagRepository;
        readonly IMapper _mapper;
        public TagController(ITagRepository tagRepository, IMapper mapper)
        {
            _tagRepository = tagRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetTags()
        {
            var tags = _tagRepository.GetAll();
            return StatusCode(200, tags);
        }

        [HttpPost]
        public async Task<IActionResult> AddTag([FromBody]TagViewModel tagViewModel)
        {
            if(tagViewModel.Content == null)
            {
                return BadRequest($"Заполните поле \'content\'!");
            }

            var tag = _mapper.Map<Tag>(tagViewModel);

            try
            {
                await _tagRepository.AddTag(tag);
                return Ok($"Тег \'{tag.Content}\' успешно создан!");
            }
            catch
            {
                return BadRequest($"Какая-то ошибка!");
            }
        }
    }
}
