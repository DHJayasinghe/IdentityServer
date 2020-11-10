using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Data.Configurations
{
    public sealed class AppPermissionConfig : EntityConfiguration<AppPermission>
    {
        public override void Configure(EntityTypeBuilder<AppPermission> builder)
        {
            builder.ToTable("AppPermission");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(50)
               .IsUnicode(false);

            builder.Property(e => e.Description)
               .IsRequired()
               .HasMaxLength(250)
               .IsUnicode(false);

            base.Configure(builder);
        }
    }
}
