using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication7.DAL.Repositories;
using WebApplication7.DAL.Enteties;
using WebApplication7.ViewModels.Tag;
using Microsoft.AspNetCore.Authorization;
using WebApplication7.DAL.Queries.Tag;
using WebApplication7.BLL.Models.Tag;

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _tagRepository.GetAll();

            if(tags == null)
            {
                return Ok("В блоге пока нет тегов!");
            }

            var tagsModels = _mapper.Map<TagViewModel[]>(tags);

            return Ok(tagsModels);
        }

        [Authorize]
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

        [Authorize]
        [HttpGet]
        [Route("/Tags/{id}")]
        public async Task<IActionResult> GetOneTag([FromRoute]int id)
        {
            var tag = await _tagRepository.GetTagById(id);

            if(tag == null)
            {
                return BadRequest($"Тега с id={id} не существует!");
            }

            var model = _mapper.Map<TagViewModel>(tag);

            return Ok(model);
        }

        [Authorize(Roles = "Moderator")]
        [HttpDelete]
        [Route("/DeleteTag/{id}")]
        public async Task<IActionResult> DeleteTag([FromRoute]int id)
        {
            var tag = await _tagRepository.GetTagById(id);

            if (tag == null)
            {
                return BadRequest($"Тега с id={id} не существует!");
            }

            await _tagRepository.DeleteTag(tag);

            return Ok($"Тег \'{tag.Content}\' успешно удалён!");
        }

        [Authorize(Roles = "Moderator")]
        [HttpPut]
        [Route("/ChangeTags/{id}")]
        public async Task<IActionResult> ChangeTag([FromRoute]int id, [FromBody]UpdateTagRequest utr)
        {
            var tag = await _tagRepository.GetTagById(id);

            if (tag == null)
            {
                return BadRequest($"Тега с id={id} не существует!");
            }

            if(utr.NewContent == null)
            {
                return BadRequest("Введите изменнёное содержание тега!");
            }

            await _tagRepository.UpdateTag(tag, _mapper.Map<UpdateTagQuery>(utr));

            return Ok($"Тег \'{tag.Content}\' успешно удалён!");
        }
    }
}
