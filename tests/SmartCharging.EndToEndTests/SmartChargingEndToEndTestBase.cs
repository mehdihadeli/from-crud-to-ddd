using SmartCharging.Shared.Application.Data;
using SmartCharging.TestsShared.Fixtures;
using SmartCharging.TestsShared.TestBases;
using Xunit.Abstractions;

namespace SmartCharging.EndToEndTests;

[Collection(SmartChargingEndToEndTestCollection.Name)]
public class SmartChargingEndToEndTestBase(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : EndToEndTestBase<SmartChargingMetadata, SmartChargingDbContext>(sharedFixture, outputHelper)
{
    protected override void OverrideInMemoryConfig(IDictionary<string, string> keyValues)
    {
        keyValues.Add("ConnectionStrings:SmartCharging_DB", SharedFixture.PostgresContainerFixture.ConnectionString);
    }
}
