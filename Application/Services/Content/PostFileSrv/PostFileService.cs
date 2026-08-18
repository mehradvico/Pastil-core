using Application.Common.Service;
using Application.Services.Content.PostFileSrv.Dto;
using Application.Services.Content.PostFileSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Content.PostFileSrv
{
    public class PostFileService : CommonSrv<PostFile, PostFileDto>, IPostFileService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PostFileService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public PostFileSearchDto Search(PostFileInputDto searchDto)
        {
            var model = _context.PostFiles.Include(s => s.File).AsQueryable();
            if (searchDto.PostId.HasValue)
            {
                model = model.Where(s => s.PostId.Equals(searchDto.PostId));
            }
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                model = model.Where(s => s.Label.Contains(searchDto.Q));
            }
            return new PostFileSearchDto(searchDto, model, mapper);
        }
        public void InsertOrUpdate(PostFileDto postFile)
        {
            var item = _context.PostFiles.FirstOrDefault(s => s.PostId == postFile.PostId && s.FileId == postFile.FileId);
            if (item != null)
            {
                item.Name = postFile.Name;
                item.Label = postFile.Label;
                _context.PostFiles.Update(item);
            }
            else
            {
                item = mapper.Map<PostFile>(postFile);
                _context.PostFiles.Add(item);
            }
            _context.SaveChanges();
        }

        public void InsertOrUpdate(Post post, List<PostFileDto> postFilesDto)
        {
            if (postFilesDto == null)
            {
                return;
            }

            var oldItems = post.PostFiles?.ToList() ?? _context.PostFiles
                .Where(s => s.PostId == post.Id)
                .ToList();

            if (oldItems.Any())
            {
                _context.PostFiles.RemoveRange(oldItems);
            }

            var newItems = postFilesDto
                .Where(s => s != null)
                .GroupBy(s => s.FileId)
                .Select(group =>
                {
                    var s = group.First();
                    s.PostId = post.Id;
                    return mapper.Map<PostFile>(s);
                })
                .ToList();

            if (newItems.Any())
            {
                _context.PostFiles.AddRange(newItems);
            }

            _context.SaveChanges();
            post.PostFiles = newItems;
        }
    }
}
