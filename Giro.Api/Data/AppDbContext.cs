using Giro.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Giro.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RevokedTokensModel> RevokedTokens { get; set; }
        public DbSet<VerificationCodeModel> VerificationCodes { get; set; }
        public DbSet<VehicleModel> Vehicles { get; set; }
    }
}
