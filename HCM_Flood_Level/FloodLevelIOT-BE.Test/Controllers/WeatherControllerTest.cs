using Core.DTOs;
using Core.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.Errors;

namespace FloodLevelIOT_BE.Test.Controllers;

public class WeatherControllerTest
{
    private readonly IOpenWeatherService _openWeatherService;

    public WeatherControllerTest()
    {
        _openWeatherService = A.Fake<IOpenWeatherService>();
    }

    private static CurrentWeatherDTO SampleWeather(double lat, double lon) =>
        new()
        {
            Lat = lat,
            Lon = lon,
            LocationName = "Ho Chi Minh City",
            TemperatureC = 30,
            FeelsLikeC = 32,
            HumidityPercent = 70,
            PressureHpa = 1010,
            WindSpeedMps = 3.5,
            WindDeg = 180,
            CloudsPercent = 40,
            WeatherMain = "Clouds",
            WeatherDescription = "scattered clouds",
            DataCalculatedUnixUtc = 1700000000,
            TimezoneOffsetSeconds = 25200
        };

    [Fact]
    public async Task GetWeatherForecast_ReturnsOkWithWeatherData()
    {
        var lat = 10.8231;
        var lon = 106.6297;
        var dto = SampleWeather(lat, lon);
        A.CallTo(() => _openWeatherService.GetCurrentByCoordinatesAsync(lat, lon, A<CancellationToken>._))
            .Returns(dto);
        var controller = new WeatherController(_openWeatherService);

        var result = await controller.GetCurrent(lat, lon, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(dto, ok.Value);
    }

    [Fact]
    public async Task GetWeatherByLocation_WithValidCoordinates_ReturnsOkWithWeatherInfo()
    {
        const double lat = 21.0285;
        const double lon = 105.8542;
        var dto = SampleWeather(lat, lon);
        A.CallTo(() => _openWeatherService.GetCurrentByCoordinatesAsync(lat, lon, A<CancellationToken>._))
            .Returns(dto);
        var controller = new WeatherController(_openWeatherService);

        var result = await controller.GetCurrent(lat, lon, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        A.CallTo(() => _openWeatherService.GetCurrentByCoordinatesAsync(lat, lon, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetWeatherAlert_WithValidLocation_ReturnsOkWithAlerts()
    {
        var lat = 16.0471;
        var lon = 108.2068;
        var dto = SampleWeather(lat, lon);
        A.CallTo(() => _openWeatherService.GetCurrentByCoordinatesAsync(lat, lon, A<CancellationToken>._))
            .Returns(dto);
        var controller = new WeatherController(_openWeatherService);
        using var cts = new CancellationTokenSource();

        var result = await controller.GetCurrent(lat, lon, cts.Token);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<CurrentWeatherDTO>(ok.Value);
        Assert.Equal(lat, payload.Lat, precision: 4);
        Assert.Equal(lon, payload.Lon, precision: 4);
    }

    [Fact]
    public async Task GetWeatherAlert_WithInvalidLocation_ReturnsNotFound()
    {
        var lat = 10.0;
        var lon = 106.0;
        A.CallTo(() => _openWeatherService.GetCurrentByCoordinatesAsync(lat, lon, A<CancellationToken>._))
            .Returns((CurrentWeatherDTO?)null);
        var controller = new WeatherController(_openWeatherService);

        var result = await controller.GetCurrent(lat, lon, CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, error.StatusCode);
        var body = Assert.IsType<BaseCommentResponse>(error.Value);
        Assert.Equal(502, body.Statuscodes);
    }

}
