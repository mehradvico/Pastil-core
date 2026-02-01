using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Accounting.UserSrv.Iface;
using Application.Services.Content.StoryItemSrv.Iface;
using Application.Services.Content.StoryUserLikeSrv.Dto;
using Application.Services.Content.StoryUserLikeSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Dapper;

namespace Application.Services.Content.StoryUserUserLikeSrv
{
    public class StoryUserLikeService : CommonSrv<StoryUserLike, StoryUserLikeDto>, IStoryUserLikeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IMessageSenderService messageService;
        private readonly IUserService userService;
        private readonly IStoryItemService _storyItemService;
        private readonly string connectionString;
        public StoryUserLikeService(IDataBaseContext _context, IConfiguration config, IMapper mapper, IMessageSenderService messageService, IUserService userService, IStoryItemService storyitemService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this.messageService = messageService;
            this.userService = userService;
            this._storyItemService = storyitemService;
            this.connectionString = config.GetValue<string>("connection");
        }

        public StoryUserLikeSearchDto SearchDto(StoryUserLikeInputDto dto)
        {
            var model = _context.StoryUserLikes.Where(s => s.UserId.Equals(dto.UserId)).Select(s => s.StoryItem).AsQueryable();
            return new StoryUserLikeSearchDto(dto, model, mapper);
        }

        public async Task ToggleLikeAsync(long storyItemId, long userId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "ToggleStoryLike",
                new { StoryItemId = storyItemId, UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
