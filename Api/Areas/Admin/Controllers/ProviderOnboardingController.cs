using Application.Common.Enumerable;
using Application.Common.Interface;
using Entities.Entities.ProviderField;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Data;
using Persistence.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderOnboardingController : ControllerBase
    {
        private readonly IDataBaseContext _db;
        private readonly ICurrentUserHelper _currentUser;
        public ProviderOnboardingController(IDataBaseContext db, ICurrentUserHelper currentUser)
        {
            _db = db; _currentUser = currentUser;
        }

        // Identity documents are restricted to the full administrator role, in addition to Admin-area JWT checks.
        private bool IsFullAdmin => _currentUser.CurrentUser?.RoleId == (long)RoleEnum.Admin;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string kind = null, [FromQuery] string status = null)
        {
            if (!IsFullAdmin) return Forbid();
            var query = _db.ProviderOnboardingApplications.AsNoTracking().AsQueryable();
            if (kind == "Provider" || kind == "Agency") query = query.Where(x => x.Kind == kind);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            var data = await query.OrderByDescending(x => x.SubmittedAt).ThenByDescending(x => x.Id).Take(200)
                .Select(x => new { x.Id, x.UserId, x.Kind, x.RoleLabel, x.Status, x.BusinessName,
                    x.FirstName, x.LastName, x.City, x.SubmittedAt, x.ReviewerNote }).ToListAsync();
            return Ok(new { isSuccess = true, data });
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            if (!IsFullAdmin) return Forbid();
            Response.Headers.CacheControl = "no-store";
            var x = await _db.ProviderOnboardingApplications.AsNoTracking().Include(a => a.Documents).Include(a => a.Events)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (x == null) return NotFound();
            return Ok(new { isSuccess = true, data = new { x.Id, x.UserId, x.ProviderId, x.Kind, x.RoleLabel, x.Status,
                x.FirstName, x.LastName, x.NationalCode, x.BirthDate, x.ApplicantType, x.CompanyName,
                x.CompanyNationalId, x.City, x.BusinessName, x.BusinessAddress, x.BusinessPhone,
                x.DetailsJson, x.ReviewerNote, x.SubmittedAt, x.ReviewedAt,
                documents = x.Documents.Select(d => new { d.Id, d.Kind, d.FileName }),
                events = x.Events.OrderByDescending(e => e.CreatedAt).Select(e => new { e.Status, e.Note, e.ActorUserId, e.CreatedAt }) } });
        }

        [HttpGet("documents/{documentId:long}")]
        public async Task<IActionResult> Document(long documentId)
        {
            if (!IsFullAdmin) return Forbid();
            var document = await _db.ProviderOnboardingDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == documentId);
            if (document == null) return NotFound();
            if (!SensitiveDataProtector.IsConfigured) return StatusCode(503);
            Response.Headers.CacheControl = "no-store";
            return File(Convert.FromBase64String(SensitiveDataProtector.Unprotect(document.ProtectedContent)), document.ContentType);
        }

        [HttpPost("{id:long}/decision")]
        public async Task<IActionResult> Decision(long id, [FromBody] OnboardingDecision input)
        {
            if (!IsFullAdmin) return Forbid();
            if (input == null || !new[] { "Approved", "NeedsEdit", "Rejected" }.Contains(input.Status)) return BadRequest();
            if (input.Status != "Approved" && string.IsNullOrWhiteSpace(input.Note))
                return BadRequest(new { isSuccess = false, message = "دلیل اصلاح یا رد را وارد کنید." });
            await using var tx = await _db.BeginTransactionAsync(IsolationLevel.Serializable);
            var item = await _db.ProviderOnboardingApplications.AsTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();
            if (item.Status != "PendingReview") return Conflict(new { isSuccess = false, message = "درخواست در انتظار بررسی نیست." });
            if (input.Status == "Approved" && item.Kind == "Provider")
            {
                var provider = await _db.Providers.AsTracking().FirstOrDefaultAsync(x => x.UserId == item.UserId && !x.Deleted);
                if (provider == null)
                {
                    provider = new Provider { UserId = item.UserId, Active = true, Approved = true, CreateDate = DateTime.UtcNow };
                    _db.Providers.Add(provider);
                }
                else { provider.Active = true; provider.Approved = true; }
                var user = await _db.Users.AsTracking().FirstAsync(x => x.Id == item.UserId);
                user.IsProvider = true;
                user.FirstName = item.FirstName;
                user.LastName = item.LastName;
                await _db.SaveChangesAsync();
                item.ProviderId = provider.Id;
            }
            if (input.Status == "Approved" && item.Kind == "Agency")
            {
                var provider = await _db.Providers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == item.UserId && x.Active && x.Approved && !x.Deleted);
                if (provider == null) return Conflict(new { isSuccess = false, message = "ارائه‌دهنده هنوز تأیید نشده است." });
                var role = await _db.ProviderRoles.AsNoTracking().FirstOrDefaultAsync(x => x.Label == item.RoleLabel && x.Active && !x.Deleted);
                if (role == null) return Conflict(new { isSuccess = false, message = "نقش فعالیت فعال نیست." });
                var assignment = await _db.ProviderRoleAssignments.AsTracking().FirstOrDefaultAsync(x => x.ProviderId == provider.Id && x.ProviderRoleId == role.Id && !x.Deleted);
                if (assignment == null) _db.ProviderRoleAssignments.Add(new ProviderRoleAssignment { ProviderId = provider.Id, ProviderRoleId = role.Id, Approved = true });
                else assignment.Approved = true;
                item.ProviderId = provider.Id;
            }
            item.Status = input.Status;
            item.ReviewerNote = input.Note?.Trim();
            item.ReviewedByUserId = _currentUser.CurrentUser.UserId;
            item.ReviewedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            _db.ProviderOnboardingEvents.Add(new ProviderOnboardingEvent { ApplicationId = item.Id,
                ActorUserId = _currentUser.CurrentUser.UserId, Status = input.Status,
                Note = item.ReviewerNote, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Ok(new { isSuccess = true });
        }
    }

    public class OnboardingDecision
    {
        public string Status { get; set; }
        public string Note { get; set; }
    }
}
