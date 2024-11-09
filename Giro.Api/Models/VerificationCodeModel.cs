namespace Giro.Api.Models
{
    public class VerificationCodeModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public int UserId { get; set; }
    }
}
