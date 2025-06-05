using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCharging.Groups.Models;

namespace SmartCharging.Shared.Application.Data.EntityConfigurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);

        builder.Property(g => g.CapacityInAmps).IsRequired();

        builder
            .HasMany(g => g.ChargeStations)
            .WithOne()
            .HasForeignKey(station => station.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
