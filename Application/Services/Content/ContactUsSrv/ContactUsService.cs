using Application.Common.Dto.Result;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Services.Content.ContactUsGroupSrv;
using Application.Services.Content.ContactUsItemSrv.Dto;
using Application.Services.Content.ContactUsSrv.Dto;
using Application.Services.Content.ContactUsSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.ContactUsSrv
{
    public class ContactUsService : IContactUsService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ILogger<ContactUsService> logger;
        private readonly IMessageSenderService messageSender;
        private readonly IAdminSettingHelper adminSettingHelper;

        public ContactUsService(
            IDataBaseContext _context,
            IMapper mapper,
            ILogger<ContactUsService> logger,
            IMessageSenderService messageSender,
            IAdminSettingHelper adminSettingHelper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.logger = logger;
            this.messageSender = messageSender;
            this.adminSettingHelper = adminSettingHelper;
        }

        public async Task<BaseResultDto<ContactUsVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ContactUses
                .Include(s => s.ContactUsGroup)
                .Include(s => s.ContactUsItems)
                .Include(s => s.File)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            return new BaseResultDto<ContactUsVDto>(item != null, mapper.Map<ContactUsVDto>(item));
        }

        public async Task<BaseResultDto<ContactUsDto>> FindAsyncDto(long id)
        {
            var item = await _context.ContactUses
                .Include(s => s.ContactUsItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            return new BaseResultDto<ContactUsDto>(item != null, mapper.Map<ContactUsDto>(item));
        }

        public ContactUsSearchDto Search(ContactUsInputDto baseSearchDto)
        {
            var model = _context.ContactUses
                .Include(s => s.ContactUsGroup)
                .Include(s => s.ContactUsItems)
                .Include(s => s.File)
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s =>
                    s.FullName.Contains(baseSearchDto.Q) ||
                    s.Mobile.Contains(baseSearchDto.Q) ||
                    s.Title.Contains(baseSearchDto.Q));
            }

            if (baseSearchDto.Status.HasValue)
            {
                model = model.Where(s => s.Status == baseSearchDto.Status);
            }

            if (baseSearchDto.ContactUsGroupId.HasValue)
            {
                model = model.Where(s => s.ContactUsGroupId == baseSearchDto.ContactUsGroupId);
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.ContactUsGroupLabel))
            {
                model = model.Where(s => s.ContactUsGroup.Label == baseSearchDto.ContactUsGroupLabel);
            }

            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Old:
                    model = model.OrderBy(s => s.Id);
                    break;
                default:
                    model = model.OrderByDescending(s => s.Id);
                    break;
            }

            return new ContactUsSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<BaseResultDto> InsertAsyncDto(ContactUsDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return new BaseResultDto(false, Resource.Notification.Unsuccess);
                }

                dto.ContactUsItems ??= new List<ContactUsItemDto>();
                dto.FullName = await SanitizeTextHelper.ToSanitizeAsync(dto.FullName?.Trim());
                dto.Title = await SanitizeTextHelper.ToSanitizeAsync(dto.Title?.Trim());
                dto.Body = await SanitizeTextHelper.ToSanitizeAsync(dto.Body?.Trim());
                dto.Email = dto.Email?.Trim();
                dto.Mobile = await NormalizeMobileAsync(dto.Mobile);

                var modelChecker = ModelHelper<ContactUsDto>.ModelErrors(dto);
                if (!modelChecker.IsSuccess)
                {
                    return modelChecker;
                }

                if (dto.ContactUsItems.Count > 20 || dto.ContactUsItems.Any(contactItem =>
                        (contactItem.Title?.Length ?? 0) > 100 ||
                        (contactItem.Value?.Length ?? 0) > 1000))
                {
                    return new BaseResultDto<ContactUsDto>(
                        false,
                        Resource.Notification.ContactUsFormItemsExceedLimit,
                        dto);
                }

                var group = await _context.ContactUsGroups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == dto.ContactUsGroupId && s.Active);

                if (group == null)
                {
                    return new BaseResultDto<ContactUsDto>(false, Resource.Notification.NothingFound, dto);
                }

                foreach (var contactItem in dto.ContactUsItems)
                {
                    contactItem.Id = 0;
                    contactItem.ContactUsId = 0;
                    contactItem.Title = await SanitizeTextHelper.ToSanitizeAsync(contactItem.Title?.Trim());
                    contactItem.Value = await (contactItem.Value ?? string.Empty).Trim().ToEnglishDigitsAsync();
                    contactItem.Value = await SanitizeTextHelper.ToSanitizeAsync(contactItem.Value);
                }

                var formError = ValidateFormItems(group.Label, dto.ContactUsItems);
                if (!string.IsNullOrWhiteSpace(formError))
                {
                    return new BaseResultDto<ContactUsDto>(false, formError, dto);
                }

                dto.ContactUsItems = dto.ContactUsItems
                    .Where(s => !string.IsNullOrWhiteSpace(s.Title) && !string.IsNullOrWhiteSpace(s.Value))
                    .ToList();

                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                var duplicateThreshold = DateTime.Now.AddMinutes(-2);
                var duplicateExists = await _context.ContactUses
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.CreateDate >= duplicateThreshold &&
                        s.Mobile == dto.Mobile &&
                        s.ContactUsGroupId == dto.ContactUsGroupId &&
                        s.Title == dto.Title &&
                        s.Body == dto.Body);

                if (duplicateExists)
                {
                    return new BaseResultDto<ContactUsDto>(
                        false,
                        Resource.Notification.ContactUsDuplicateRecentSubmission,
                        dto);
                }

                if (dto.FileId.HasValue)
                {
                    var minimumUploadDate = DateTime.Now.AddHours(-2);
                    var fileIsValid = await _context.Files
                        .AsNoTracking()
                        .AnyAsync(s =>
                            s.Id == dto.FileId.Value &&
                            !s.Protected &&
                            s.Size > 0 &&
                            s.Size <= 10 * 1024 * 1024 &&
                            s.CreateDate >= minimumUploadDate &&
                            (s.Extension == ".jpg" ||
                             s.Extension == ".jpeg" ||
                             s.Extension == ".png" ||
                             s.Extension == ".webp" ||
                             s.Extension == ".pdf") &&
                            (s.ContentType == "image/jpeg" ||
                             s.ContentType == "image/png" ||
                             s.ContentType == "image/webp" ||
                             s.ContentType == "application/pdf") &&
                            !_context.ContactUses.Any(contact => contact.FileId == s.Id));

                    if (!fileIsValid)
                    {
                        return new BaseResultDto<ContactUsDto>(
                            false,
                            Resource.Notification.ContactUsAttachmentInvalidOrExpired,
                            dto);
                    }
                }

                dto.Id = 0;
                var item = mapper.Map<ContactUs>(dto);
                item.Answer = null;
                item.Status = false;
                item.CreateDate = DateTime.Now;

                await _context.ContactUses.AddAsync(item);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var departmentName = string.IsNullOrWhiteSpace(group.Name) ? group.Label : group.Name;
                var adminSmsUserName = await GetUserFullNameAsync(dto.UserId, dto.FullName);
                await SendRegistrationSmsAsync(dto.FullName, adminSmsUserName, departmentName, dto.Mobile);

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Contact-us submission failed for group {ContactUsGroupId}.",
                    dto?.ContactUsGroupId);
                return new BaseResultDto<ContactUsDto>(false, Resource.Notification.Unsuccess, dto);
            }
        }

        public BaseResultDto Update(ContactUsDto dto)
        {
            var item = _context.ContactUses.FirstOrDefault(s => s.Id == dto.Id);
            if (item == null)
            {
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            }

            item.Answer = dto.Answer;
            item.Status = dto.Status;
            _context.ContactUses.Update(item);
            _context.SaveChanges();

            return new BaseResultDto(true);
        }

        private static async Task<string> NormalizeMobileAsync(string mobile)
        {
            var normalized = await (mobile ?? string.Empty).Trim().ToEnglishDigitsAsync();
            normalized = normalized
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");

            if (normalized.StartsWith("+98"))
            {
                normalized = "0" + normalized.Substring(3);
            }

            if (normalized.StartsWith("98") && normalized.Length == 12)
            {
                normalized = "0" + normalized.Substring(2);
            }

            return normalized;
        }

        private async Task<string> GetUserFullNameAsync(long? userId, string fallbackName)
        {
            if (!userId.HasValue)
            {
                return fallbackName;
            }

            var userName = await _context.Users
                .AsNoTracking()
                .Where(s => s.Id == userId.Value)
                .Select(s => new { s.FirstName, s.LastName })
                .FirstOrDefaultAsync();

            if (userName == null)
            {
                return fallbackName;
            }

            var fullName = $"{userName.FirstName} {userName.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName) ? fallbackName : fullName;
        }

        private async Task SendRegistrationSmsAsync(
            string userMessageName,
            string adminMessageName,
            string departmentName,
            string userMobile)
        {
            await TrySendRegistrationSmsAsync(
                MessageTypeEnum.UserContactUs,
                userMobile,
                userMessageName,
                departmentName);

            string adminMobiles = null;
            try
            {
                adminMobiles = adminSettingHelper.BaseAdminSetting?.AdminMobiles;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reading admin mobiles for contact-us SMS failed.");
            }

            await TrySendRegistrationSmsAsync(
                MessageTypeEnum.AdminContactUs,
                adminMobiles,
                adminMessageName,
                departmentName);
        }

        private async Task TrySendRegistrationSmsAsync(
            MessageTypeEnum messageType,
            string receptor,
            string userName,
            string departmentName)
        {
            if (string.IsNullOrWhiteSpace(receptor))
            {
                return;
            }

            try
            {
                await messageSender.SendMessageAsync(
                    messageType: messageType,
                    mobileReceptor: receptor,
                    emailReceptor: null,
                    token1: userName,
                    token2: departmentName);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Sending contact-us SMS of type {MessageType} failed.",
                    messageType);
            }
        }

        private static string ValidateFormItems(string groupLabel, List<ContactUsItemDto> items)
        {
            if (!ContactUsGroupFormSchema.IsManaged(groupLabel))
            {
                return null;
            }

            var fields = ContactUsGroupFormSchema.GetFields(groupLabel);
            var duplicateKey = items
                .Where(s => !string.IsNullOrWhiteSpace(s.Title))
                .GroupBy(s => s.Title.Trim(), StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(s => s.Count() > 1);

            if (duplicateKey != null)
            {
                return Resource.Notification.ContactUsFormFieldDuplicate;
            }

            var values = items
                .Where(s => !string.IsNullOrWhiteSpace(s.Title))
                .ToDictionary(s => s.Title.Trim(), s => s.Value?.Trim(), StringComparer.OrdinalIgnoreCase);

            if (values.Keys.Any(key => fields.All(field =>
                    !field.Key.Equals(key, StringComparison.OrdinalIgnoreCase))))
            {
                return Resource.Notification.ContactUsFormFieldNotValidForDepartment;
            }

            foreach (var field in fields)
            {
                values.TryGetValue(field.Key, out var value);

                if (field.Required && string.IsNullOrWhiteSpace(value))
                {
                    return string.Format(Resource.Notification.ContactUsFormFieldRequired, field.Label);
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (field.MaxLength.HasValue && value.Length > field.MaxLength.Value)
                {
                    return string.Format(Resource.Notification.ContactUsFormFieldValueExceedsLimit, field.Label);
                }

                if (field.InputType == "select" && field.Options.All(option =>
                        !option.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
                {
                    return string.Format(Resource.Notification.ContactUsFormFieldSelectedValueInvalid, field.Label);
                }

                if (field.InputType == "number" &&
                    (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) ||
                     number < (field.MinValue ?? 0)))
                {
                    return string.Format(Resource.Notification.ContactUsFormFieldMustBeValidNumber, field.Label);
                }

                if (field.InputType == "url" &&
                    (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                     (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
                {
                    return string.Format(Resource.Notification.ContactUsFormFieldInvalidUrl, field.Label);
                }
            }

            return null;
        }
    }
}
