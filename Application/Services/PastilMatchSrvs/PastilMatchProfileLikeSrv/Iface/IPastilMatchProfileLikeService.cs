using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Dto;
using Entities.Entities.PastilMatchField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilMatchSrvs.PastilMatchProfileLikeSrv.Iface
{
    public interface IPastilMatchProfileLikeService : ICommonSrv<PastilMatchProfileLike, PastilMatchProfileLikeDto>
    {
    }
}
