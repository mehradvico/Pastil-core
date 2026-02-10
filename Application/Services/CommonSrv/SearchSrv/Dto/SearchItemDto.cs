using Application.Common.Enumerable;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.SearchSrv.Dto
{
    public class SearchItemDto
    {
        public SearchItemType Type { get; set; }
        public long Id { get; set; }

        public string Title { get; set; }
        public string SubTitle { get; set; }

        public PictureVDto Picture { get; set; }

        public double Score { get; set; }
        public string Url { get; set; }
    }
}
