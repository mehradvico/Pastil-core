using System.Collections.Generic;

namespace Application.Services.Accounting.PermissionSrv.Dto
{
    public class PermissionActionsDto
    {
        public bool IsFullAccess { get; set; }
        public List<PermissionActionItemDto> Items { get; set; }
    }

    public class PermissionActionItemDto
    {
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
