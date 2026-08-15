using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تخصص ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpertiseController : ControllerBase
    {
        private readonly IExpertiseService expertiseService;

        public ExpertiseController(IExpertiseService expertiseService)
        {
            this.expertiseService = expertiseService;
        }

        /// <summary>
        /// اطلاعات تخصص
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ExpertiseVDto>), 200)]
        public async Task<IActionResult> Get(long id) =>
            Ok(await expertiseService.FindAsyncVDto(id));

        /// <summary>
        /// جستجوی تخصص‌ها
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ExpertiseSearchDto), 200)]
        public IActionResult Get([FromQuery] ExpertiseInputDto dto) =>
            Ok(expertiseService.Search(dto));

        /// <summary>
        /// ثبت تخصص
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post(ExpertiseDto dto) =>
            Ok(await expertiseService.InsertValidatedAsync(dto));

        /// <summary>
        /// ویرایش تخصص
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Put(ExpertiseDto dto) =>
            Ok(await expertiseService.UpdateValidatedAsync(dto));

        /// <summary>
        /// حذف تخصص
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(long id) =>
            Ok(await expertiseService.DeleteValidatedAsync(id));
    }
}
