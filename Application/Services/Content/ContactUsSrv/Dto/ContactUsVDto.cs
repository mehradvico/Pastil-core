using Application.Common.Dto.Field;
using Application.Services.Content.ContactUsGroupSrv.Dto;
using Application.Services.Content.ContactUsItemSrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.FileSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Content.ContactUsSrv.Dto
{
    public class ContactUsVDto : Id_FieldDto
    {
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool Status { get; set; }
        public long ContactUsGroupId { get; set; }
        public string Answer { get; set; }
        public long? FileId { get; set; }
        public UserMinVDto User { get; set; }
        public FileVDto File { get; set; }
        public ContactUsGroupDto ContactUsGroup { get; set; }
        public List<ContactUsItemDto> ContactUsItems { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
