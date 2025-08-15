using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartChargingApi.Groups.Models;
using SmartChargingApi.Groups.Models.ValueObjects;

namespace SmartChargingApi.Shared.Data.EntityConfigurations;

public class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.ToTable(nameof(Connector).Pluralize().Underscore());

        builder
            .Property(cc => cc.Id)
            // Id is not based on StronglyTypedIdValueConverterSelector because it is `int` and not `Guid`
            .HasConversion(id => id.Value, value => ConnectorId.Of(value))
            .ValueGeneratedNever()
            .IsRequired();

        // Define a composite primary key: ChargeStationId + ConnectorId
        // Connector has integer Identifier unique within the context of a charge station
        builder.HasKey(c => new { c.ChargeStationId, c.Id });

        builder.Property(c => c.ChargeStationId).IsRequired();

        // Owned type configuration for MaxCurrentInAmps
        builder.OwnsOne(
            c => c.MaxCurrentInAmps,
            mc =>
            {
                mc.Property(p => p.Value).HasColumnName(nameof(Connector.MaxCurrentInAmps).Underscore());
            }
        );
    }
}
