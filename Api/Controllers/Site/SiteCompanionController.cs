using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Site
{
    /// <summary>
    /// نمایندگان قابل نمایش در سایت معرفی پاستیل
    /// </summary>
    [Route("api/Site/Companion")]
    [ApiController]
    [AllowAnonymous]
    public class SiteCompanionController : ControllerBase
    {
        private readonly ICompanionService _companionService;

        public SiteCompanionController(ICompanionService companionService)
        {
            _companionService = companionService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] CompanionInputDto dto)
        {
            dto.Available = true;
            dto.Approved = true;
            dto.ShowToSite = true;
            return Ok(_companionService.Search(dto));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionService.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data == null || !result.Data.Active || !result.Data.Approved || !result.Data.ShowToSite)
                return Ok(new BaseResultDto<CompanionVDto>(false, Resource.Notification.NothingFound, null!));

            return Ok(result);
        }
    }
}
