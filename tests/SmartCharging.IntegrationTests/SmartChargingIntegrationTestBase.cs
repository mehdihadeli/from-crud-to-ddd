using SmartCharging.ServiceDefaults.Constants;
using SmartCharging.TestsShared.Fixtures;
using SmartCharging.TestsShared.MockServers;
using SmartCharging.TestsShared.TestBases;
using SmartChargingApi;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.IntegrationTests;

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(SmartChargingIntegrationTestCollection.Name)]
public class SmartChargingIntegrationTestBase(
    SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture
) : IntegrationTestBase<SmartChargingMetadata, SmartChargingDbContext>(sharedFixture)
{
    // The inner works of that external API is not in the scope of our integration tests.
    // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
    private GroupStatisticsExternalServiceMock? _groupStatisticsExternalServiceMock;

    public GroupStatisticsExternalServiceMock GroupStatisticsExternalServiceMock =>
        _groupStatisticsExternalServiceMock ??= new GroupStatisticsExternalServiceMock(SharedFixture.WireMockServer);

    protected override void OverrideInMemoryConfig(IDictionary<string, string> keyValues)
    {
        keyValues.Add(
            $"Services:{AspireApplicationResources.Api.SmartChargingStatisticsApi}:http:0",
            SharedFixture.WireMockServerUrl
        );
        keyValues.Add(
            $"Services:{AspireApplicationResources.Api.SmartChargingStatisticsApi}:https:0",
            SharedFixture.WireMockServerUrl
        );
    }
}
