using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionPetSrv.Dto;
using Application.Services.PansionSrvs.PansionPetSrv.Iface;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Application.Services.PansionSrvs.PansionOwnershipSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پت های پانسیون
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionPetController : ControllerBase
    {
        private readonly IPansionPetService _PansionPetService;
        private readonly IPansionService _PansionService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPansionOwnershipService _ownership;
        public PansionPetController(IPansionPetService PansionPetService, IPansionService PansionService, ICurrentUserHelper currentUser, IPansionOwnershipService ownership)
        {
            this._ownership = ownership;
            this._PansionPetService = PansionPetService;
            this._PansionService = PansionService;
            this._currentUser = currentUser;
        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionPetSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionPetInputDto dto)
        {
            dto.OwnerUserId = _currentUser.CurrentUser.UserId;
            var search = _PansionPetService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه خدمات</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionPetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var agency = await _PansionPetService.FindAsyncDto(id);
            if (agency.IsSuccess && agency.Data != null)
            {
                var owner = await _PansionService.FindAsyncVDto(agency.Data.PansionId);
                if (!owner.IsSuccess || await _ownership.PansionOfPansionAsync(owner.Data.Id, _currentUser.CurrentUser.UserId, approvedOnly: false) == null)
                    return Ok(new BaseResultDto<PansionPetDto>(false, Resource.Notification.AccessDenied, null));
            }
            return Ok(agency);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionPetDto>), 200)]
        public async Task<IActionResult> Post(PansionPetDto dto)
        {
            var pansion = await _PansionService.FindAsyncVDto(dto.PansionId);
            if (!pansion.IsSuccess || await _ownership.PansionOfPansionAsync(pansion.Data.Id, _currentUser.CurrentUser.UserId) == null)
                return Ok(new BaseResultDto<PansionPetDto>(false, Resource.Notification.AccessDenied, dto));

            var result = await _PansionPetService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionPetDto dto)
        {
            var existing = await _PansionPetService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var pansion = await _PansionService.FindAsyncVDto(existing.Data.PansionId);
            if (!pansion.IsSuccess || await _ownership.PansionOfPansionAsync(pansion.Data.Id, _currentUser.CurrentUser.UserId) == null)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            dto.PansionId = existing.Data.PansionId;
            var agency = _PansionPetService.UpdateDto(dto);
            return Ok(agency);
        }

        /// <summary>
        ///  حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _PansionPetService.FindAsyncVDto(id);
            if (!existing.IsSuccess)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            var pansion = await _PansionService.FindAsyncVDto(existing.Data.PansionId);
            if (!pansion.IsSuccess || await _ownership.PansionOfPansionAsync(pansion.Data.Id, _currentUser.CurrentUser.UserId) == null)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = _PansionPetService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
