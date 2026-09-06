using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Services.Accounting.OtpVerifySrv.Dto;
using Application.Services.Accounting.OtpVerifySrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Accounting.OtpVerifySrv
{
    public class OtpVerifyService : IOtpVerifyService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IRegixHelper RegixHelper;

        public OtpVerifyService(IDataBaseContext _context, IMapper mapper, IMessageSenderService messageSenderService, IRegixHelper RegixHelper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._messageSenderService = messageSenderService;
            this.RegixHelper = RegixHelper;

        }
        public async Task<BaseResultDto<OtpVerifyVDto>> InsertAsyncDto(OtpVerifyVDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<OtpVerifyVDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var errors = new List<Tuple<string, string>>();
                    if (string.IsNullOrEmpty(dto.Mobile) && string.IsNullOrEmpty(dto.Email))
                    {
                        return new BaseResultDto<OtpVerifyVDto>(false, Resource.Notification.PleaseEnterEitherMobileOrEmail, dto);
                    }
                    dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
                    dto.Email = await dto.Email.ToEnglishDigitsAsync();
                    if (!string.IsNullOrEmpty(dto.Mobile) && !await RegixHelper.IsMobileAsync(dto.Mobile))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile)));
                    }
                    if (!string.IsNullOrEmpty(dto.Email) && !await RegixHelper.IsEmailAsync(dto.Email))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email)));
                    }

                    if (errors.Any())
                    {
                        return new BaseResultDto<OtpVerifyVDto>(isSuccess: false, messages: errors, dto);
                    }
                    var item = await _context.OtpVerifies.FirstOrDefaultAsync(s => (string.IsNullOrEmpty(dto.Mobile) == false && s.Mobile == dto.Mobile) || (string.IsNullOrEmpty(dto.Email) == false && s.Email == dto.Email));
                    var newCode = GenerateHelper.RandomDigit();

                    if (item != null)
                    {
                        // بعد از ۳ بار درخواست کد (TryCount از ۰ شروع می‌شه، پس ۲ یعنی این چهارمین درخواسته)
                        // بدون ورود موفق، تا ۱۰ دقیقه اجازه‌ی درخواست کد جدید نداره - جلوگیری از تموم شدن
                        // شارژ پیامک با درخواست‌های پشت‌سرهم. زمان همون ۱۰ دقیقه‌ی از قبل موجوده.
                        if (item.TryCount > 1 && item.UpdateDate.AddMinutes(10) > DateTime.Now)
                        {
                            var timeSpan = (item.UpdateDate.AddMinutes(10) - DateTime.Now).Minutes;
                            if (timeSpan < 1) { timeSpan = 1; }
                            errors.Add(new Tuple<string, string>(string.Format(Resource.Pattern.TooManyOtpRequests, timeSpan), nameof(dto.Code)));
                            return new BaseResultDto<OtpVerifyVDto>(isSuccess: false, messages: errors, dto);
                        }
                        else if (item.UpdateDate.AddMinutes(10) < DateTime.Now)
                        {
                            item.TryCount = 0;
                        }
                        item.TryCount++;
                        item.UpdateDate = DateTime.Now;
                        item.Code = newCode;
                        item.Verify = null;
                        _context.OtpVerifies.Update(item);
                        _context.SaveChanges();

                    }
                    else
                    {
                        item = new OtpVerify();
                        item.Mobile = dto.Mobile;
                        item.Email = dto.Email;
                        item.Code = newCode;
                        item.TryCount = 0;
                        item.UpdateDate = DateTime.Now;
                        item.CreateDate = DateTime.Now;
                        await _context.OtpVerifies.AddAsync(item);
                        await _context.SaveChangesAsync();
                    }
                    //dto.Code = newCode;
                    await _messageSenderService.SendMessageAsync(messageType: dto.Type, mobileReceptor: dto.Mobile, emailReceptor: dto.Email, token1: newCode);
                    return new BaseResultDto<OtpVerifyVDto>(isSuccess: true, dto);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto<OtpVerifyVDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
        public async Task<BaseResultDto> CheckVerify(OtpVerifyVDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<OtpVerifyVDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var errors = new List<Tuple<string, string>>();

                    dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
                    dto.Email = await dto.Email.ToEnglishDigitsAsync();
                    dto.Code = await dto.Code.ToEnglishDigitsAsync();

                    if (!string.IsNullOrEmpty(dto.Mobile) && !await RegixHelper.IsMobileAsync(dto.Mobile))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile)));
                    }
                    if (!string.IsNullOrEmpty(dto.Email) && !await RegixHelper.IsEmailAsync(dto.Email))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email)));
                    }
                    if (string.IsNullOrEmpty(dto.Code))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheCode, nameof(dto.Code)));
                    }

                    if (errors.Any())
                    {
                        return new BaseResultDto(isSuccess: false, messages: errors);
                    }
                    var item = await _context.OtpVerifies.FirstOrDefaultAsync(s => (string.IsNullOrEmpty(dto.Mobile) == false && s.Mobile == dto.Mobile) || (string.IsNullOrEmpty(dto.Email) == false && s.Email == dto.Email));
                    if (item == null)
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.Unsuccess, nameof(dto.Mobile)));
                        return new BaseResultDto(isSuccess: false, messages: errors);

                    }
                    else
                    {
                        if (item.TryCount > 3 && item.UpdateDate.AddMinutes(10) > DateTime.Now)
                        {
                            var timeSpan = (item.UpdateDate.AddMinutes(10) - DateTime.Now).Minutes;
                            if (timeSpan < 1) { timeSpan = 1; }
                            errors.Add(new Tuple<string, string>(string.Format(Resource.Pattern.WrongVrifyCodeCount, timeSpan), nameof(dto.Mobile)));
                            return new BaseResultDto(isSuccess: false, messages: errors);
                        }
                        if (item.Code != dto.Code || item.UpdateDate < DateTime.Now.AddMinutes(-3))
                        {
                            item.Code = GenerateHelper.RandomDigit();
                            item.UpdateDate = DateTime.Now;
                            item.Verify = false;
                            item.TryCount++;
                            _context.OtpVerifies.Update(item);
                            _context.SaveChanges();
                            errors.Add(new Tuple<string, string>(Resource.Notification.TheCodeIsWrong, nameof(dto.Code)));
                            return new BaseResultDto(isSuccess: false, messages: errors);
                        }
                        item.UpdateDate = DateTime.Now;
                        item.Verify = true;
                        item.TryCount = 0;
                        _context.OtpVerifies.Update(item);
                        _context.SaveChanges();
                        var result = new OtpVerifyResultVDto();
                        result.Mobile = dto.Mobile;
                        result.Code = dto.Code;
                        var user = await _context.Users.FirstOrDefaultAsync(s => s.Mobile == dto.Mobile && s.Deleted == false);
                        if (user != null)
                        {
                            result.HasAccount = true;
                            if (user.TwoFactorEnabled)
                            {
                                result.TwoFactorEnabled = true;
                            }
                        }
                        return new BaseResultDto<OtpVerifyResultVDto>(isSuccess: true, result);
                    }

                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public async Task<BaseResultDto> IsVerified(OtpVerifyVDto dto)
        {
            var errors = new List<Tuple<string, string>>();
            dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
            dto.Email = await dto.Email.ToEnglishDigitsAsync();
            dto.Code = await dto.Code.ToEnglishDigitsAsync();

            if (!string.IsNullOrEmpty(dto.Mobile) && !await RegixHelper.IsMobileAsync(dto.Mobile))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile)));
            }
            if (!string.IsNullOrEmpty(dto.Email) && !await RegixHelper.IsEmailAsync(dto.Email))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email)));
            }
            if (string.IsNullOrEmpty(dto.Code))
            {
                errors.Add(new Tuple<string, string>(Resource.Notification.PleaseEnterTheCode, nameof(dto.Code)));
            }
            if (errors.Any())
            {
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            var item = await _context.OtpVerifies.FirstOrDefaultAsync(s => (string.IsNullOrEmpty(dto.Mobile) == false && s.Mobile == dto.Mobile) || (string.IsNullOrEmpty(dto.Email) == false && s.Email == dto.Email));
            var isVerified = false;
            if (item != null && item.Code == dto.Code && item.Verify == true && item.TryCount < 4 && item.UpdateDate.AddMinutes(5) > DateTime.Now)
            {
                isVerified = true;
            }
            else if (item != null && item.TryCount > 3 && item.UpdateDate.AddMinutes(10) > DateTime.Now)
            {
                var timeSpan = (item.UpdateDate.AddMinutes(10) - DateTime.Now).Minutes;
                if (timeSpan < 1) { timeSpan = 1; }
                errors.Add(new Tuple<string, string>(string.Format(Resource.Pattern.WrongVrifyCodeCount, timeSpan), nameof(dto.Mobile)));
                return new BaseResultDto(isSuccess: false, messages: errors);
            }
            else if (item != null)
            {
                item.UpdateDate = DateTime.Now;
                item.TryCount++;
                _context.OtpVerifies.Update(item);
                _context.SaveChanges();
            }
            return new BaseResultDto(isSuccess: isVerified);
        }
    }
}
