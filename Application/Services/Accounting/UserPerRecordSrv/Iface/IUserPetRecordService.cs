using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserPerRecordSrv.Iface
{
    public interface IUserPetRecordService : ICommonSrv<UserPetRecord, UserPetRecordDto>
    {
        /// <summary>آیا این کاربر (نیروی تخصیص‌یافته یا صاحب مرکز) در یک رزروِ فعال، این پت را سرویس داده است؟</summary>
        System.Threading.Tasks.Task<bool> CanOperateOnPetAsync(long userPetId, long operatorUserId);
        UserPetRecordSearchDto Search(UserPetRecordInputDto baseSearchDto);
        Task<BaseResultDto<UserPetRecordVDto>> FindAsyncVDto(long id);

    }
}
