using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartCharging.TestsShared.TestBases;
using Xunit.Abstractions;

namespace SmartCharging.IntegrationTests;

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(SmartChargingIntegrationTestCollection.Name)]
public class SmartChargingIntegrationTestBase(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : IntegrationTestBase<SmartChargingMetadata, SmartChargingDbContext>(sharedFixture, outputHelper)
{
    protected override void OverrideInMemoryConfig(IDictionary<string, string> keyValues)
    {
        keyValues.Add("ConnectionStrings:SmartCharging_DB", SharedFixture.PostgresContainerFixture.ConnectionString);
    }
}
