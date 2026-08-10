using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// پانسیون‌های قابل نمایش در سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Pansion")]
    [ApiController]
    [AllowAnonymous]
    public class SitePansionController : ControllerBase
    {
        private readonly IPansionService _pansionService;

        public SitePansionController(IPansionService pansionService)
        {
            _pansionService = pansionService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] PansionInputDto dto)
        {
            dto.Available = true;
            dto.Approve = true;
            dto.ShowToSite = true;
            return Ok(_pansionService.Search(dto));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pansionService.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data == null || !result.Data.Active || !result.Data.Approve || !result.Data.ShowToSite)
                return Ok(new BaseResultDto<PansionVDto>(false, Resource.Notification.NothingFound, null!));

            return Ok(result);
        }
    }
}
