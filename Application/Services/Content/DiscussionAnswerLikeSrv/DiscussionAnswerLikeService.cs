using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Content.DiscussionAnswerLikeSrv.Dto;
using Application.Services.Content.DiscussionAnswerLikeSrv.Iface;
using Application.Services.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Content.DiscussionAnswerLikeSrv
{
    public class DiscussionAnswerLikeService : CommonSrv<DiscussionAnswerLike, DiscussionAnswerLikeDto>, IDiscussionAnswerLikeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly CurrentUserDto currentUser;
        private readonly string connectionString;

        public DiscussionAnswerLikeService(IDataBaseContext _context, IMapper mapper, IConfiguration config, ICurrentUserHelper currentUserHelper, IMessageSenderService messageService, IUserService userService, IProductService productService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.currentUser = currentUserHelper.CurrentUser;
            this.connectionString = config.GetValue<string>(
           "connection");
        }
        public override BaseResultDto DeleteDto(DiscussionAnswerLikeDto dto)
        {
            var item = _context.DiscussionAnswerLikes.FirstOrDefault(s => s.DiscussionAnswerId == dto.DiscussionAnswerId);
            if (item != null)
            {
                _context.DiscussionAnswerLikes.Remove(item);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Success);
            }
            return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);
        }
        public DiscussionAnswerLikeSearchDto SearchDto(DiscussionAnswerLikeInputDto dto)
        {
            var model = _context.DiscussionAnswerLikes.Include(s => s.DiscussionAnswer).Select(s => s.DiscussionAnswer).AsQueryable();
            return new DiscussionAnswerLikeSearchDto(dto, model, mapper);
        }
        public async Task<BaseResultDto> InsertOrDeleteAsync(DiscussionAnswerLikeInsertDeleteDto dto)
        {
            var item = await _context.DiscussionAnswerLikes.FirstOrDefaultAsync(s => s.DiscussionAnswerId == dto.DiscussionAnswerId && s.UserId == currentUser.UserId);
            if (dto.IsLike.HasValue)
            {
                if (item != null)
                {
                    item.IsLike = dto.IsLike.Value;
                    item.CreateDate = System.DateTime.Now;
                    _context.DiscussionAnswerLikes.Update(item);
                }
                else
                {
                    item = new DiscussionAnswerLike() { IsLike = dto.IsLike.Value, DiscussionAnswerId = dto.DiscussionAnswerId, UserId = currentUser.UserId, CreateDate = DateTime.Now };
                    await _context.DiscussionAnswerLikes.AddAsync(item);
                }
            }
            else
            {
                if (item != null)
                {
                    _context.DiscussionAnswerLikes.Remove(item);
                }
                else
                {
                    return new BaseResultDto(false);
                }
            }
            await _context.SaveChangesAsync();
            await UpdateDiscussionAnswerLikeAsync(dto.DiscussionAnswerId);
            return new BaseResultDto(true);
        }
        public async Task UpdateDiscussionAnswerLikeAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateDiscussionAnswerLikes", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
