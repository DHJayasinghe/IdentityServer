using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Data.Configurations
{
    public sealed class RefreshTokenConfig : EntityConfiguration<RefreshToken>
    {
        public override void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshToken");

            builder.HasKey(e => e.Id);

            builder.Property(d => d.Id)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasDefaultValueSql("newid()");

            builder.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);

            builder.Property(e => e.Expires)
                .IsRequired()
                .HasColumnType("datetime");

            builder.Property(e => e.Created)
                .IsRequired()
                .HasColumnType("datetime");

            builder.Property(e => e.CreatedByIp)
                .HasMaxLength(250)
                .IsUnicode(false);

            builder.Property(e => e.Revoked)
                .HasColumnType("datetime");

            builder.Property(e => e.RevokedByIp)
                .HasMaxLength(250)
                .IsUnicode(false);

            builder.Property(e => e.ReplacedByToken)
                .HasMaxLength(250)
                .IsUnicode(false);

            builder.HasOne(e => e.AppUser)
               .WithMany(e => e.RefreshTokens)
               .HasForeignKey(e => e.AppUserId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_AppUser_RefreshToken");

            base.Configure(builder);
        }
    }
}
