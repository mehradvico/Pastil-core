using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Order.RebateSrv.Iface;
using Application.Services.PansionSrvs.PansionOwnershipSrv;
using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
using Entities.Entities.PastilClubField;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>کدهای تخفیف اختصاصی مرکزهای متعلق به نمایندهٔ جاری.</summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class RebateController : ControllerBase
    {
        private readonly IRebateService _rebates;
        private readonly IDataBaseContext _context;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPansionOwnershipService _pansions;
        private readonly ISchoolOwnershipService _schools;

        public RebateController(IRebateService rebates, IDataBaseContext context, ICurrentUserHelper currentUser,
            IPansionOwnershipService pansions, ISchoolOwnershipService schools)
        {
            _rebates = rebates;
            _context = context;
            _currentUser = currentUser;
            _pansions = pansions;
            _schools = schools;
        }

        private async Task<bool> OwnsAsync(ClubRewardTargetTypeEnum type, long targetId, bool approvedOnly = false)
        {
            if (targetId <= 0) return false;
            var userId = _currentUser.CurrentUser.UserId;
            return type switch
            {
                ClubRewardTargetTypeEnum.Coach => await _context.Coaches.AsNoTracking()
                    .AnyAsync(coach => coach.Id == targetId && coach.OwnerId == userId && !coach.Deleted && (!approvedOnly || (coach.Active && coach.Approved))),
                ClubRewardTargetTypeEnum.Clinic => await _context.Clinics.AsNoTracking()
                    .AnyAsync(clinic => clinic.Id == targetId && clinic.OwnerId == userId && !clinic.Deleted && (!approvedOnly || (clinic.Active && clinic.Approved))),
                ClubRewardTargetTypeEnum.Groomer => await _context.Groomers.AsNoTracking()
                    .AnyAsync(groomer => groomer.Id == targetId && groomer.OwnerId == userId && !groomer.Deleted && (!approvedOnly || (groomer.Active && groomer.Approved))),
                ClubRewardTargetTypeEnum.Pansion => await _pansions.PansionOfPansionAsync(targetId, userId, approvedOnly) != null
                    && await _context.Pansions.AsNoTracking().AnyAsync(pansion => pansion.Id == targetId && !pansion.Deleted),
                ClubRewardTargetTypeEnum.School => await _schools.SchoolOfSchoolAsync(targetId, userId, approvedOnly) != null
                    && await _context.Schools.AsNoTracking().AnyAsync(school => school.Id == targetId && !school.Deleted),
                _ => false
            };
        }

        [HttpGet("{targetType}/{targetId:long}")]
        public async Task<IActionResult> Get(ClubRewardTargetTypeEnum targetType, long targetId, [FromQuery] RebateInputDto input)
        {
            if (!await OwnsAsync(targetType, targetId)) return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            input.TargetType = targetType;
            input.TargetId = targetId;
            input.UserId = null;
            return Ok(_rebates.SearchByTarget(input));
        }

        [HttpPost("{targetType}/{targetId:long}")]
        public async Task<IActionResult> Post(ClubRewardTargetTypeEnum targetType, long targetId, RebateDto dto)
        {
            if (!await OwnsAsync(targetType, targetId, approvedOnly: true)) return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            dto.UserId = null;
            dto.SiteFundedPercent = 0m;
            dto.ProviderFundedPercent = 100m;
            return Ok(await _rebates.InsertForTargetAsync(dto, targetType, targetId));
        }

        [HttpPut("{targetType}/{targetId:long}/{id:long}/Active")]
        public async Task<IActionResult> Active(ClubRewardTargetTypeEnum targetType, long targetId, long id, [FromQuery] bool active)
        {
            if (!await OwnsAsync(targetType, targetId, approvedOnly: true) || !await CouponBelongsAsync(id, targetType, targetId))
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _rebates.SetActiveAsync(id, active));
        }

        [HttpDelete("{targetType}/{targetId:long}/{id:long}")]
        public async Task<IActionResult> Delete(ClubRewardTargetTypeEnum targetType, long targetId, long id)
        {
            if (!await OwnsAsync(targetType, targetId, approvedOnly: true) || !await CouponBelongsAsync(id, targetType, targetId))
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _rebates.DeleteForStoreAsync(id, null));
        }

        private Task<bool> CouponBelongsAsync(long id, ClubRewardTargetTypeEnum type, long targetId) =>
            _context.Rebate.AsNoTracking().AnyAsync(rebate => rebate.Id == id && !rebate.Deleted &&
                rebate.TargetType == type && rebate.TargetId == targetId &&
                (rebate.SiteFundedPercent == null || rebate.SiteFundedPercent == 0));
    }
}
