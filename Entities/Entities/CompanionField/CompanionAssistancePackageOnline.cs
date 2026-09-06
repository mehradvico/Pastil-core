using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class CompanionAssistancePackageOnline : Name_Field
    {
        public double Price { get; set; }
        public bool Active { get; set; }
        // یعنی این نوع ارتباط «فوری» است (رزرو با DoDate=همین الان، بدون نیاز به انتخاب زمان از قبل).
        public bool IsInstant { get; set; }
        public bool Deleted { get; set; }
    }
}
