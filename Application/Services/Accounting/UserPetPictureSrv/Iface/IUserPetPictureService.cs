using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.UserPetPictureSrv.Iface
{
    public interface IUserPetPictureService : ICommonSrv<UserPetPicture, UserPetPictureDto>
    {
        UserPetPictureSearchDto Search(UserPetPictureInputDto searchDto);
        Task<BaseResultDto<UserPetPictureVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto<UserPetPictureVDto>> FindAsyncVDto(long id, long userId);
        Task<bool> IsUserPetOwnedByAsync(long userPetId, long userId);

        void InsertOrUpdate(UserPetPictureDto UserPetPicture);
        void InsertOrUpdate(UserPet UserPet, List<UserPetPictureDto> UserPetPicturesDto);
    }
}
