using Application.Common.Enumerable.Message;
using Application.Services.Setting.EmailSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Setting.MessageSenderSrv
{
    public class MessageSenderService : IMessageSenderService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly INoticeEventService _noticeEventService;
        private readonly ILogger<MessageSenderService> _logger;
        public MessageSenderService(IEmailService emailService, ISmsService smsService, INoticeEventService noticeEventService, ILogger<MessageSenderService> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _noticeEventService = noticeEventService;
            _logger = logger;
        }

        public async Task SendMessageAsync(MessageTypeEnum messageType, string mobileReceptor, string emailReceptor, string body = null, string token1 = null, string token2 = null, string token3 = null, string token4 = null, string token5 = null, DateTime? sendDate = null)
        {
            if (!string.IsNullOrEmpty(mobileReceptor))
            {
                await _smsService.SendSmsAsync(smsType: messageType, receptor: mobileReceptor, body: body, token1: token1, token2: token2, token3: token3, token4: token4, token5: token5, sendDate: sendDate);
                // اعلان «ارسال پیامک» برای ادمین ساخته نمی‌شود (شلوغی زنگ اعلان‌ها؛ رویداد اصلی مثل «ثبت‌نام کاربر جدید» خودش اعلان دارد).
            }

            if (!string.IsNullOrEmpty(emailReceptor))
                await _emailService.SendEmailAsync(emailType: messageType, receptor: emailReceptor, body: body, token1: token1, token2: token2, token3: token3, token4: token4, token5: token5, sendDate: sendDate);
        }
    }
}
