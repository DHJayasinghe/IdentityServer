using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Data.Configurations
{
    public sealed class AppUserGroupConfig : EntityConfiguration<AppUserGroup>
    {
        public override void Configure(EntityTypeBuilder<AppUserGroup> builder)
        {
            builder.ToTable("AppUserGroup");

            builder.HasKey(e => e.Id);

            builder.Ignore(e => e.Inserted);

            builder.Ignore(e => e.Deleted);

            builder.Property(d => d.Id)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasDefaultValueSql("newid()");

            builder.HasOne(e => e.AppUser)
                .WithMany(e => e.UserGroups)
                .HasForeignKey(e => e.AppUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppUser_UserGroup");

            builder.HasOne(e => e.AppGroup)
               .WithMany(e => e.UserGroups)
               .HasForeignKey(e => e.AppGroupId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_AppGroup_UserGroup");

            base.Configure(builder);
        }
    }
}
