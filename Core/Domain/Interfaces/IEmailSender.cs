namespace MindLog.Api.Core.Domain.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string fileName);
    }
}