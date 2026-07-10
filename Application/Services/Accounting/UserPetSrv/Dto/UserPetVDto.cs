using Application.Common.Dto.Field;
using Application.Services.Accounting.PetSrv.Dto;
using Application.Services.Accounting.UserPerRecordSrv.Dto;
using Application.Services.Accounting.UserPetPictureSrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;

namespace Application.Services.Accounting.UserPetSrv.Dto
{
    public class UserPetVDto : Name_FieldDto
    {
        public long PetId { get; set; }
        public long UserId { get; set; }
        public long? PictureId { get; set; }
        public long? PetBreedId { get; set; }
        public long? PetBreed2Id { get; set; }
        public bool IsMixBreed { get; set; }
        public DateTime Birthday { get; set; }
        public string MicroChipCode { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public bool IsMale { get; set; }
        public bool IsSterile { get; set; }
        public string SpecificDisease { get; set; }
        public string SpecificMedicene { get; set; }
        public string AddressValue { get; set; }
        public UserMinVDto User { get; set; }
        public PetVDto Pet { get; set; }
        public PictureVDto Picture { get; set; }
        public List<UserPetRecordMinVDto> UserPetRecords { get; set; }
        public List<UserPetPictureVDto> UserPetPictures { get; set; }
    }
}
