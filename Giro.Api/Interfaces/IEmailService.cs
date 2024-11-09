namespace Giro.Api.Interfaces
{
    public interface IEmailService
    {
        public Task SendEmailConfirmationAsync(string toEmail, int code);
    }
}
