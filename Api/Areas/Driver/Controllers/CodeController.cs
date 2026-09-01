using Application.Common.Dto.Result;
using Application.Services.CategorySrv.Dto;
using Application.Services.CodeSrv.Dto;
using Application.Services.Setting.CodeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Driver.Controllers
{
    /// <summary>
    /// دیکشنری کد/گروه‌کد — فقط خواندنی، برای پرکردن لیست‌های ثابت (مثل دلایل لغو سفر) در اپ راننده
    /// </summary>
    [Area("Driver")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CodeController : ControllerBase
    {
        private readonly ICodeService _codeService;
        public CodeController(ICodeService codeService)
        {
            _codeService = codeService;
        }

        /// <summary>
        /// جستجو — با CodeGroupLabel فیلتر کن (مثلاً TripCancelReason_Driver)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CodeSearchDto), 200)]
        public IActionResult Get([FromQuery] CodeInputDto dto)
        {
            var searchDto = _codeService.Search(dto);
            return Ok(searchDto);
        }
    }
}
