using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Data.Configurations
{
    public sealed class AppGroupPermissionConfig : EntityConfiguration<AppGroupPermission>
    {
        public override void Configure(EntityTypeBuilder<AppGroupPermission> builder)
        {
            builder.ToTable("AppGroupPermission");

            builder.HasKey(e => e.Id);

            builder.Property(d => d.Id)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasDefaultValueSql("newid()");

            builder.Ignore(e => e.Deleted);
            builder.Ignore(e => e.Inserted);

            builder.HasOne(e => e.AppGroup)
                .WithMany(e => e.GroupPermissions)
                .HasForeignKey(e => e.AppGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppGroup_GroupPermission");

            builder.HasOne(e => e.Permission)
               .WithMany(e => e.GroupPermissions)
               .HasForeignKey(e => e.PermissionId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_AppPermission_GroupPermission");

            base.Configure(builder);
        }
    }
}
