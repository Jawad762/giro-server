namespace Giro.Api.Models
{
    public class RevokedTokensModel
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}
