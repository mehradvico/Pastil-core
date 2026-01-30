using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.CommonSrv.CommentLikeSrv.Dto;
using Application.Services.CommonSrv.CommentLikeSrv.Iface;
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

namespace Application.Services.CommonSrv.CommentLikeSrv
{
    public class CommentLikeService : CommonSrv<CommentLike, CommentLikeDto>, ICommentLikeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly CurrentUserDto currentUser;
        private readonly string connectionString;

        public CommentLikeService(IDataBaseContext _context, IMapper mapper, IConfiguration config, ICurrentUserHelper currentUserHelper, IMessageSenderService messageService, IUserService userService, IProductService productService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.currentUser = currentUserHelper.CurrentUser;
            this.connectionString = config.GetValue<string>(
           "connection");
        }
        public override BaseResultDto DeleteDto(CommentLikeDto dto)
        {
            var item = _context.CommentLikes.FirstOrDefault(s => s.CommentId == dto.CommentId);
            if (item != null)
            {
                _context.CommentLikes.Remove(item);
                _context.SaveChanges();
                return new BaseResultDto(isSuccess: false, val: Resource.Notification.Success);
            }
            return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);
        }
        public CommentLikeSearchDto SearchDto(CommentLikeInputDto dto)
        {
            var model = _context.CommentLikes.Include(s => s.Comment).Select(s => s.Comment).AsQueryable();
            return new CommentLikeSearchDto(dto, model, mapper);
        }
        public async Task<BaseResultDto> InsertOrDeleteAsync(CommentLikeInsertDeleteDto dto)
        {
            var item = await _context.CommentLikes.FirstOrDefaultAsync(s => s.CommentId == dto.CommentId && s.UserId == currentUser.UserId);
            if (dto.IsLike.HasValue)
            {
                if (item != null)
                {
                    item.IsLike = dto.IsLike.Value;
                    item.CreateDate = System.DateTime.Now;
                    _context.CommentLikes.Update(item);
                }
                else
                {
                    item = new CommentLike() { IsLike = dto.IsLike.Value, CommentId = dto.CommentId, UserId = currentUser.UserId, CreateDate = DateTime.Now };
                    await _context.CommentLikes.AddAsync(item);
                }
            }
            else
            {
                if (item != null)
                {
                    _context.CommentLikes.Remove(item);
                }
                else
                {
                    return new BaseResultDto(false);
                }
            }
            await _context.SaveChangesAsync();
            await UpdateProductCommenLikeAsync(dto.CommentId);
            return new BaseResultDto(true);
        }
        public async Task UpdateProductCommenLikeAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateCommentLikes", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
