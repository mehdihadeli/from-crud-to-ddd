using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;

namespace SmartCharging.EndToEndTests;

[CollectionDefinition(Name)]
public class SmartChargingEndToEndTestCollection
    : ICollectionFixture<SharedFixture<SmartChargingMetadata, SmartChargingDbContext>>
{
    public const string Name = "End-To-End Test";
}
