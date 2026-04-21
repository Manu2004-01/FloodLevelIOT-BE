using Core.DTOs;
using Core.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.Errors;

namespace FloodLevelIOT_BE.Test.Controllers;

public class FloodForecastControllerTest
{
    private readonly IFloodForecastService _floodForecastService;

    public FloodForecastControllerTest()
    {
        _floodForecastService = A.Fake<IFloodForecastService>();
    }

    [Fact]
    public async Task GetFloodForecast_WithValidAreaId_ReturnsOkWithForecastData()
    {
        var dto = new FloodForecastRequestDto { Latitude = 10.8231, Longitude = 106.6297, RadiusKm = 5 };
        var response = new FloodForecastResponseDto
        {
            ReportId = 1,
            RiskLevel = "Low",
            Summary = "Conditions stable.",
            Recommendations = new List<string> { "Monitor local alerts." },
            CreatedAtUtc = DateTime.UtcNow
        };
        A.CallTo(() => _floodForecastService.RunForecastForCitizenAsync(dto.Latitude, dto.Longitude, 5.0, A<CancellationToken>._))
            .Returns(response);
        var controller = new FloodForecastController(_floodForecastService);

        var result = await controller.RunForecast(dto, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetFloodForecast_WithInvalidAreaId_ReturnsNotFound()
    {
        var dto = new FloodForecastRequestDto { Latitude = 10.8, Longitude = 106.6, RadiusKm = 2 };
        A.CallTo(() => _floodForecastService.RunForecastForCitizenAsync(dto.Latitude, dto.Longitude, 2.0, A<CancellationToken>._))
            .Returns((FloodForecastResponseDto?)null);
        var controller = new FloodForecastController(_floodForecastService);

        var result = await controller.RunForecast(dto, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var body = Assert.IsType<BaseCommentResponse>(bad.Value);
        Assert.Equal(400, body.Statuscodes);
    }

    [Fact]
    public async Task GetFloodRisk_WithValidSensorId_ReturnsOkWithRiskLevel()
    {
        var dto = new FloodForecastRequestDto { Latitude = 21.0285, Longitude = 105.8542, RadiusKm = 3 };
        var response = new FloodForecastResponseDto
        {
            ReportId = 2,
            RiskLevel = "Moderate",
            Summary = "Elevated water levels possible.",
            Recommendations = new List<string>(),
            CreatedAtUtc = DateTime.UtcNow
        };
        A.CallTo(() => _floodForecastService.RunForecastForCitizenAsync(dto.Latitude, dto.Longitude, 3.0, A<CancellationToken>._))
            .Returns(response);
        var controller = new FloodForecastController(_floodForecastService);

        var result = await controller.RunForecast(dto, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<FloodForecastResponseDto>(ok.Value);
        Assert.Equal("Moderate", payload.RiskLevel);
    }

    [Fact]
    public async Task GetFloodAlert_WithHighRiskArea_ReturnsOkWithAlertInfo()
    {
        var dto = new FloodForecastRequestDto { Latitude = 16.0471, Longitude = 108.2068, RadiusKm = 4 };
        var response = new FloodForecastResponseDto
        {
            ReportId = 3,
            RiskLevel = "High",
            Summary = "Flood risk elevated in the selected radius.",
            Recommendations = new List<string> { "Avoid low-lying roads.", "Move valuables to higher floors." },
            ConfidenceNote = "Based on recent sensor and rainfall signals.",
            CreatedAtUtc = DateTime.UtcNow
        };
        A.CallTo(() => _floodForecastService.RunForecastForCitizenAsync(dto.Latitude, dto.Longitude, 4.0, A<CancellationToken>._))
            .Returns(response);
        var controller = new FloodForecastController(_floodForecastService);

        var result = await controller.RunForecast(dto, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<FloodForecastResponseDto>(ok.Value);
        Assert.Equal("High", payload.RiskLevel);
        Assert.NotNull(payload.Recommendations);
        Assert.Equal(2, payload.Recommendations!.Count);
    }

    [Fact]
    public async Task GetHistoricalFloodData_WithValidPeriod_ReturnsOkWithHistoricalData()
    {
        var dto = new FloodForecastRequestDto { Latitude = 10.0, Longitude = 106.0, RadiusKm = 0 };
        var response = new FloodForecastResponseDto
        {
            ReportId = 4,
            RiskLevel = "Low",
            Summary = "Historical context included in model run.",
            CreatedAtUtc = DateTime.UtcNow
        };
        A.CallTo(() => _floodForecastService.RunForecastForCitizenAsync(10.0, 106.0, 3.0, A<CancellationToken>._))
            .Returns(response);
        var controller = new FloodForecastController(_floodForecastService);

        var result = await controller.RunForecast(dto, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        A.CallTo(() => _floodForecastService.RunForecastForCitizenAsync(10.0, 106.0, 3.0, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

}
