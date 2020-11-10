using Identity.API.Data.Configurations;
using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data
{
    public partial class IdentityDBContext : DbContext
    {
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<AppPermission> AppPermission { get; set; }
        public DbSet<AppGroup> AppGroup { get; set; }
        public DbSet<AppGroupPermission> AppGroupPermission { get; set; }
        public DbSet<AppUserGroup> AppUserGroup { get; set; }

        public IdentityDBContext() { }

        public IdentityDBContext(DbContextOptions<IdentityDBContext> options) : base(options) { }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //        optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=AmanaTakaful_IdentityDB;Trusted_Connection=True;");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AppUserConfig());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfig());
            modelBuilder.ApplyConfiguration(new AppPermissionConfig());
            modelBuilder.ApplyConfiguration(new AppGroupConfig());
            modelBuilder.ApplyConfiguration(new AppGroupPermissionConfig());
            modelBuilder.ApplyConfiguration(new AppUserGroupConfig());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
