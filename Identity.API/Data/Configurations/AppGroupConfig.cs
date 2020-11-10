using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.API.Data.Configurations
{
    public sealed class AppGroupConfig : EntityConfiguration<AppGroup>
    {
        public override void Configure(EntityTypeBuilder<AppGroup> builder)
        {
            builder.ToTable("AppGroup");

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
