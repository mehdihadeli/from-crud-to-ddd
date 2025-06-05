using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCharging.Groups.Models;

namespace SmartCharging.Shared.Application.Data.EntityConfigurations;

public class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.MaxCurrentInAmps).IsRequired();

        builder.Property(c => c.ChargeStationId).IsRequired();
    }
}
