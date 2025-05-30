using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCharging.Groups.Models;
using SmartCharging.Groups.Models.ValueObjects;

namespace SmartCharging.Shared.Application.Data.EntityConfigurations;

public class ChargeStationConfiguration : IEntityTypeConfiguration<ChargeStation>
{
    public void Configure(EntityTypeBuilder<ChargeStation> builder)
    {
        builder.ToTable(nameof(ChargeStation).Pluralize().Underscore());

        // Primary key - automatically handled by StronglyTypedIdValueConverterSelector<Guid>
        builder.Property(cs => cs.Id).ValueGeneratedNever().IsRequired();
        builder.HasKey(cs => cs.Id);

        // OwnsOne calling ValueObject private constructor, for skipping value object validation during ef core materialization process,
        // this is not possible with using `builder.Property` because we can't call private constructor directly and we should use `Of`.
        builder.OwnsOne(
            cs => cs.Name,
            name =>
            {
                name.Property(p => p.Value).HasColumnName(nameof(ChargeStation.Name).Underscore()).IsRequired();
            }
        );

        builder
            .HasMany(g => g.Connectors)
            .WithOne()
            // Explicit relationship
            .HasForeignKey(c => c.ChargeStationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
