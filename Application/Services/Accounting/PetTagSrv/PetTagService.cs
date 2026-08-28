using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Accounting.PetTagSrv.Dto;
using Application.Services.Accounting.PetTagSrv.Iface;
using AutoMapper;
using ClosedXML.Excel;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetTagSrv
{
    public class PetTagService : CommonSrv<PetTag, PetTagDto>, IPetTagService
    {
        private const int MaxGenerateCount = 2000;

        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IConfiguration _configuration;

        public PetTagService(IDataBaseContext _context, IMapper mapper, IConfiguration configuration) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._configuration = configuration;
        }

        private string BuildUrl(string code)
        {
            var baseUrl = _configuration["Urls:BaseUrl"];
            var path = _configuration["Urls:PetTagUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(path))
                return null;

            return $"{baseUrl.TrimEnd('/')}{path.Replace("id", code)}";
        }

        public async Task<BaseResultDto<PetTagGenerateResultDto>> GenerateAsync(int count)
        {
            if (count < 1 || count > MaxGenerateCount)
                return new BaseResultDto<PetTagGenerateResultDto>(false, Resource.Notification.PetTagGenerateCountInvalid, null);

            var codes = new HashSet<string>();
            while (codes.Count < count)
                codes.Add(PetTagCodeGenerator.Create());

            var existing = await _context.PetTags
                .Where(item => codes.Contains(item.Code))
                .Select(item => item.Code)
                .ToListAsync();

            while (existing.Count > 0)
            {
                foreach (var duplicate in existing)
                    codes.Remove(duplicate);

                while (codes.Count < count)
                    codes.Add(PetTagCodeGenerator.Create());

                existing = await _context.PetTags
                    .Where(item => codes.Contains(item.Code))
                    .Select(item => item.Code)
                    .ToListAsync();
            }

            var now = DateTime.Now;
            var tags = codes.Select(code => new PetTag
            {
                Code = code,
                Active = true,
                Deleted = false,
                CreateDate = now
            }).ToList();

            await _context.PetTags.AddRangeAsync(tags);
            await _context.SaveChangesAsync();

            var result = new PetTagGenerateResultDto
            {
                GeneratedCount = tags.Count,
                Items = tags.Select(item => new PetTagGeneratedItemDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    Url = BuildUrl(item.Code)
                }).ToList()
            };

            return new BaseResultDto<PetTagGenerateResultDto>(true, result);
        }

        public BaseSearchDto<PetTagVDto> Search(PetTagInputDto dto)
        {
            var model = _context.PetTags
                .Include(item => item.UserPet)
                    .ThenInclude(item => item.User)
                .Where(item => item.Deleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Q))
                model = model.Where(item => item.Code.Contains(dto.Q));

            if (dto.Claimed.HasValue)
                model = dto.Claimed.Value
                    ? model.Where(item => item.UserPetId != null)
                    : model.Where(item => item.UserPetId == null);

            if (dto.Active.HasValue)
                model = model.Where(item => item.Active == dto.Active.Value);

            model = model.OrderByDescending(item => item.Id);

            var result = new BaseSearchDto<PetTag, PetTagVDto>(dto, model, mapper);
            foreach (var item in result.List)
                item.Url = BuildUrl(item.Code);

            return result;
        }

        public async Task<BaseResultDto<PetTagPublicStatusDto>> GetPublicStatusAsync(string code)
        {
            var normalizedCode = code?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedCode))
                return new BaseResultDto<PetTagPublicStatusDto>(false, Resource.Notification.PetTagCodeNotValid, null);

            var tag = await _context.PetTags
                .Include(item => item.UserPet)
                    .ThenInclude(item => item.Pet)
                .Include(item => item.UserPet)
                    .ThenInclude(item => item.PetBreed)
                .Include(item => item.UserPet)
                    .ThenInclude(item => item.Picture)
                .Include(item => item.UserPet)
                    .ThenInclude(item => item.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Code == normalizedCode && !item.Deleted && item.Active);

            if (tag == null)
                return new BaseResultDto<PetTagPublicStatusDto>(false, Resource.Notification.PetTagCodeNotValid, null);

            if (tag.UserPetId == null || tag.UserPet == null)
                return new BaseResultDto<PetTagPublicStatusDto>(true, new PetTagPublicStatusDto { Claimed = false });

            var userPet = tag.UserPet;
            var publicPet = new PetTagPublicPetVDto
            {
                Name = userPet.Name,
                BreedName = userPet.PetBreed?.Name ?? userPet.Pet?.Name,
                IsMale = userPet.IsMale,
                PictureUrl = userPet.Picture?.Url,
                OwnerFirstName = userPet.User?.FirstName,
                OwnerLastName = userPet.User?.LastName,
                OwnerMobile = userPet.User?.Mobile
            };

            return new BaseResultDto<PetTagPublicStatusDto>(true, new PetTagPublicStatusDto { Claimed = true, Pet = publicPet });
        }

        public async Task<BaseResultDto> ClaimAsync(string code, long userPetId, long currentUserId)
        {
            var normalizedCode = code?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedCode))
                return new BaseResultDto(false, Resource.Notification.PetTagCodeNotValid);

            var tag = await _context.PetTags.AsTracking()
                .FirstOrDefaultAsync(item => item.Code == normalizedCode && !item.Deleted && item.Active);

            if (tag == null)
                return new BaseResultDto(false, Resource.Notification.PetTagCodeNotValid);

            if (tag.UserPetId != null)
                return new BaseResultDto(false, Resource.Notification.PetTagAlreadyClaimed);

            var userPet = await _context.UserPets
                .FirstOrDefaultAsync(item => item.Id == userPetId && !item.Deleted);

            if (userPet == null || userPet.UserId != currentUserId)
                return new BaseResultDto(false, Resource.Notification.AccessDenied);

            // هر پت فقط یک‌بار و برای همیشه می‌تواند به یک کد قلاده متصل شود؛
            // این از اتصال دوباره/جابه‌جایی کد بین پت‌ها جلوگیری می‌کند.
            var petAlreadyTagged = await _context.PetTags
                .AnyAsync(item => item.UserPetId == userPetId && !item.Deleted);

            if (petAlreadyTagged)
                return new BaseResultDto(false, Resource.Notification.PetTagPetAlreadyHasTag);

            tag.UserPetId = userPetId;
            tag.ClaimedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto<List<PetTagMineItemDto>>> GetMineAsync(long currentUserId)
        {
            var items = await _context.PetTags
                .Where(item => !item.Deleted && item.UserPet != null && item.UserPet.UserId == currentUserId)
                .Select(item => new PetTagMineItemDto
                {
                    UserPetId = item.UserPetId.Value,
                    Code = item.Code,
                    ClaimedDate = item.ClaimedDate
                })
                .ToListAsync();

            return new BaseResultDto<List<PetTagMineItemDto>>(true, items);
        }

        public async Task<MemoryStream> GetExcelAsync(PetTagExportFilterDto filter)
        {
            var model = _context.PetTags
                .Include(item => item.UserPet)
                    .ThenInclude(item => item.User)
                .Where(item => !item.Deleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Q))
                model = model.Where(item => item.Code.Contains(filter.Q));

            if (filter.Claimed.HasValue)
                model = filter.Claimed.Value
                    ? model.Where(item => item.UserPetId != null)
                    : model.Where(item => item.UserPetId == null);

            if (filter.Active.HasValue)
                model = model.Where(item => item.Active == filter.Active.Value);

            var items = await model
                .OrderByDescending(item => item.Id)
                .Select(item => new
                {
                    item.Id,
                    item.Code,
                    item.CreateDate,
                    item.ClaimedDate,
                    Claimed = item.UserPetId != null,
                    PetName = item.UserPet != null ? item.UserPet.Name : null,
                    OwnerFirstName = item.UserPet != null && item.UserPet.User != null ? item.UserPet.User.FirstName : null,
                    OwnerLastName = item.UserPet != null && item.UserPet.User != null ? item.UserPet.User.LastName : null,
                    OwnerMobile = item.UserPet != null && item.UserPet.User != null ? item.UserPet.User.Mobile : null
                })
                .ToListAsync();

            var baseUrl = _configuration["Urls:BaseUrl"];
            var path = _configuration["Urls:PetTagUrl"];

            var workbook = new XLWorkbook { RightToLeft = true };
            var worksheet = workbook.Worksheets.Add("PetTags");
            worksheet.RightToLeft = true;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.Columns().Width = 30;
            worksheet.Column(1).Width = 12;

            string[] headers = { "QR", "ردیف", "کد", "لینک", "وضعیت", "نام پت", "نام مالک", "موبایل مالک", "تاریخ ساخت", "تاریخ اتصال" };
            for (var i = 0; i < headers.Length; i++)
                worksheet.Cell(1, i + 1).Value = headers[i];

            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DDDDDD");
            headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            var qrGenerator = new QRCoder.QRCodeGenerator();

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var row = i + 2;
                var ownerFullName = string.IsNullOrWhiteSpace(item.OwnerFirstName) && string.IsNullOrWhiteSpace(item.OwnerLastName)
                    ? "-"
                    : $"{item.OwnerFirstName} {item.OwnerLastName}".Trim();
                var url = !string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(path)
                    ? $"{baseUrl.TrimEnd('/')}{path.Replace("id", item.Code)}"
                    : null;

                worksheet.Row(row).Height = 60;

                try
                {
                    using var qrData = qrGenerator.CreateQrCode(url ?? item.Code, QRCoder.QRCodeGenerator.ECCLevel.Q);
                    var qrPng = new QRCoder.PngByteQRCode(qrData);
                    var qrBytes = qrPng.GetGraphic(8);
                    using var qrStream = new MemoryStream(qrBytes);
                    worksheet.AddPicture(qrStream)
                        .MoveTo(worksheet.Cell(row, 1), 4, 4)
                        .WithSize(72, 72);
                }
                catch
                {
                    // اگر ساخت QR برای یک ردیف شکست بخورد، بقیه‌ی اطلاعات همچنان توی ردیف قرار می‌گیره
                }

                worksheet.Cell(row, 2).Value = i + 1;
                worksheet.Cell(row, 3).Value = item.Code;
                worksheet.Cell(row, 4).Value = url ?? "-";
                worksheet.Cell(row, 5).Value = item.Claimed ? "متصل‌شده" : "متصل‌نشده";
                worksheet.Cell(row, 6).Value = item.PetName ?? "-";
                worksheet.Cell(row, 7).Value = ownerFullName;
                worksheet.Cell(row, 8).Value = item.OwnerMobile ?? "-";
                worksheet.Cell(row, 9).Value = item.CreateDate.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 10).Value = item.ClaimedDate.HasValue ? item.ClaimedDate.Value.ToString("yyyy-MM-dd HH:mm") : "-";

                var rowRange = worksheet.Range(row, 2, row, headers.Length);
                rowRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                rowRange.Style.Fill.BackgroundColor = i % 2 == 0 ? XLColor.White : XLColor.FromHtml("#F0F0F0");
            }

            worksheet.Columns(2, headers.Length).AdjustToContents();
            worksheet.SheetView.FreezeRows(1);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
