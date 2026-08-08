using Application.Common.Dto.Result;
using Application.Common.Enumerable.Message;
using Application.Services.Setting.EmailSrv.Iface;
using Application.Services.Setting.MessageSenderSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.Notice;

public class MessageSenderNoticeTests
{
    [Fact]
    public async Task OtpSms_IsSent_WithoutCreatingAdminNotice()
    {
        var sms = new SmsStub();
        var notice = new NoticeStub();
        var service = new MessageSenderService(
            new EmailStub(),
            sms,
            notice,
            NullLogger<MessageSenderService>.Instance
        );

        await service.SendMessageAsync(MessageTypeEnum.Otp, "09120000000", null, token1: "12345");

        Assert.Equal(1, sms.SendCount);
        Assert.Equal(0, notice.CreateCount);
    }

    [Fact]
    public async Task NonOtpSms_CreatesAdminNotice()
    {
        var notice = new NoticeStub();
        var service = new MessageSenderService(
            new EmailStub(),
            new SmsStub(),
            notice,
            NullLogger<MessageSenderService>.Instance
        );

        await service.SendMessageAsync(MessageTypeEnum.UserSignUp, "09120000000", null);

        Assert.Equal(1, notice.CreateCount);
    }

    private sealed class SmsStub : ISmsService
    {
        public int SendCount { get; private set; }

        public Task SendSmsAsync(MessageTypeEnum smsType, string receptor, string body = null, string token1 = null, string token2 = null, string token3 = null, string token4 = null, string token5 = null, DateTime? sendDate = null)
        {
            SendCount++;
            return Task.CompletedTask;
        }

        public Task SendSmsGroupAsync(int pageSize = 100) => Task.CompletedTask;
    }

    private sealed class EmailStub : IEmailService
    {
        public Task SendEmailAsync(MessageTypeEnum emailType, string receptor, string body = null, string token1 = null, string token2 = null, string token3 = null, string token4 = null, string token5 = null, DateTime? sendDate = null) => Task.CompletedTask;

        public Task SendEmailGroupAsync(int pageSize = 100) => Task.CompletedTask;
    }

    private sealed class NoticeStub : INoticeEventService
    {
        public int CreateCount { get; private set; }

        public Task<BaseResultDto<NoticeDto>> CreateAsync(NoticeCreateDto dto)
        {
            CreateCount++;
            return Task.FromResult(new BaseResultDto<NoticeDto>(true, (NoticeDto)null));
        }
    }
}
