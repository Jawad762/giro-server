using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Giro.Api.Models
{
    public enum UserRole
    {
        Driver,
        Rider
    }

    public class UserModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public string Role { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class DriverCacheModel
    {
        public string ConnectionId { get; set; }
        public decimal Lat { get; set; }
        public decimal Long { get; set; }
    }

}