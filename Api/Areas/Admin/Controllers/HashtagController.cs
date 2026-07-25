using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.Content.HashtagSrv.Dto;
using Application.Services.Content.HashtagSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  هشتگ ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class HashtagController : ControllerBase
    {
        private IHashtagService _hashtagService;
        public HashtagController(IHashtagService hashtagService)
        {
            this._hashtagService = hashtagService;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<HashtagDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = _hashtagService.Search(dto);
            return Ok(searchDto);
        }


    }
}
