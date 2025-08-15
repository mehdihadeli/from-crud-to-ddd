using System.Net;
using SmartChargingApi.Groups.Contracts;
using SmartChargingApi.Groups.Dtos;
using SmartChargingApi.Groups.Dtos.Clients;

namespace SmartChargingApi.Groups.Services;

public class GroupStatisticsExternalProvider(HttpClient httpClient) : IGroupStatisticsExternalProvider
{
    private const string CapacityStatsPath = "/api/v1/group-statistics/group-capacity";
    private const string EnergyStatsPath = "/api/v1/group-statistics/group-energy";

    public async Task<GroupCapacityStatisticsDto?> GetCapacityStatisticsAsync(Guid groupId)
    {
        var url = $"{CapacityStatsPath}/{groupId}";
        var response = await httpClient.GetAsync(url);

        // here because notfound item for us is not an exception and item is not mandatory, we return null for that
        if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var clientDto = await response.Content.ReadFromJsonAsync<GroupCapacityStatisticsClientResponseDto>();
        return clientDto == null
            ? null
            : new GroupCapacityStatisticsDto(
                clientDto.CurrentLoadAmps,
                clientDto.MaxCapacityAmps,
                clientDto.AvailableCapacityAmps
            );
    }

    public async Task<GroupEnergyConsumptionDto?> GetEnergyConsumptionAsync(Guid groupId)
    {
        var url = $"{EnergyStatsPath}/{groupId}";
        var response = await httpClient.GetAsync(url);

        // here because notfound item for us is not an exception and item is not mandatory, we return null for that
        if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var clientDto = await response.Content.ReadFromJsonAsync<GroupEnergyConsumptionClientResponseDto>();
        return clientDto == null
            ? null
            : new GroupEnergyConsumptionDto(clientDto.EnergyUsedKWh, clientDto.PeriodStart, clientDto.PeriodEnd);
    }
}
