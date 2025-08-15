using SmartCharging.ServiceDefaults.Constants;
using SmartCharging.TestsShared.Fixtures;
using SmartCharging.TestsShared.MockServers;
using SmartCharging.TestsShared.TestBases;
using SmartChargingApi;
using SmartChargingApi.Shared.Data;

namespace SmartCharging.EndToEndTests;

[Collection(SmartChargingEndToEndTestCollection.Name)]
public class SmartChargingEndToEndTestBase(SharedFixture<SmartChargingMetadata, SmartChargingDbContext> sharedFixture)
    : EndToEndTestBase<SmartChargingMetadata, SmartChargingDbContext>(sharedFixture)
{
    // The inner works of that external API is not in the scope of our integration tests.
    // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
    private GroupStatisticsExternalServiceMock? _groupStatisticsExternalServiceMock;

    public GroupStatisticsExternalServiceMock GroupStatisticsExternalServiceMock =>
        _groupStatisticsExternalServiceMock ??= new GroupStatisticsExternalServiceMock(SharedFixture.WireMockServer);

    // We don't need to inject `CustomersServiceMockServersFixture` class fixture in the constructor because it initialized by `collection fixture` and its static properties are accessible in the codes
    // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
    // note1: for E2E test we use real identity service in on a TestContainer docker of this service, coordination with an external system is necessary in E2E
    // note2: add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
    // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
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
