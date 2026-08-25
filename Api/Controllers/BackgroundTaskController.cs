using Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility.BackgroundTask.Iface;
using Utility.Reflection.Iface;

namespace Api.Controllers
{
    /// <summary>
    /// پس زمینه
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class BackgroundTaskController : ControllerBase
    {
        private readonly IBackgroundTask backgroundTask;
        private readonly IControllerActionDiscoveryService _controllerActionDiscoveryService;
        /// <summary>
        /// مرتبط با پس زمینه
        /// </summary>
        public BackgroundTaskController(IBackgroundTask backgroundTask, IControllerActionDiscoveryService controllerActionDiscoveryService)
        {
            this.backgroundTask = backgroundTask;
            this._controllerActionDiscoveryService = controllerActionDiscoveryService;
        }
        /// <summary>
        /// اجرا پس زمینه
        /// </summary>
        ///        
        [HttpGet]
        public IActionResult Get()
        {

            backgroundTask.StartSyncSmsAsync();
            backgroundTask.StartSyncCloseTicketAsync();
            backgroundTask.StartSyncExpiredDiscountAsync();
            backgroundTask.StartSyncReminderAsync();
            backgroundTask.StartSyncDriverAcceptAsync();
            //string  xmlFile = $"ZandShop.Api.xml";
            //string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //XDocument _xmlComments = XDocument.Load(xmlPath);
            //_controllerActionDiscoveryService.GetControllerActions( _xmlComments);
            return Ok();
        }
    }
}
