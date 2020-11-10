using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Data.Configurations
{
    public sealed class AppUserConfig : EntityConfiguration<AppUser>
    {
        public override void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("AppUser");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName)
                //.IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(e => e.LastName)
                //.IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(false);

            builder.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);

            builder.Property(e=>e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);

            builder.Property(e => e.EmailConfirmed)
                .HasDefaultValue(0);

            builder.Property(e => e.LockoutEnabled)
                .HasDefaultValue(0);

            builder.Property(e => e.LockoutEndDateTimeUtc)
                .HasColumnType("datetime");

            builder.Property(e => e.AccessFailedCount)
                .HasDefaultValue(0);

            builder.Property(e => e.CreatedDateTimeUtc)
                .HasColumnType("datetime");

            builder.Property(e => e.ModifiedDateTimeUtc)
                .HasColumnType("datetime");

            base.Configure(builder);
        }
    }
}
