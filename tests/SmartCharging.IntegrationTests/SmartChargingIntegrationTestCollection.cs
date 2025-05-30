using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;

namespace SmartCharging.IntegrationTests;

[CollectionDefinition(Name)]
public class SmartChargingIntegrationTestCollection
    : ICollectionFixture<SharedFixture<SmartChargingMetadata, SmartChargingDbContext>>
{
    public const string Name = "Smart Charging Integration Test";
}
