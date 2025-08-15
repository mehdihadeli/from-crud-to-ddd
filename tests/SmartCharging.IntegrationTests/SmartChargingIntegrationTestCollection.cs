using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests;

[CollectionDefinition(Name)]
public class SmartChargingIntegrationTestCollection
    : ICollectionFixture<SharedFixture<SmartChargingMetadata, SmartChargingDbContext>>
{
    public const string Name = "Smart Charging Integration Test";
}
