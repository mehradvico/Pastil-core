using Application.Common.Service;
using Application.Services.Content.PostPictureSrv.Dto;
using Application.Services.Content.PostPictureSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.Content.PostPictureSrv
{
    public class PostPictureService : CommonSrv<PostPicture, PostPictureDto>, IPostPictureService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public PostPictureService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public PostPictureSearchDto Search(PostPictureInputDto searchDto)
        {
            var model = _context.PostPictures.Include(s => s.Picture).AsQueryable();
            if (searchDto.PostId.HasValue)
            {
                model = model.Where(s => s.PostId.Equals(searchDto.PostId));
            }
            if (!string.IsNullOrEmpty(searchDto.Q))
            {
                model = model.Where(s => s.Label.Contains(searchDto.Q));
            }
            return new PostPictureSearchDto(searchDto, model, mapper);
        }
        public void InsertOrUpdate(PostPictureDto PostPicture)
        {
            var item = _context.PostPictures.FirstOrDefault(s => s.PostId == PostPicture.PostId && s.PictureId == PostPicture.PictureId);
            if (item != null)
            {
                item.Name = PostPicture.Name;
                item.Label = PostPicture.Label;
                _context.PostPictures.Update(item);
            }
            else
            {
                item = mapper.Map<PostPicture>(PostPicture);
                _context.PostPictures.Add(item);
            }
            _context.SaveChanges();
        }

        public void InsertOrUpdate(Post post, List<PostPictureDto> PostPicturesDto)
        {
            if (PostPicturesDto == null)
            {
                return;
            }

            var oldItems = post.PostPictures?.ToList() ?? _context.PostPictures
                .Where(s => s.PostId == post.Id)
                .ToList();

            if (oldItems.Any())
            {
                _context.PostPictures.RemoveRange(oldItems);
            }

            var newItems = PostPicturesDto
                .Where(s => s != null)
                .GroupBy(s => s.PictureId)
                .Select(group =>
                {
                    var s = group.First();
                    s.PostId = post.Id;
                    return mapper.Map<PostPicture>(s);
                })
                .ToList();

            if (newItems.Any())
            {
                _context.PostPictures.AddRange(newItems);
            }

            _context.SaveChanges();
            post.PostPictures = newItems;
        }
    }
}
