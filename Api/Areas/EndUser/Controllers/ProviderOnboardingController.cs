using Application.Common.Interface;
using Entities.Entities.ProviderField;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using Persistence.Security;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderOnboardingController : ControllerBase
    {
        private static readonly string[] Roles = { "Store", "Clinic", "Groomer", "Coach", "School", "Pansion" };
        private static readonly string[] DocumentKinds = { "NationalFront", "NationalBack", "CompanyProof", "BusinessLicense", "VenuePhoto", "Qualification" };
        private static readonly System.Collections.Generic.Dictionary<string, string[]> RequiredRoleFields = new()
        {
            ["Store"] = new[] { "salesChannels", "productCategories", "delivery" },
            ["Groomer"] = new[] { "services", "petTypes", "experience" },
            ["Clinic"] = new[] { "technicalManager", "licenseNumber", "services" },
            ["Coach"] = new[] { "specialty", "experience", "sessionPlace" },
            ["School"] = new[] { "program", "capacity", "staff" },
            ["Pansion"] = new[] { "petTypes", "capacity", "care" },
        };
        private readonly IDataBaseContext _db;
        private readonly ICurrentUserHelper _currentUser;

        public ProviderOnboardingController(IDataBaseContext db, ICurrentUserHelper currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(new
        {
            isSuccess = true,
            providerApproved = await _db.Providers.AsNoTracking().AnyAsync(x => x.UserId == UserId && x.Approved && x.Active && !x.Deleted),
            data = await _db.ProviderOnboardingApplications.AsNoTracking()
                .Where(x => x.UserId == UserId).OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Kind, x.RoleLabel, x.Status, x.BusinessName, x.ReviewerNote,
                    x.CreatedAt, x.SubmittedAt, x.ReviewedAt }).ToListAsync()
        });

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            Response.Headers.CacheControl = "no-store";
            var item = await _db.ProviderOnboardingApplications.AsNoTracking()
                .Include(x => x.Documents).Include(x => x.Events).FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId);
            return item == null ? NotFound() : Ok(new { isSuccess = true, data = View(item) });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApplicationInput input)
        {
            if (input == null || (input.Kind != "Provider" && input.Kind != "Agency")) return BadRequest();
            if (input.Kind == "Agency")
            {
                if (!Roles.Contains(input.RoleLabel)) return BadRequest(new { isSuccess = false, message = "نوع فعالیت معتبر نیست." });
                var provider = await _db.Providers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserId && x.Approved && x.Active && !x.Deleted);
                if (provider == null) return BadRequest(new { isSuccess = false, message = "ابتدا باید درخواست ارائه‌دهندگی شما تأیید شود." });
            }
            else if (await _db.Providers.AnyAsync(x => x.UserId == UserId && x.Approved && !x.Deleted)
                || await _db.ProviderOnboardingApplications.AnyAsync(x => x.UserId == UserId && x.Kind == "Provider" && x.Status != "Rejected"))
                return Conflict(new { isSuccess = false, message = "درخواست ارائه‌دهندگی شما قبلاً ثبت شده است." });

            var now = DateTime.UtcNow;
            var item = new ProviderOnboardingApplication { UserId = UserId, Kind = input.Kind,
                RoleLabel = input.Kind == "Agency" ? input.RoleLabel : null, CreatedAt = now, UpdatedAt = now };
            _db.ProviderOnboardingApplications.Add(item);
            await _db.SaveChangesAsync();
            _db.ProviderOnboardingEvents.Add(new ProviderOnboardingEvent { ApplicationId = item.Id, ActorUserId = UserId, Status = "Draft", CreatedAt = now });
            await _db.SaveChangesAsync();
            return Ok(new { isSuccess = true, data = View(item) });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Put(long id, [FromBody] ApplicationInput input)
        {
            var item = await _db.ProviderOnboardingApplications.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId);
            if (item == null) return NotFound();
            if (item.Status != "Draft" && item.Status != "NeedsEdit") return Conflict(new { isSuccess = false, message = "این درخواست در حال بررسی است." });
            if (input == null || input.Kind != item.Kind || (item.Kind == "Agency" && input.RoleLabel != item.RoleLabel)) return BadRequest();
            if (item.Kind == "Provider" && !SensitiveDataProtector.IsConfigured)
                return StatusCode(503, new { isSuccess = false, message = "ذخیرهٔ امن اطلاعات هویتی آماده نیست." });
            if (input.DetailsJson?.Length > 10000) return BadRequest(new { isSuccess = false, message = "اطلاعات فرم بیش از حد طولانی است." });
            if (new[] { input.FirstName, input.LastName, input.City }.Any(x => x?.Length > 100)
                || input.BusinessName?.Length > 200 || input.BusinessAddress?.Length > 500
                || input.BusinessPhone?.Length > 30 || input.CompanyName?.Length > 200
                || input.CompanyNationalId?.Length > 20 || input.NationalCode?.Length > 10
                || input.BirthDate?.Length > 20)
                return BadRequest(new { isSuccess = false, message = "طول یکی از فیلدها بیش از حد مجاز است." });
            if (!string.IsNullOrWhiteSpace(input.DetailsJson))
            {
                try { using var _ = JsonDocument.Parse(input.DetailsJson); }
                catch (JsonException) { return BadRequest(new { isSuccess = false, message = "جزئیات فرم معتبر نیست." }); }
            }
            item.FirstName = input.FirstName?.Trim(); item.LastName = input.LastName?.Trim();
            item.NationalCode = NormalizeDigits(input.NationalCode)?.Trim(); item.BirthDate = NormalizeDigits(input.BirthDate)?.Trim();
            item.ApplicantType = input.ApplicantType?.Trim(); item.CompanyName = input.CompanyName?.Trim();
            item.CompanyNationalId = NormalizeDigits(input.CompanyNationalId)?.Trim(); item.City = input.City?.Trim();
            item.BusinessName = input.BusinessName?.Trim(); item.BusinessAddress = input.BusinessAddress?.Trim();
            item.BusinessPhone = NormalizeDigits(input.BusinessPhone)?.Trim(); item.DetailsJson = input.DetailsJson;
            item.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { isSuccess = true, data = View(item) });
        }

        [HttpPost("{id:long}/documents")]
        [RequestSizeLimit(3_000_000)]
        public async Task<IActionResult> Upload(long id, [FromForm] string kind, [FromForm] IFormFile file)
        {
            var item = await _db.ProviderOnboardingApplications.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId);
            if (item == null) return NotFound();
            if (item.Status != "Draft" && item.Status != "NeedsEdit") return Conflict();
            if (!DocumentKinds.Contains(kind) || file == null || file.Length == 0 || file.Length > 2_000_000)
                return BadRequest(new { isSuccess = false, message = "فایل یا نوع مدرک معتبر نیست (حداکثر ۲ مگابایت)." });
            if (!SensitiveDataProtector.IsConfigured) return StatusCode(503, new { isSuccess = false, message = "ذخیرهٔ امن مدارک آماده نیست." });
            byte[] bytes;
            await using (var stream = new MemoryStream()) { await file.CopyToAsync(stream); bytes = stream.ToArray(); }
            var jpeg = bytes.Length > 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff;
            var png = bytes.Length > 8 && bytes.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            var pdf = bytes.Length > 4 && bytes[0] == 37 && bytes[1] == 80 && bytes[2] == 68 && bytes[3] == 70;
            if (!jpeg && !png && !pdf) return BadRequest(new { isSuccess = false, message = "فقط تصویر JPG/PNG یا PDF پذیرفته می‌شود." });
            var contentType = jpeg ? "image/jpeg" : png ? "image/png" : "application/pdf";
            var document = await _db.ProviderOnboardingDocuments.AsTracking()
                .FirstOrDefaultAsync(x => x.ApplicationId == id && x.Kind == kind);
            if (document == null)
            {
                document = new ProviderOnboardingDocument { ApplicationId = id, Kind = kind };
                _db.ProviderOnboardingDocuments.Add(document);
            }
            document.FileName = Path.GetFileName(file.FileName).Length > 200 ? "document" : Path.GetFileName(file.FileName);
            document.ContentType = contentType;
            document.ProtectedContent = SensitiveDataProtector.Protect(Convert.ToBase64String(bytes));
            document.CreatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { isSuccess = true, data = new { document.Id, document.Kind, document.FileName } });
        }

        [HttpPost("{id:long}/submit")]
        public async Task<IActionResult> Submit(long id, [FromBody] SubmissionConfirmation input)
        {
            if (input?.ConfirmAccuracy != true)
                return BadRequest(new { isSuccess = false, message = "صحت اطلاعات واردشده را تأیید کنید." });
            var item = await _db.ProviderOnboardingApplications.AsTracking().Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == UserId);
            if (item == null) return NotFound();
            if (item.Status != "Draft" && item.Status != "NeedsEdit") return Conflict();
            if (item.Kind == "Provider")
            {
                if (string.IsNullOrWhiteSpace(item.FirstName) || string.IsNullOrWhiteSpace(item.LastName)
                    || !ValidNationalCode(item.NationalCode) || string.IsNullOrWhiteSpace(item.BirthDate)
                    || !Regex.IsMatch(item.BirthDate, @"^\d{4}/\d{2}/\d{2}$")
                    || (item.ApplicantType != "Natural" && item.ApplicantType != "Legal")
                    || !item.Documents.Any(x => x.Kind == "NationalFront") || !item.Documents.Any(x => x.Kind == "NationalBack"))
                    return BadRequest(new { isSuccess = false, message = "اطلاعات هویتی و دو تصویر کارت ملی را تکمیل کنید." });
                if (item.ApplicantType == "Legal" && (string.IsNullOrWhiteSpace(item.CompanyName)
                    || string.IsNullOrWhiteSpace(item.CompanyNationalId) || !item.Documents.Any(x => x.Kind == "CompanyProof")))
                    return BadRequest(new { isSuccess = false, message = "اطلاعات و مدرک شرکت را تکمیل کنید." });
            }
            else
            {
                if (!await _db.Providers.AnyAsync(x => x.UserId == UserId && x.Approved && x.Active && !x.Deleted)) return Forbid();
                if (string.IsNullOrWhiteSpace(item.BusinessName) || string.IsNullOrWhiteSpace(item.City)
                    || string.IsNullOrWhiteSpace(item.BusinessAddress) || string.IsNullOrWhiteSpace(item.BusinessPhone)
                    || string.IsNullOrWhiteSpace(item.DetailsJson) || !item.Documents.Any(x => x.Kind == "VenuePhoto"))
                    return BadRequest(new { isSuccess = false, message = "اطلاعات و تصویر محل فعالیت را تکمیل کنید." });
                if ((item.RoleLabel == "Clinic") && !item.Documents.Any(x => x.Kind == "BusinessLicense"))
                    return BadRequest(new { isSuccess = false, message = "مدرک فعالیت کلینیک را بارگذاری کنید." });
                try
                {
                    using var json = JsonDocument.Parse(item.DetailsJson);
                    if (json.RootElement.ValueKind != JsonValueKind.Object ||
                        RequiredRoleFields[item.RoleLabel].Any(key => !json.RootElement.TryGetProperty(key, out var value)
                            || value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString())))
                        return BadRequest(new { isSuccess = false, message = "فیلدهای اختصاصی فعالیت را تکمیل کنید." });
                }
                catch (JsonException) { return BadRequest(new { isSuccess = false, message = "جزئیات فرم معتبر نیست." }); }
            }
            item.Status = "PendingReview"; item.ReviewerNote = null; item.SubmittedAt = DateTime.UtcNow;
            item.ConfirmedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
            _db.ProviderOnboardingEvents.Add(new ProviderOnboardingEvent { ApplicationId = item.Id, ActorUserId = UserId,
                Status = "PendingReview", CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();
            return Ok(new { isSuccess = true, data = View(item) });
        }

        private static bool ValidNationalCode(string code)
        {
            if (code?.Length != 10 || !code.All(char.IsDigit) || code.Distinct().Count() == 1) return false;
            var sum = Enumerable.Range(0, 9).Sum(i => (code[i] - '0') * (10 - i));
            var rem = sum % 11;
            return (code[9] - '0') == (rem < 2 ? rem : 11 - rem);
        }

        private static string NormalizeDigits(string value) => value?.Replace('۰','0').Replace('۱','1').Replace('۲','2').Replace('۳','3').Replace('۴','4').Replace('۵','5').Replace('۶','6').Replace('۷','7').Replace('۸','8').Replace('۹','9').Replace('٠','0').Replace('١','1').Replace('٢','2').Replace('٣','3').Replace('٤','4').Replace('٥','5').Replace('٦','6').Replace('٧','7').Replace('٨','8').Replace('٩','9');

        private static object View(ProviderOnboardingApplication x) => new { x.Id, x.Kind, x.RoleLabel, x.Status,
            x.FirstName, x.LastName, x.NationalCode, x.BirthDate, x.ApplicantType, x.CompanyName,
            x.CompanyNationalId, x.City, x.BusinessName, x.BusinessAddress, x.BusinessPhone, x.DetailsJson,
            x.ReviewerNote, x.CreatedAt, x.SubmittedAt,
            documents = x.Documents?.Select(d => new { d.Id, d.Kind, d.FileName }),
            events = x.Events?.OrderByDescending(e => e.CreatedAt).Select(e => new { e.Status, e.Note, e.CreatedAt }) };
    }

    public class ApplicationInput
    {
        public string Kind { get; set; }
        public string RoleLabel { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
        public string BirthDate { get; set; }
        public string ApplicantType { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNationalId { get; set; }
        public string City { get; set; }
        public string BusinessName { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessPhone { get; set; }
        public string DetailsJson { get; set; }
    }

    public class SubmissionConfirmation
    {
        public bool ConfirmAccuracy { get; set; }
    }
}
