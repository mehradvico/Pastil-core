namespace Utility.Reflection.Dto
{
    public class PermissionSyncResultDto
    {
        public int ParentCount { get; set; }
        public int ControllerCount { get; set; }
        public int ActionCount { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int MergedDuplicateCount { get; set; }
        public List<string> UnmappedControllers { get; set; } = [];
    }
}
