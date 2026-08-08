using Application.Common.Dto.Field;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetBreedSrv.Dto
{
    public class PetBreedVDto : Name_FieldDto
    {
        public long PetId { get; set; }
        public int Priority { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public long? PictureId { get; set; }

        public PetVDto Pet { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
