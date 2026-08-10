using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// خدمات قابل نمایش در سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Assistance")]
    [ApiController]
    [AllowAnonymous]
    public class SiteAssistanceController : ControllerBase
    {
        private readonly IAssistanceService _assistanceService;

        public SiteAssistanceController(IAssistanceService assistanceService)
        {
            _assistanceService = assistanceService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] AssistanceInputDto dto)
        {
            dto.Available = true;
            dto.ShowToSite = true;
            return Ok(_assistanceService.Search(dto));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _assistanceService.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data == null || !result.Data.Active || !result.Data.ShowToSite)
                return Ok(new BaseResultDto<AssistanceVDto>(false, Resource.Notification.NothingFound, null!));

            return Ok(result);
        }
    }
}
