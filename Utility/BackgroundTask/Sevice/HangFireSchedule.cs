using Application.Services.Accounting.TicketSrv.Iface;
using Application.Services.Order.ProductOrderSrv.Iface;
using Application.Services.ProductSrvs.DiscountSrv.IFace;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using Application.Services.TripSrv.TripSrv.Iface;
using Hangfire;
using Utility.BackgroundTask.Iface;
using Utility.ExternalRequest.Iface;

namespace Api.HangFire
{
    public class HangFireSchedule : IBackgroundTask
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ISmsService smsService;
        private readonly ITicketService ticketService;
        private readonly IProductOrderService _productOrderService;
        private readonly IRestSharpApi restSharpApi;
        private readonly IDiscountService _discountService;
        private readonly IReminderService _reminderService;
        private readonly ITripService _tripService;
        private static bool SyncSmsIsSyncing = false;
        private static bool SyncCloseTicketSyncing = false;
        private static bool SyncExpiredDiscountSyncing = false;
        private static bool SyncReminderSyncing = false;
        private static bool SyncDriverAcceptSyncing = false;
        public HangFireSchedule(IRecurringJobManager recurringJobManager, ITicketService ticketService, ISmsService smsService, IRestSharpApi restSharpApi, IProductOrderService productOrderService, IDiscountService discountService, IReminderService reminderService, ITripService tripService)
        {
            _recurringJobManager = recurringJobManager;
            this.smsService = smsService;
            this.restSharpApi = restSharpApi;
            this.ticketService = ticketService;
            this._productOrderService = productOrderService;
            this._discountService = discountService;
            this._reminderService = reminderService;
            this._tripService = tripService;
        }
        public async Task StartSyncSmsAsync()
        {
            await Task.Run(() =>
            {
                _recurringJobManager.AddOrUpdate("Sms", () => SyncSmsAsync(), "*/30 * * * * *");
            });
        }
        public async Task StartSyncCloseTicketAsync()
        {
            await Task.Run(() =>
            {
                _recurringJobManager.AddOrUpdate("CloseTicket", () => SyncCloseTicketAsync(), "0 */2 * * *");
            });
        }
        public async Task StartSyncExpiredDiscountAsync()
        {
            await Task.Run(() =>
            {
                _recurringJobManager.AddOrUpdate("ExpiredDiscount1", () => SyncExpiredDiscountAsync(), "1 0 * * *");
                _recurringJobManager.AddOrUpdate("ExpiredDiscount2", () => SyncExpiredDiscountAsync(), "0 */6 * * *");
                //_recurringJobManager.AddOrUpdate("ExpiredDiscount2", () => SyncExpiredDiscountAsync(), "*/30 * * * * *");
            });
        }

        public async Task StartSyncReminderAsync()
        {
            await Task.Run(() =>
            {
                _recurringJobManager.AddOrUpdate("Reminder", () => SyncReminderAsync(), "*/20 * * * *");
            });
        }

        public async Task SyncReminderAsync()
        {
            try
            {
                if (DateTime.Now.Hour == 9 || DateTime.Now.Hour == 10 || DateTime.Now.Hour == 11)
                {
                    if (!SyncReminderSyncing)
                    {
                        SyncReminderSyncing = true;
                        await _reminderService.SyncReminderAsync();
                        SyncReminderSyncing = false;
                    }
                }

            }
            catch (Exception)
            {
                SyncReminderSyncing = false;
            }
        }

        public async Task SyncExpiredDiscountAsync()
        {
            try
            {
                if (!SyncExpiredDiscountSyncing)
                {
                    SyncExpiredDiscountSyncing = true;
                    await _discountService.SyncExpiredAsync();
                    SyncExpiredDiscountSyncing = false;
                }
            }
            catch
            {
                SyncExpiredDiscountSyncing = false;
            }
        }
        public async Task SyncSmsAsync()
        {
            try
            {
                if (!SyncSmsIsSyncing)
                {
                    SyncSmsIsSyncing = true;
                    await smsService.SendSmsGroupAsync();
                    SyncSmsIsSyncing = false;
                }
            }
            catch
            {
                SyncSmsIsSyncing = false;
            }
        }




        public async Task SyncCloseTicketAsync()
        {
            try
            {
                if (!SyncCloseTicketSyncing)
                {
                    SyncCloseTicketSyncing = true;
                    await ticketService.CloseTicketAsync();

                    SyncCloseTicketSyncing = false;
                }
            }
            catch
            {
                SyncCloseTicketSyncing = false;
            }
        }


        public async Task StartSyncDriverAcceptAsync()
        {
            await Task.Run(() =>
            {
                _recurringJobManager.AddOrUpdate("DriverAccept", () => SyncDriverAcceptAsync(), "*/20 * * * *");
            });
        }

        public async Task SyncDriverAcceptAsync()
        {
            try
            {
                if (!SyncDriverAcceptSyncing)
                {
                    SyncDriverAcceptSyncing = true;
                    await _tripService.SyncDriverAcceptAsync();
                    SyncDriverAcceptSyncing = false;
                }

            }
            catch (Exception)
            {
                SyncDriverAcceptSyncing = false;
            }
        }

    }
}
