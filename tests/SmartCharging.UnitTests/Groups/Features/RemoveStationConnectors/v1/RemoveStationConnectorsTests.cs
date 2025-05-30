using SmartCharging.Groups.Features.RemoveStationConnectors.v1;
using SmartCharging.Shared.BuildingBlocks.Exceptions;

namespace SmartCharging.UnitTests.Groups.Features.RemoveStationConnectors.v1;

public class RemoveStationConnectorsTests
{
    [Fact]
    public void Of_WithValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var request = new RemoveStationConnectorsRequest(new List<int> { 1, 2 });

        // Act
        var result = SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors.Of(
            groupId,
            chargeStationId,
            request
        );

        // Assert
        result.ShouldNotBeNull();
        result.GroupId.Value.ShouldBe(groupId);
        result.ChargeStationId.Value.ShouldBe(chargeStationId);
        result.ConnectorIds.ShouldNotBeNull();
        result.ConnectorIds.Select(c => c.Value).ShouldBe(request.ConnectorIds);
    }

    [Fact]
    public void Of_WithNullGroupId_ThrowsValidationException()
    {
        // Arrange
        Guid? nullGroupId = null;
        var chargeStationId = Guid.NewGuid();
        var request = new RemoveStationConnectorsRequest(new List<int> { 1, 2 });

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors.Of(
                    nullGroupId,
                    chargeStationId,
                    request
                )
            )
            .Message.ShouldContain("groupId cannot be null or empty.");
    }

    [Fact]
    public void Of_WithNullChargeStationId_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        Guid? nullChargeStationId = null;
        var request = new RemoveStationConnectorsRequest(new List<int> { 1, 2 });

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors.Of(
                    groupId,
                    nullChargeStationId,
                    request
                )
            )
            .Message.ShouldContain("chargeStationId cannot be null or empty.");
    }

    [Fact]
    public void Of_WithNullRequest_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        RemoveStationConnectorsRequest? nullRequest = null;

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors.Of(
                    groupId,
                    chargeStationId,
                    nullRequest
                )
            )
            .Message.ShouldContain("request cannot be null or empty.");
    }

    [Fact]
    public void Of_WithNullConnectorIds_ThrowsValidationException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var chargeStationId = Guid.NewGuid();
        var request = new RemoveStationConnectorsRequest(null);

        // Act & Assert
        Should
            .Throw<ValidationException>(() =>
                SmartCharging.Groups.Features.RemoveStationConnectors.v1.RemoveStationConnectors.Of(
                    groupId,
                    chargeStationId,
                    request
                )
            )
            .Message.ShouldContain("request.ConnectorIds cannot be null or empty.");
    }
}
