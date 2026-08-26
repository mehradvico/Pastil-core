using Application.Common.Dto.Result;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.FileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// فایل
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        /// <summary>
        /// فایل
        /// </summary>
        ///        
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }
        /// <summary>
        ///  دریافت 
        /// </summary>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<FileVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await _fileService.FindVDtoAsync(id);
            return Ok(item);
        }
    }
}
