namespace Entities.Entities.SchoolField
{
    public class SchoolComment : Comment
    {
        public long SchoolId { get; set; }
        public long? SchoolReserveId { get; set; }
        public bool IsReserved { get; set; }
        public School School { get; set; }
        public SchoolReserve SchoolReserve { get; set; }
    }
}
