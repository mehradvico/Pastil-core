using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Dto;
using Entities.Entities.SchoolField;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolPictureSrv.Iface
{
    public interface ISchoolPictureService : ICommonSrv<SchoolPicture, SchoolPictureDto>
    {
        SchoolPictureSearchDto Search(SchoolPictureInputDto searchDto);
        Task<BaseResultDto<SchoolPictureVDto>> FindAsyncVDto(long id);
    }
}
