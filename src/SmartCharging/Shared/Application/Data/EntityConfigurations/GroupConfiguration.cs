using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCharging.Groups.Models;

namespace SmartCharging.Shared.Application.Data.EntityConfigurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable(nameof(Group).Pluralize().Underscore());

        // Primary key - automatically handled by StronglyTypedIdValueConverterSelector<Guid>
        builder.Property(g => g.Id).ValueGeneratedNever().IsRequired();
        builder.HasKey(g => g.Id);

        // OwnsOne calling ValueObject private constructor, for skipping value object validation during ef core materialization process,
        // this is not possible with using `builder.Property` because we can't call private constructor directly and we should use `Of`.
        builder.OwnsOne(
            g => g.Name,
            a =>
            {
                // Configuration just for changing the column name in db (instead of name_value)
                a.Property(p => p.Value).HasColumnName(nameof(Group.Name).Underscore()).IsRequired();
            }
        );

        // OwnsOne calling ValueObject private constructor, for skipping value object validation during ef core materialization process
        builder.OwnsOne(
            g => g.CapacityInAmps,
            a =>
            {
                a.Property(p => p.Value).HasColumnName(nameof(Group.CapacityInAmps).Underscore()).IsRequired();
            }
        );

        // Configures the one-to-many relationship between Group and ChargeStation
        // - Creates a shadow property 'group_id' on the ChargeStation entity (dependent side)
        // - This shadow property will:
        //   * Be of type Guid (matching GroupId.Value's type)
        //   * Serve as the foreign key to Group's primary key
        //   * Not be visible in the domain model (only managed by EF Core)
        // - Cascade delete ensures ChargeStations are automatically deleted when their Group is deleted
        builder
            .HasMany(g => g.ChargeStations)
            .WithOne()
            // Explicit relationship
            .HasForeignKey(c => c.GroupId)
            .OnDelete(DeleteBehavior.Cascade); // Auto-delete stations when group is deleted
    }
}
