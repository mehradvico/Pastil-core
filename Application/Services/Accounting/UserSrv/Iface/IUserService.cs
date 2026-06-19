using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.Accounting.UserSrv.Dto;
using Application.Services.Dto;
using Entities.Entities.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserSrv.Iface
{
    public interface IUserService : ICommonSrv<User, UserDto>
    {
        UserVDto GetVDto(long userId);
        public UserSearchDto Search(UserInputDto searchDto);
        Task<BaseResultDto> CheckUser(string token, long userId, string area, string controller, string action);
        Task<BaseResultDto> SignIn(SignInDto user);
        Task<BaseResultDto> SignUp(SignUpDto user);
        Task<BaseResultDto> ChangePassword(ChangePasswordDto user);
        Task<BaseResultDto> ForgetPassword(ForgetPasswordDto dto);
        Task<BaseResultDto> ResetPassword(ResetPasswordDto dto);
        Task<UserDto> GetByMobileDto(string mobile);
        Task<CurrentUserDto> GetByTokenDto(string token);
        List<UserVDto> GetWithRole(RoleEnum roleEnum);
        Task<BaseResultDto> UserDetail(UserDetailDto user);
        Task<BaseResultDto> UserRole(string mobile);
        Task<BaseResultDto> ChangeMobileRequestAsync(ChangeMobileDto dto);
        Task<BaseResultDto> ChangeMobileAsync(ChangeMobileDto dto);
        Task<BaseResultDto> ChangeEmailRequestAsync(ChangeEmailDto dto);
        Task<BaseResultDto> ChangeEmailAsync(ChangeEmailDto dto);
        Task<UserDto> GetUserByReferralCodeAsync(string referralCode);
        BaseResultDto<User> GetUserByMobile(string mobile);
    }
}
