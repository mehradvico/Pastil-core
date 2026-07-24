using System.Xml.Linq;
using Utility.Reflection.Dto;
using System.Reflection;

namespace Utility.Reflection.Iface
{
    public interface IControllerActionDiscoveryService
    {
        List<ControllerActionInfoDto> GetControllerActions(Assembly assembly, XDocument xmlComments);
        Task<PermissionSyncResultDto> SynchronizePermissionsAsync(
            Assembly assembly,
            XDocument xmlComments,
            CancellationToken cancellationToken = default);
    }
}
