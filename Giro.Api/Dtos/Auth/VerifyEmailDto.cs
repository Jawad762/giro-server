namespace Giro.Api.Dtos.Auth
{
    public class VerifyEmailRequestDto
    {
        public int UserId { get; set; }
        public int Code { get; set; }
    }
}
