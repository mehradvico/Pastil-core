using System.Collections.Generic;

namespace Application.Services.Content.ContactUsGroupSrv.Dto
{
    public class ContactUsFormFieldDto
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string InputType { get; set; }
        public bool Required { get; set; }
        public int Priority { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public string Placeholder { get; set; }
        public List<ContactUsFormOptionDto> Options { get; set; } = new List<ContactUsFormOptionDto>();
    }

    public class ContactUsFormOptionDto
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }
}
