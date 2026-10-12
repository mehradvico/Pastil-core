namespace Utility.Reflection.Dto
{
    public class ControllerActionInfoDto
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Parent { get; set; }
        public long ParentId { get; set; }
        // کنترلر اصلیِ بخشی که این کنترلر کمکی زیرش می‌نشیند (null = مستقیم زیر گروه)
        public string HostController { get; set; }
        public bool IsMenu { get; set; }
        public int Priority { get; set; }
        public List<ActionInfoDto> Actions { get; set; }
    }
}
