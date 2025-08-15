using SmartCharging.Shared.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartChargingApi;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.EndToEndTests;

// https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection, but it can implements multiple ICollectionFixture in its definitions
[CollectionDefinition(Name)]
public class SmartChargingEndToEndTestCollection
    : ICollectionFixture<SharedFixture<SmartChargingMetadata, SmartChargingDbContext>>
{
    public const string Name = "End-To-End Test";
}
