using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Dto;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Iface;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت گالری عکس مدرسه (توسط خودِ نماینده)
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolPictureController : ControllerBase
    {
        private readonly ISchoolPictureService SchoolPictureService;
        private readonly ISchoolService _SchoolService;
        private readonly ICurrentUserHelper _currentUser;

        public SchoolPictureController(ISchoolPictureService SchoolPictureService, ISchoolService SchoolService, ICurrentUserHelper currentUser)
        {
            this.SchoolPictureService = SchoolPictureService;
            this._SchoolService = SchoolService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        ///
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var SchoolPicture = await SchoolPictureService.FindAsyncVDto(id);
            return Ok(SchoolPicture);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(SchoolPictureSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolPictureInputDto dto)
        {
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var SchoolPicture = SchoolPictureService.Search(dto);
            return Ok(SchoolPicture);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureDto>), 200)]
        public async Task<IActionResult> Post(SchoolPictureDto SchoolPictureDto)
        {
            var school = await _SchoolService.FindAsyncVDto(SchoolPictureDto.SchoolId);
            if (!school.IsSuccess || school.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<SchoolPictureDto>(false, Resource.Notification.AccessDenied, SchoolPictureDto));

            var result = await SchoolPictureService.InsertAsyncDto(SchoolPictureDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureDto>), 200)]
        public async Task<IActionResult> Put(SchoolPictureDto SchoolPictureDto)
        {
            var existing = await SchoolPictureService.FindAsyncVDto(SchoolPictureDto.Id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto<SchoolPictureDto>(false, Resource.Notification.AccessDenied, SchoolPictureDto));
            var school = await _SchoolService.FindAsyncVDto(existing.Data.SchoolId);
            if (!school.IsSuccess || school.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<SchoolPictureDto>(false, Resource.Notification.AccessDenied, SchoolPictureDto));

            SchoolPictureDto.SchoolId = existing.Data.SchoolId;
            var result = SchoolPictureService.UpdateDto(SchoolPictureDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<SchoolPictureDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await SchoolPictureService.FindAsyncVDto(id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto<SchoolPictureDto>(false, Resource.Notification.AccessDenied, default!));
            var school = await _SchoolService.FindAsyncVDto(existing.Data.SchoolId);
            if (!school.IsSuccess || school.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<SchoolPictureDto>(false, Resource.Notification.AccessDenied, default!));

            var result = SchoolPictureService.DeleteDto(id);
            return Ok(result);
        }
    }
}
