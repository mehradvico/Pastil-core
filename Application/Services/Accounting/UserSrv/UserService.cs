using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Helpers.Iface;
using Application.Common.Service;
using Application.Common.Security.Iface;
using Application.Services.Accounting.OtpVerifySrv.Dto;
using Application.Services.Accounting.OtpVerifySrv.Iface;
using Application.Services.Accounting.UserSrv.Dto;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Accounting.UserTokenSrv.Dto;
using Application.Services.Accounting.UserTokenSrv.Iface;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.Dto;
using Application.Services.PastilClubSrvs.PointEventSrv.Iface;
using Application.Services.Setting.BaseDetailSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.UserSrv
{
    public class UserService : CommonSrv<User, UserDto>, IUserService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IUserTokenService userTokenSevice;
        private readonly IConfiguration configuration;
        private readonly IOtpVerifyService otpVerifyService;
        private readonly IRegixHelper RegixHelper;
        private readonly IBaseDetailService _baseDetailService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INoticeService _noticeService;
        private readonly IUserPasswordService _passwordService;
        private readonly IClubPointIntegrationService _clubPointIntegrationService;

        public UserService(
            IDataBaseContext _context,
            IPushNotificationService pushNotificationService,
            IUserTokenService userTokenSevice,
            IOtpVerifyService otpVerifyService,
            IMapper mapper,
            IConfiguration configuration,
            IRegixHelper RegixHelper,
            IBaseDetailService baseDetailService,
            IMessageSenderService messageSenderService,
            INoticeService noticeService,
            IUserPasswordService passwordService,
            IClubPointIntegrationService clubPointIntegrationService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.userTokenSevice = userTokenSevice;
            this.configuration = configuration;
            this.otpVerifyService = otpVerifyService;
            this.RegixHelper = RegixHelper;
            this._baseDetailService = baseDetailService;
            this._messageSenderService = messageSenderService;
            this._pushNotificationService = pushNotificationService;
            this._noticeService = noticeService;
            this._passwordService = passwordService;
            this._clubPointIntegrationService = clubPointIntegrationService;
        }
        public override async Task<BaseResultDto<UserDto>> InsertAsyncDto(UserDto dto)
        {
            try
            {
                dto.Expertise = null;

                var modelCheker = ModelHelper<UserDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var errors = new List<Tuple<string, string>>();

                    dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
                    if (!await RegixHelper.IsMobileAsync(dto.Mobile))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile)));
                    }
                    dto.Email = await dto.Email.ToEnglishDigitsAsync();
                    if (!string.IsNullOrEmpty(dto.Email) && !await RegixHelper.IsEmailAsync(dto.Email))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email)));
                    }
                    if (errors.Any())
                    {
                        return new BaseResultDto<UserDto>(isSuccess: false, messages: errors, dto);
                    }
                    if (!MobileIsUnique(dto.Mobile))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsDuplicate, nameof(dto.Mobile)));
                    }
                    if (!string.IsNullOrEmpty(dto.Email) && !EmailIsUnique(dto.Email))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsDuplicate, nameof(dto.Email)));
                    }
                    dto.Password = await dto.Password.ToEnglishDigitsAsync();
                    if (!string.IsNullOrEmpty(dto.Password) && !await RegixHelper.IsPassword(dto.Password))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.ThePasswordIsWrong, nameof(dto.Password)));
                    }
                    if (errors.Any())
                    {
                        return new BaseResultDto<UserDto>(isSuccess: false, messages: errors, dto);
                    }

                    var item = mapper.Map<User>(dto);
                    item.Password = string.IsNullOrEmpty(dto.Password)
                        ? null
                        : _passwordService.HashPassword(item, dto.Password);
                    item.CreateDate = DateTime.Now;
                    item.ReferralCode = await ReferralCodeGenerator.CreateUserCodeAsync(_context);
                    item.RegistrationReferralSource = dto.RegistrationReferralSource;
                    item.UsedReferralCode = dto.UsedReferralCode;
                    item.ReferredByUserId = dto.ReferredByUserId;
                    item.ReferredByCompanionId = dto.ReferredByCompanionId;
                    item.ReferredByStoreId = dto.ReferredByStoreId;

                    await _context.Users.AddAsync(item);
                    await _context.SaveChangesAsync();
                    dto.Id = item.Id;
                    dto.Password = null;
                    await _noticeService.CreateAsync(new NoticeCreateDto
                    {
                        Label = NoticeTypeLabels.UserRegistered,
                        ActorUserId = item.Id,
                        ReferenceType = "User",
                        ReferenceId = item.Id,
                        DeduplicationKey = $"{NoticeTypeLabels.UserRegistered}:{item.Id}",
                        Metadata = new Dictionary<string, string>
                        {
                            { "userName", string.IsNullOrWhiteSpace($"{item.FirstName} {item.LastName}".Trim()) ? item.Mobile : $"{item.FirstName} {item.LastName}".Trim() },
                            { "mobile", item.Mobile }
                        }
                    });
                    return new BaseResultDto<UserDto>(isSuccess: true, dto);
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<UserDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }


        public override BaseResultDto UpdateDto(UserDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<UserDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = _context.Users.FirstOrDefault(s => s.Id == dto.Id && !s.Deleted);
                    if (item == null)
                    {
                        return new BaseResultDto<UserDto>(
                            false,
                            Resource.Notification.NothingFound,
                            dto);
                    }

                    var errors = new List<Tuple<string, string>>();
                    dto.Mobile = dto.Mobile.ToEnglishDigitsAsync().Result;

                    if (!RegixHelper.IsMobileAsync(dto.Mobile).Result)
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile)));
                    }

                    if (!string.IsNullOrEmpty(dto.Email) && !RegixHelper.IsEmailAsync(dto.Email).Result)
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email)));
                    }

                    if (errors.Any())
                    {
                        return new BaseResultDto<UserDto>(isSuccess: false, messages: errors, dto);
                    }

                    if (dto.Mobile != item.Mobile && (!MobileIsUnique(dto.Mobile)))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheMobileNumberIsDuplicate, nameof(dto.Mobile)));
                    }

                    if (!string.IsNullOrEmpty(dto.Email) &&
                        dto.Email != item.Email &&
                        !EmailIsUnique(dto.Email))
                    {
                        errors.Add(new Tuple<string, string>(Resource.Notification.TheEmailAddressIsDuplicate, nameof(dto.Email)));
                    }

                    if (errors.Any())
                    {
                        return new BaseResultDto(isSuccess: false, messages: errors);
                    }

                    var assignedExpertise = _context.CompanionUsers
                        .Where(companionUser =>
                            companionUser.UserId == item.Id &&
                            !companionUser.Deleted &&
                            companionUser.UserAccept != false &&
                            companionUser.ExpertiseId.HasValue &&
                            companionUser.Expertise.Active &&
                            !companionUser.Expertise.Deleted)
                        .OrderByDescending(companionUser => companionUser.UserAccept == true)
                        .ThenByDescending(companionUser => companionUser.Id)
                        .Select(companionUser => companionUser.Expertise.Name)
                        .FirstOrDefault();

                    var canSetExpertise = CanHaveExpertise(item.Id);
                    if (string.IsNullOrWhiteSpace(assignedExpertise) &&
                        !canSetExpertise &&
                        !string.IsNullOrWhiteSpace(dto.Expertise))
                    {
                        return new BaseResultDto<UserDto>(
                            false,
                            "عنوان شغلی فقط برای صاحب نمایندگی یا عضو فعال و تأییدشده نمایندگی قابل ثبت است.",
                            nameof(dto.Expertise),
                            dto);
                    }

                    dto.Expertise = !string.IsNullOrWhiteSpace(assignedExpertise)
                        ? canSetExpertise
                            ? assignedExpertise
                            : item.Expertise
                        : canSetExpertise
                            ? dto.Expertise?.Trim()
                            : null;

                    var passwordHash = item.Password;
                    if (!string.IsNullOrEmpty(dto.Password))
                    {
                        dto.Password = dto.Password.ToEnglishDigitsAsync().Result;
                        if (!RegixHelper.IsPassword(dto.Password).Result)
                        {
                            errors.Add(new Tuple<string, string>(Resource.Notification.ThePasswordIsWrong, nameof(dto.Password)));
                        }
                        if (errors.Any())
                        {
                            return new BaseResultDto<UserDto>(isSuccess: false, messages: errors, dto);
                        }
                        passwordHash = _passwordService.HashPassword(item, dto.Password);
                    }

                    mapper.Map(dto, item);
                    item.Password = passwordHash;
                    item.ReferralCode = item.ReferralCode;

                    _context.Users.Update(item);
                    _context.SaveChanges();
                    return new BaseResultDto(isSuccess: true);
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }


        public UserVDto GetVDto(long userId)
        {
            var user = _context.Users
                .Include(s => s.UserCurrentLocation).ThenInclude(s => s.City)
                .Include(s => s.UserCurrentLocation).ThenInclude(s => s.Neighborhood)
                .FirstOrDefault(s => s.Id == userId);
            return mapper.Map<UserVDto>(user);
        }
        bool MobileIsUnique(string mobile)
        {
            var result = GetUserByMobile(mobile);
            if (result.IsSuccess)
                return false;
            return true;
        }
        public BaseResultDto<User> GetUserByMobile(string mobile)
        {
            var user = _context.Users.FirstOrDefault(x => x.Mobile == mobile && !x.Deleted);
            if (user == null)
            {
                return new BaseResultDto<User>(false, Resource.Notification.NothingFound, null);
            }

            return new BaseResultDto<User>(true, user);
        }
        bool EmailIsUnique(string email)
        {
            var item = GetUserByEmail(email);
            if (item == null)
                return true;
            return false;
        }
        User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(x => x.Email == email);
        }
        public UserSearchDto Search(UserInputDto searchDto)
        {
            var query = _context.Users
                .Include(s => s.Role)
                .Include(s => s.Picture)
                .Include(s => s.UserCurrentLocation).ThenInclude(s => s.City)
                .Include(s => s.UserCurrentLocation).ThenInclude(s => s.Neighborhood)
                .AsQueryable();
            if (searchDto.RoleId.HasValue)
            {
                query = query.Where(s => s.RoleId == searchDto.RoleId);
            }
            else if (searchDto.RoleEnum.HasValue)
            {
                query = query.Where(s => s.Role.Label == searchDto.RoleEnum.ToString());
            }
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                query = query.Where(s => s.FirstName.Contains(searchDto.Q) || s.LastName.Contains(searchDto.Q) || s.Mobile.Contains(searchDto.Q));
            }
            if (searchDto.SortBy != Common.Enumerable.SortEnum.Default)
            {
                switch (searchDto.SortBy)
                {
                    case Common.Enumerable.SortEnum.Default:
                        {
                            query = query.OrderByDescending(s => s.Id);

                            break;
                        }
                    case Common.Enumerable.SortEnum.New:
                        {
                            query = query.OrderByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            query = query.OrderBy(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Name:
                        {
                            query = query.OrderByDescending(s => s.LastName);
                            break;
                        }

                    default:
                        break;
                }
            }

            return new UserSearchDto(searchDto, query, mapper);
        }
        public async Task<BaseResultDto> CheckUser(string token, long userId, string area, string controller, string action)
        {

            var hashed = token.Tosha256Hash();
            var userToken = await _context.UserTokens.Include(s => s.User).ThenInclude(s => s.Stores.Where(a => a.Active)).Include(s => s.User).ThenInclude(s => s.Role).ThenInclude(s => s.Permissions)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == hashed);
            if (userToken == null)
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidToken);
            if (userToken.TokenExp < DateTime.UtcNow || userToken.Deleted == true)
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.TokenExpired);
            else if (userToken.User.Deleted)
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.UserNotFount);
            else if (userToken.User.Locked)
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.TheUserAccountIsBlocked);
            else if (userToken.User.RoleId == (long)RoleEnum.Admin)
            {
                return new BaseResultDto(isSuccess: true);
            }
            else if ((!string.IsNullOrEmpty(area)) && (area.ToLower().Equals("admin")) && !userToken.User.Role.Permissions.Any(s => s.Area.ToLower().Equals(area.ToLower()) && s.Controller.ToLower().Equals(controller.ToLower()) && s.Action.ToLower().Equals(action.ToLower())))
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.YouHaveNotPermission);
            else
            {
                return new BaseResultDto(isSuccess: true);
            }
        }

        public async Task<BaseResultDto> SignIn(SignInDto user)
        {
            if (string.IsNullOrEmpty(user.Mobile))
            {
                return new BaseResultDto(false, Resource.Notification.PleaseEnterEitherMobileOrEmail);
            }
            user.Mobile = await user.Mobile.ToEnglishDigitsAsync();
            user.Password = await user.Password.ToEnglishDigitsAsync();
            user.Code = await user.Code.ToEnglishDigitsAsync();
            var item = await _context.Users
                .Include(s => s.Role)
                .ThenInclude(s => s.Permissions)
                .FirstOrDefaultAsync(x => (!string.IsNullOrEmpty(x.Mobile) && x.Mobile == user.Mobile) || (!string.IsNullOrEmpty(x.Email) && x.Email == user.Mobile));
            if (item == null)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.UserNotFound);
            }
            else if (item.Deleted)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.UserNotFound);
            }
            else if (item.Locked)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.TheUserAccountIsBlocked);
            }
            var isPanelLogin = user.IsAdmin || user.IsSiteAdmin;
            if (isPanelLogin && item.Role.Label == RoleEnum.Customer.ToString())
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);
            }
            if (user.IsSiteAdmin &&
                item.Role.Label != RoleEnum.Admin.ToString() &&
                !item.Role.Permissions.Any(permission =>
                    !permission.Deleted &&
                    permission.Area == "Admin" &&
                    permission.Controller == "SiteDashboard"))
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied);
            }

            var codeVerified = await otpVerifyService.IsVerified(new OtpVerifyVDto() { Mobile = item.Mobile, Email = item.Email, Code = user.Code });
            if (item.TwoFactorEnabled)
            {
                if (codeVerified.IsSuccess == false)
                {
                    return new BaseResultDto(isSuccess: false, val1: Resource.Notification.TheCodeIsWrong, val2: nameof(user.Code));

                }
                if (string.IsNullOrEmpty(user.Password))
                    return new BaseResultDto(isSuccess: false, val1: Resource.Notification.PleaseEnterThePassword, val2: nameof(user.Password));
                if (!await VerifyAndUpgradePasswordAsync(item, user.Password))
                {
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.UserNotFound);
                }
            }
            else
            {
                bool success = false;
                if (!string.IsNullOrEmpty(user.Code))
                {
                    if (codeVerified.IsSuccess == false)
                    {
                        return new BaseResultDto(isSuccess: false, val1: Resource.Notification.TheCodeIsWrong, val2: nameof(user.Code));
                    }
                    success = true;
                }
                if (success == false)
                {
                    if (string.IsNullOrEmpty(user.Password))
                        return new BaseResultDto(isSuccess: false, val1: Resource.Notification.PleaseEnterThePassword, val2: nameof(user.Password));
                    if (!await VerifyAndUpgradePasswordAsync(item, user.Password))
                    {
                        return new BaseResultDto(isSuccess: false, val: Resource.Notification.UserNotFound);
                    }
                }

            }

            var tokenResult = await userTokenSevice.ResetTokenAsync(
                item,
                isPanelLogin,
                isPanelLogin ? user.RememberMe : true);
            await ChangUserCartAsync(item.Id, user.CartCode);
            return tokenResult;
        }

        private async Task<bool> VerifyAndUpgradePasswordAsync(
            User user,
            string providedPassword)
        {
            var verification = _passwordService.VerifyPassword(
                user,
                user.Password,
                providedPassword);

            if (!verification.Succeeded)
            {
                return false;
            }

            if (verification.NeedsRehash)
            {
                user.Password = _passwordService.HashPassword(
                    user,
                    providedPassword);
                await _context.SaveChangesAsync();
            }

            return true;
        }
        private UserTokenDto CreateToken(long userId)
        {
            var user = _context.Users.Include(s => s.Role).FirstOrDefault(s => s.Id == userId);
            return userTokenSevice.CreateToken(user, rememberMe: true);
        }

        public async Task<BaseResultDto> SignUp(SignUpDto user)
        {
            var codeVerified = await otpVerifyService.IsVerified(new OtpVerifyVDto() { Mobile = user.Mobile, Email = user.Email, Code = user.Code });
            if (codeVerified.IsSuccess == false)
            {
                return new BaseResultDto(isSuccess: false, val1: Resource.Notification.TheCodeIsWrong, val2: nameof(user.Code));
            }
            user.Password = await user.Password.ToEnglishDigitsAsync();
            user.Mobile = await user.Mobile.ToEnglishDigitsAsync();
            user.Email = await user.Email.ToEnglishDigitsAsync();
            var referral = await ResolveRegistrationReferralAsync(user);
            if (!referral.IsSuccess)
            {
                return new BaseResultDto<SignUpDto>(false, referral.Messages, user);
            }

            var userDto = mapper.Map<UserDto>(user);

            userDto.RoleId = (long)RoleEnum.Customer;
            userDto.RegistrationReferralSource = user.ReferralSource;
            userDto.UsedReferralCode = referral.ReferralCode;
            userDto.ReferredByUserId = referral.UserId;
            userDto.ReferredByCompanionId = referral.CompanionId;
            userDto.ReferredByStoreId = referral.StoreId;

            var insertResult = await InsertAsyncDto(userDto);
            if (insertResult.IsSuccess)
            {
                if (referral.RewardRecipientUserId.HasValue)
                {
                    await _clubPointIntegrationService.RegistrationReferralCompletedAsync(
                        insertResult.Data.Id,
                        referral.RewardRecipientUserId.Value,
                        referral.CompanionId.HasValue || referral.StoreId.HasValue);
                }

                var token = CreateToken(insertResult.Data.Id);
                await ChangUserCartAsync(insertResult.Data.Id, user.CartCode);

                await _messageSenderService.SendMessageAsync(messageType: Common.Enumerable.Message.MessageTypeEnum.UserSignUp, mobileReceptor: user.Mobile, emailReceptor: user.Email, token1: user.FirstName);
                await _pushNotificationService.SendPushAsync(pushType: PushTypeEnum.PushSignUpUser, userId: insertResult.Data.Id, token1: user.FirstName);

                return new BaseResultDto<UserTokenDto>(isSuccess: true, data: token);
            }
            else
            {
                return new BaseResultDto<SignUpDto>(false, insertResult.Messages, user);
            }
        }

        private async Task<RegistrationReferralResolution> ResolveRegistrationReferralAsync(SignUpDto dto)
        {
            var result = new RegistrationReferralResolution();

            if (!Enum.IsDefined(typeof(RegistrationReferralSource), dto.ReferralSource))
            {
                result.AddError(Resource.Notification.InvalidData, nameof(dto.ReferralSource));
                return result;
            }

            var requiresReferralCode = dto.ReferralSource == RegistrationReferralSource.Clinic ||
                                       dto.ReferralSource == RegistrationReferralSource.PetShop ||
                                       dto.ReferralSource == RegistrationReferralSource.Acquaintances;

            if (!requiresReferralCode)
            {
                result.IsSuccess = true;
                return result;
            }

            var referralCode = string.IsNullOrWhiteSpace(dto.ReferralCode)
                ? null
                : (await dto.ReferralCode.Trim().ToEnglishDigitsAsync());

            if (string.IsNullOrWhiteSpace(referralCode) || referralCode.Any(character => !char.IsDigit(character)))
            {
                result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                return result;
            }

            switch (dto.ReferralSource)
            {
                case RegistrationReferralSource.Clinic:
                {
                    if (referralCode.Length != 10)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }

                    var companion = await _context.Companions
                        .AsNoTracking()
                        .Include(item => item.Owner)
                        .FirstOrDefaultAsync(item =>
                            !item.Deleted && item.Active && item.Approved &&
                            item.ReferralCode == referralCode);

                    if (companion == null)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }

                    if (HasSameRegistrationIdentity(
                            dto,
                            companion.Owner?.Mobile,
                            companion.Owner?.Email))
                    {
                        result.AddError(Resource.Notification.ReferralCodeCanNotBeYours, nameof(dto.ReferralCode));
                        return result;
                    }

                    result.CompanionId = companion.Id;
                    result.RewardRecipientUserId = companion.OwnerId;
                    break;
                }
                case RegistrationReferralSource.PetShop:
                {
                    if (referralCode.Length != 10)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }

                    var store = await _context.Stores
                        .AsNoTracking()
                        .Include(item => item.Users)
                        .FirstOrDefaultAsync(item =>
                            !item.Deleted && item.Active &&
                            item.ReferralCode == referralCode);

                    if (store == null)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }

                    if (store.Users?.Any(owner =>
                            HasSameRegistrationIdentity(dto, owner.Mobile, owner.Email)) == true)
                    {
                        result.AddError(Resource.Notification.ReferralCodeCanNotBeYours, nameof(dto.ReferralCode));
                        return result;
                    }

                    result.StoreId = store.Id;
                    result.RewardRecipientUserId = store.Users
                        .OrderBy(item => item.Id)
                        .Select(item => (long?)item.Id)
                        .FirstOrDefault();

                    if (!result.RewardRecipientUserId.HasValue)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }
                    break;
                }
                case RegistrationReferralSource.Acquaintances:
                {
                    if (referralCode.Length != 7)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }

                    var referrer = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(item =>
                            !item.Deleted && !item.Locked &&
                            item.ReferralCode == referralCode);

                    if (referrer == null)
                    {
                        result.AddError(Resource.Notification.TheReferralCodeIsWrong, nameof(dto.ReferralCode));
                        return result;
                    }

                    if (HasSameRegistrationIdentity(dto, referrer.Mobile, referrer.Email))
                    {
                        result.AddError(Resource.Notification.ReferralCodeCanNotBeYours, nameof(dto.ReferralCode));
                        return result;
                    }

                    result.UserId = referrer.Id;
                    result.RewardRecipientUserId = referrer.Id;
                    break;
                }
            }

            result.ReferralCode = referralCode;
            result.IsSuccess = true;
            return result;
        }

        private static bool HasSameRegistrationIdentity(
            SignUpDto registration,
            string candidateMobile,
            string candidateEmail)
        {
            var hasSameMobile = !string.IsNullOrWhiteSpace(registration.Mobile) &&
                                !string.IsNullOrWhiteSpace(candidateMobile) &&
                                string.Equals(
                                    registration.Mobile.Trim(),
                                    candidateMobile.Trim(),
                                    StringComparison.OrdinalIgnoreCase);
            var hasSameEmail = !string.IsNullOrWhiteSpace(registration.Email) &&
                               !string.IsNullOrWhiteSpace(candidateEmail) &&
                               string.Equals(
                                   registration.Email.Trim(),
                                   candidateEmail.Trim(),
                                   StringComparison.OrdinalIgnoreCase);

            return hasSameMobile || hasSameEmail;
        }

        private sealed class RegistrationReferralResolution
        {
            public bool IsSuccess { get; set; }
            public string ReferralCode { get; set; }
            public long? UserId { get; set; }
            public long? CompanionId { get; set; }
            public long? StoreId { get; set; }
            public long? RewardRecipientUserId { get; set; }
            public List<Tuple<string, string>> Messages { get; } = new List<Tuple<string, string>>();

            public void AddError(string message, string field)
            {
                Messages.Add(new Tuple<string, string>(message, field));
            }
        }

        public async Task<BaseResultDto> ChangePassword(ChangePasswordDto user)
        {
            user.NewPassword = await user.NewPassword.ToEnglishDigitsAsync();
            if (!await RegixHelper.IsPassword(user.NewPassword))

                return new BaseResultDto(isSuccess: false, val1: Resource.Notification.ThePasswordIsWrong, val2: nameof(user.NewPassword));

            user.Mobile = await user.Mobile.ToEnglishDigitsAsync();
            user.OldPassword = await user.OldPassword.ToEnglishDigitsAsync();
            var item = await _context.Users.FirstOrDefaultAsync(s =>
                (s.Mobile == user.Mobile || s.Email == user.Mobile) &&
                !s.Deleted &&
                !s.Locked);
            if (item == null ||
                !_passwordService.VerifyPassword(
                    item,
                    item.Password,
                    user.OldPassword).Succeeded)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);
            }
            else
            {
                item.Password = _passwordService.HashPassword(
                    item,
                    user.NewPassword);
                _context.Users.Update(item);
                await _context.SaveChangesAsync();
                return await userTokenSevice.ResetTokenAsync(item);
            }
        }
        public async Task<BaseResultDto> ForgetPassword(ForgetPasswordDto dto)
        {
            dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
            dto.Email = await dto.Email.ToEnglishDigitsAsync();
            var item = await _context.Users.FirstOrDefaultAsync(s =>
                        (((!string.IsNullOrEmpty(s.Mobile)) && s.Mobile.Equals(dto.Mobile)) ||
                        ((!string.IsNullOrEmpty(s.Email)) && s.Email.Equals(dto.Email)))
                        && s.Deleted != true && s.Locked != true);
            if (item != null)
            {
                await otpVerifyService.InsertAsyncDto(new OtpVerifyVDto() { Type = MessageTypeEnum.ForgetPassword, Mobile = item.Mobile, Email = item.Email });
                var messages = new List<Tuple<string, string>>();
                messages.Add(new Tuple<string, string>(Resource.Notification.TheRecoveryCodeWasEmailed, nameof(dto.Email)));
                messages.Add(new Tuple<string, string>(Resource.Notification.TheRecoveryCodeWasSentBySMS, nameof(dto.Mobile)));

                return new BaseResultDto(isSuccess: true, messages: messages);

            }
            return new BaseResultDto(false, val: Resource.Notification.InvalidData);
        }
        public async Task<BaseResultDto> ResetPassword(ResetPasswordDto dto)
        {
            dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
            dto.Email = await dto.Email.ToEnglishDigitsAsync();
            dto.Code = await dto.Code.ToEnglishDigitsAsync();
            dto.Password = await dto.Password.ToEnglishDigitsAsync();

            if (!await RegixHelper.IsPassword(dto.Password))
            {
                return new BaseResultDto(isSuccess: false, val1: Resource.Notification.ThePasswordIsWrong, val2: nameof(dto.Password));
            }

            if (string.IsNullOrEmpty(dto.Mobile) && string.IsNullOrEmpty(dto.Email))
            {
                return new BaseResultDto(false, val: Resource.Notification.InvalidData);
            }
            if (string.IsNullOrEmpty(dto.Code))
            {
                return new BaseResultDto(false, val1: Resource.Notification.InvalidCode, val2: nameof(dto.Code));
            }
            var codeVerified = await otpVerifyService.IsVerified(new OtpVerifyVDto() { Mobile = dto.Mobile, Email = dto.Email, Code = dto.Code });
            if (codeVerified.IsSuccess == false)
            {
                return new BaseResultDto(isSuccess: false, val1: Resource.Notification.TheCodeIsWrong, val2: nameof(dto.Code));
            }
            var item = await _context.Users.FirstOrDefaultAsync(s =>
                        (((!string.IsNullOrEmpty(s.Mobile)) && s.Mobile.Equals(dto.Mobile)) ||
                        ((!string.IsNullOrEmpty(s.Email)) && s.Email.Equals(dto.Email)))
                        && s.Deleted != true && s.Locked != true);
            if (item != null)
            {

                item.RequestCode = null;
                item.RequestCodeTryCount = 0;
                item.Password = _passwordService.HashPassword(
                    item,
                    dto.Password);
                _context.Users.Update(item);
                await _context.SaveChangesAsync();
                return await userTokenSevice.ResetTokenAsync(item);


            }
            return new BaseResultDto(false, val: Resource.Notification.InvalidData);

        }
        public async Task<UserDto> GetByMobileDto(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return null;

            mobile = await mobile.Trim().ToEnglishDigitsAsync();

            mobile = mobile.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (mobile.StartsWith("+98"))
                mobile = "0" + mobile.Substring(3);

            if (mobile.StartsWith("98") && mobile.Length == 12)
                mobile = "0" + mobile.Substring(2);

            var item = await _context.Users.AsNoTracking().FirstOrDefaultAsync(s => !s.Deleted && !s.Locked &&s.Mobile == mobile);

            if (item != null)
                return mapper.Map<UserDto>(item);

            return null;
        }
        public async Task<CurrentUserDto> GetByTokenDto(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var hashed = token.Tosha256Hash();
                var userToken = await _context.UserTokens.AsNoTracking()
                    .Include(s => s.User).ThenInclude(s => s.CompanionUsers).ThenInclude(s => s.Companion)
                    .Include(s => s.User).ThenInclude(s => s.CompanionUsers).ThenInclude(s => s.Expertise)
                    .Include(s => s.User).ThenInclude(s => s.Picture).Include(s => s.User).ThenInclude(s => s.Role)
                    .Include(s => s.User).ThenInclude(s => s.Companions).Include(s => s.User).ThenInclude(s => s.Driver)
                    .Include(s => s.User).ThenInclude(s => s.UserCurrentLocation).ThenInclude(s => s.City)
                    .Include(s => s.User).ThenInclude(s => s.UserCurrentLocation).ThenInclude(s => s.Neighborhood)
                    .Include(s => s.User).ThenInclude(s => s.Stores).FirstOrDefaultAsync(x => x.TokenHash == hashed);
                if (userToken != null)
                {
                    var currentUser = mapper.Map<CurrentUserDto>(userToken.User);
                    currentUser.Expertise = await ResolveProfileExpertiseAsync(
                        userToken.User.Id,
                        userToken.User.Expertise);

                    return currentUser;
                }
            }

            return null;
        }

        public List<UserVDto> GetWithRole(RoleEnum roleEnum)
        {
            var model = _context.Users
                .Include(s => s.UserCurrentLocation).ThenInclude(s => s.City)
                .Include(s => s.UserCurrentLocation).ThenInclude(s => s.Neighborhood)
                .Where(s => !s.Deleted && !s.Locked && s.Role.Label == roleEnum.ToString());
            return mapper.Map<List<UserVDto>>(model);
        }
        public async Task<BaseResultDto> UserDetail(UserDetailDto user)
        {
            user.Mobile = await user.Mobile.ToEnglishDigitsAsync();
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Mobile == user.Mobile || x.Email == user.Mobile);
            if (item == null)
                return new BaseResultDto(isSuccess: true, code: (int)ResultCodeEnum.SignUp);
            else
            {
                if (item.Locked)
                {
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.TheUserAccountIsBlocked, code: (int)ResultCodeEnum.Locked);
                }
                else if (item.TwoFactorEnabled == false)
                {
                    return new BaseResultDto(isSuccess: true, code: (int)ResultCodeEnum.OneFactor);
                }
                else
                {
                    return new BaseResultDto(isSuccess: true, code: (int)ResultCodeEnum.TwoFactor);
                }
            }
        }
        public async Task<BaseResultDto> UserRole(string mobile)
        {
            mobile = await mobile.ToEnglishDigitsAsync();

            var item = await _context.Users.Include(s => s.Role).FirstOrDefaultAsync(x => !x.Deleted &&(x.Mobile == mobile || x.Email == mobile));

            if (item == null)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.UserNotFound, code: 2);
            }
            if (item.Locked)
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.TheUserAccountIsBlocked, code: 4);
            }
            if (item.Role.Label == RoleEnum.Customer.ToString())
            {
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.AccessDenied, code: 1);
            }
            return new BaseResultDto(isSuccess: true, code: 0);
        }
        private async Task ChangUserCartAsync(long userId, string cartCode)
        {
            if (!string.IsNullOrEmpty(cartCode))
            {
                var currentCart = await _context.Carts.AsTracking().FirstOrDefaultAsync(s => s.UniqueId == cartCode);
                if (currentCart != null)
                {
                    var carts = _context.Carts.Include(s => s.CartStores).ThenInclude(s => s.CartItems).AsTracking().Where(s => s.UserId == userId && s.Id != currentCart.Id).ToList();
                    foreach (var cart in carts)
                    {
                        foreach (var cartStore in cart.CartStores)
                        {
                            _context.CartItems.RemoveRange(cartStore.CartItems);
                            _context.CartStores.Remove(cartStore);
                        }
                        _context.Carts.Remove(cart);
                    }
                }
                currentCart.UserId = userId;
                _context.Carts.Update(currentCart);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<BaseResultDto> ChangeMobileRequestAsync(ChangeMobileDto dto)
        {
            dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == dto.UserId && s.Deleted != true && s.Locked != true);
            if (user == null)
            {
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            }
            if (!await RegixHelper.IsEmailAsync(dto.Mobile))
            {
                return new BaseResultDto(false, Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile));
            }
            if (!MobileIsUnique(dto.Mobile))
            {
                return new BaseResultDto(false, Resource.Notification.TheMobileNumberIsDuplicate, nameof(dto.Mobile));
            }

            await otpVerifyService.InsertAsyncDto(new OtpVerifyVDto() { Type = MessageTypeEnum.ChangeMobile, Mobile = dto.Mobile, Email = user.Email });
            return new BaseResultDto(true, Resource.Notification.TheMobileChangeRequestSent);

        }
        public async Task<BaseResultDto> ChangeMobileAsync(ChangeMobileDto dto)
        {
            var modelCheker = ModelHelper<ChangeMobileDto>.ModelErrors(dto);
            if (!modelCheker.IsSuccess)
            {
                return modelCheker;
            }
            dto.Mobile = await dto.Mobile.ToEnglishDigitsAsync();
            dto.Code = await dto.Code.ToEnglishDigitsAsync();

            var user = await _context.Users.Include(s => s.Role).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.UserId && s.Deleted != true && s.Locked != true);
            if (user == null)
            {
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            }
            if (!await RegixHelper.IsMobileAsync(dto.Mobile))
            {
                return new BaseResultDto(false, Resource.Notification.TheMobileNumberIsWrong, nameof(dto.Mobile));
            }
            if (!MobileIsUnique(dto.Mobile))
            {
                return new BaseResultDto(false, Resource.Notification.TheMobileNumberIsDuplicate, nameof(dto.Mobile));
            }

            var codeVerified = await otpVerifyService.IsVerified(new OtpVerifyVDto() { Mobile = dto.Mobile, Email = user.Email, Code = dto.Code });
            if (codeVerified.IsSuccess == false)
            {
                return new BaseResultDto(isSuccess: false, val1: Resource.Notification.TheCodeIsWrong, val2: nameof(dto.Code));
            }
            user.Mobile = dto.Mobile;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return await userTokenSevice.ResetTokenAsync(user);


        }
        public async Task<BaseResultDto> ChangeEmailRequestAsync(ChangeEmailDto dto)
        {
            dto.Email = await dto.Email.ToEnglishDigitsAsync();
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Id == dto.UserId && s.Deleted != true && s.Locked != true);
            if (user == null)
            {
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            }
            if (!await RegixHelper.IsEmailAsync(dto.Email))
            {
                return new BaseResultDto(false, Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email));
            }
            if (!MobileIsUnique(dto.Email))
            {
                return new BaseResultDto(false, Resource.Notification.TheEmailAddressIsDuplicate, nameof(dto.Email));
            }

            await otpVerifyService.InsertAsyncDto(new OtpVerifyVDto() { Type = MessageTypeEnum.ChangeEmail, Mobile = user.Mobile, Email = dto.Email });
            return new BaseResultDto(true, Resource.Notification.TheEmailChangeRequestSent);

        }
        public async Task<BaseResultDto> ChangeEmailAsync(ChangeEmailDto dto)
        {
            var modelCheker = ModelHelper<ChangeEmailDto>.ModelErrors(dto);
            if (!modelCheker.IsSuccess)
            {
                return modelCheker;
            }
            dto.Email = await dto.Email.ToEnglishDigitsAsync();
            dto.Code = await dto.Code.ToEnglishDigitsAsync();

            var user = await _context.Users.Include(s => s.Role).AsTracking().FirstOrDefaultAsync(s => s.Id == dto.UserId && s.Deleted != true && s.Locked != true);
            if (user == null)
            {
                return new BaseResultDto(false, Resource.Notification.AccessDenied);
            }
            if (!await RegixHelper.IsEmailAsync(dto.Email))
            {
                return new BaseResultDto(false, Resource.Notification.TheEmailAddressIsWrong, nameof(dto.Email));
            }
            if (!MobileIsUnique(dto.Email))
            {
                return new BaseResultDto(false, Resource.Notification.TheEmailAddressIsDuplicate, nameof(dto.Email));
            }

            var codeVerified = await otpVerifyService.IsVerified(new OtpVerifyVDto() { Mobile = user.Mobile, Email = dto.Email, Code = dto.Code });
            if (codeVerified.IsSuccess == false)
            {
                return new BaseResultDto(isSuccess: false, val1: Resource.Notification.TheCodeIsWrong, val2: nameof(dto.Code));
            }
            user.Email = dto.Email;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true, Resource.Notification.MobileChangedSuccessfully);

        }

        public async Task<UserDto> GetUserByReferralCodeAsync(string referralCode)
        {
            var user = await _context.Users
                .Where(u => u.ReferralCode == referralCode)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                ReferralCode = user.ReferralCode,
            };

            return userDto;
        }

        public override async Task<BaseResultDto<UserDto>> FindAsyncDto(long id)
        {
            var item = await _context.Users.Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                var dto = mapper.Map<UserDto>(item);
                dto.Expertise = await ResolveProfileExpertiseAsync(item.Id, item.Expertise);

                return new BaseResultDto<UserDto>(true, dto);
            }
            return new BaseResultDto<UserDto>(false, mapper.Map<UserDto>(item));
        }

        private bool CanHaveExpertise(long userId)
        {
            return _context.Companions.Any(companion =>
                       companion.OwnerId == userId &&
                       !companion.Deleted &&
                       companion.Active &&
                       companion.Approved) ||
                   _context.CompanionUsers.Any(companionUser =>
                       companionUser.UserId == userId &&
                       !companionUser.Deleted &&
                       companionUser.Active &&
                       companionUser.UserAccept == true &&
                       !companionUser.Companion.Deleted &&
                       companionUser.Companion.Active &&
                       companionUser.Companion.Approved);
        }

        private async Task<bool> CanHaveExpertiseAsync(long userId)
        {
            return await _context.Companions.AsNoTracking().AnyAsync(companion =>
                       companion.OwnerId == userId &&
                       !companion.Deleted &&
                       companion.Active &&
                       companion.Approved) ||
                   await _context.CompanionUsers.AsNoTracking().AnyAsync(companionUser =>
                       companionUser.UserId == userId &&
                       !companionUser.Deleted &&
                       companionUser.Active &&
                       companionUser.UserAccept == true &&
                       !companionUser.Companion.Deleted &&
                       companionUser.Companion.Active &&
                       companionUser.Companion.Approved);
        }

        private async Task<string> ResolveProfileExpertiseAsync(
            long userId,
            string storedExpertise)
        {
            var assignedExpertise = await _context.CompanionUsers
                .AsNoTracking()
                .Where(companionUser =>
                    companionUser.UserId == userId &&
                    !companionUser.Deleted &&
                    companionUser.UserAccept != false &&
                    companionUser.ExpertiseId.HasValue &&
                    companionUser.Expertise.Active &&
                    !companionUser.Expertise.Deleted)
                .OrderByDescending(companionUser => companionUser.UserAccept == true)
                .ThenByDescending(companionUser => companionUser.Id)
                .Select(companionUser => companionUser.Expertise.Name)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(assignedExpertise))
                return assignedExpertise;

            return await CanHaveExpertiseAsync(userId)
                ? storedExpertise
                : null;
        }
    }
}
