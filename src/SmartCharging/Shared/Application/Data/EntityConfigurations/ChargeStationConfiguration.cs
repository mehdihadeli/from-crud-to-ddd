using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCharging.Groups.Models;

namespace SmartCharging.Shared.Application.Data.EntityConfigurations;

public class ChargeStationConfiguration : IEntityTypeConfiguration<ChargeStation>
{
    public void Configure(EntityTypeBuilder<ChargeStation> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Name).IsRequired().HasMaxLength(100);

        builder.Property(c => c.GroupId).IsRequired();

        builder
            .HasMany(cs => cs.Connectors)
            .WithOne()
            .HasForeignKey(connector => connector.ChargeStationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
