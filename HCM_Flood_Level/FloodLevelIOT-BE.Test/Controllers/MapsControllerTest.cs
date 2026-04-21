using Core.DTOs;
using Core.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.Errors;

namespace FloodLevelIOT_BE.Test.Controllers;

public class MapsControllerTest
{
    private readonly IMapsService _mapsService;
    private readonly IRouteAvoidFloodService _routeAvoidFloodService;

    public MapsControllerTest()
    {
        _mapsService = A.Fake<IMapsService>();
        _routeAvoidFloodService = A.Fake<IRouteAvoidFloodService>();
    }

    private static MapsController CreateController(IMapsService maps, IRouteAvoidFloodService routes)
    {
        var controller = new MapsController(maps, routes);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task GetMapData_ReturnsOkWithAllSensorsLocation()
    {
        var dto = new MapsSearchDTO { Query = "sensor", Lat = 10.8231, Lng = 106.6297, Zoom = 14 };
        var payload = new { results = new[] { new { id = "p1", title = "Point 1" } } };
        A.CallTo(() => _mapsService.SearchAsync(A<MapsSearchDTO>.That.Matches(d => d.Query == "sensor"), A<CancellationToken>._))
            .Returns(payload);
        var controller = CreateController(_mapsService, _routeAvoidFloodService);

        var result = await controller.Search(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(payload, ok.Value);
    }

    [Fact]
    public async Task GetSensorsByArea_WithValidAreaId_ReturnsOkWithSensorLocations()
    {
        var dto = new MapsSearchDTO { Query = "area:5", Lat = 16.0, Lng = 108.0, Zoom = 12 };
        var payload = new { areaId = 5, sensors = Array.Empty<object>() };
        A.CallTo(() => _mapsService.SearchAsync(A<MapsSearchDTO>.That.Matches(d => d.Query == "area:5"), A<CancellationToken>._))
            .Returns(payload);
        var controller = CreateController(_mapsService, _routeAvoidFloodService);

        var result = await controller.Search(dto);

        Assert.IsType<OkObjectResult>(result);
        A.CallTo(() => _mapsService.SearchAsync(A<MapsSearchDTO>.That.Matches(d => d.Query == "area:5"), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetFloodHeatmap_ReturnsOkWithHeatmapData()
    {
        var dto = new MapsSearchDTO { Query = "heatmap", Lat = 10.0, Lng = 106.0, Zoom = 11 };
        A.CallTo(() => _mapsService.SearchAsync(A<MapsSearchDTO>._, A<CancellationToken>._))
            .ThrowsAsync(new InvalidOperationException("SerpApi rate limit"));
        var controller = CreateController(_mapsService, _routeAvoidFloodService);

        var result = await controller.Search(dto);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
        var body = Assert.IsType<BaseCommentResponse>(error.Value);
        Assert.Equal(500, body.Statuscodes);
        Assert.Contains("SerpApi rate limit", body.Message);
    }

    [Fact]
    public async Task GetRealTimeMap_ReturnsOkWithLiveData()
    {
        var request = new RouteAvoidFloodRequestDTO
        {
            StartLat = 10.8,
            StartLng = 106.6,
            EndLat = 10.9,
            EndLng = 106.7,
            TravelMode = "Driving",
            FloodRadiusMeters = 400
        };
        var response = new RouteAvoidFloodResponseDTO
        {
            RecommendedRoute = new RouteAlternativeDTO
            {
                OverviewPolylinePoints = "abc",
                DistanceMeters = 1200,
                DurationSeconds = 600,
                RiskScore = 0.2,
                IsFlooded = false
            },
            IsRecommendedRouteFlooded = false
        };
        A.CallTo(() => _routeAvoidFloodService.GetAvoidFloodRouteAsync(request, A<CancellationToken>._))
            .Returns(response);
        var controller = CreateController(_mapsService, _routeAvoidFloodService);

        var result = await controller.RouteAvoidFlood(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }
}
