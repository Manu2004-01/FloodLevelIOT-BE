using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using WebAPI.Controllers;
using WebAPI.Errors;

namespace FloodLevelIOT_BE.Test.Controllers;

public class LocationControllerTest
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocationRepository _locationRepository;

    public LocationControllerTest()
    {
        _locationRepository = A.Fake<ILocationRepository>();
        _mapper = A.Fake<IMapper>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        A.CallTo(() => _unitOfWork.LocationRepository).Returns(_locationRepository);
    }

    // Get All Locations Tests (4 tests)
    [Fact]
    public async Task GetAllLocations_WhenLocationsExist_ReturnsOkWithLocationsList()
    {
        var locations = new List<Location>
        {
            new()
            {
                PlaceId = 1,
                Title = "Point A",
                Address = "Addr",
                Latitude = 10.5m,
                Longitude = 106.5m,
                Area = new Area { AreaId = 1, AreaName = "District 1" }
            }
        };
        var dtos = (IReadOnlyList<LocationDTO>)new List<LocationDTO>
        {
            new()
            {
                PlaceId = 1,
                AreaName = "District 1",
                Title = "Point A",
                Address = "Addr",
                Latitude = 10.5,
                Longitude = 106.5
            }
        };
        A.CallTo(() => _locationRepository.GetAllAsync(A<Expression<Func<Location, object>>[]>.Ignored))
            .Returns(locations);
        A.CallTo(() => _mapper.Map<IReadOnlyList<Location>, IReadOnlyList<LocationDTO>>(A<IReadOnlyList<Location>>._))
            .Returns(dtos);
        var controller = new LocationController(_unitOfWork, _mapper);

        var result = await controller.GetAllLocations();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(dtos, ok.Value);
    }

    [Fact]
    public async Task GetAllLocations_WhenLocationsHaveArea_ReturnsOkWithAreaInfo()
    {
        var locations = new List<Location>
        {
            new()
            {
                PlaceId = 2,
                Title = "B",
                Address = "Road 1",
                Latitude = 1m,
                Longitude = 2m,
                Area = new Area { AreaId = 3, AreaName = "Thu Duc" }
            }
        };
        var dtos = (IReadOnlyList<LocationDTO>)new List<LocationDTO>
        {
            new() { PlaceId = 2, AreaName = "Thu Duc", Title = "B", Address = "Road 1", Latitude = 1, Longitude = 2 }
        };
        A.CallTo(() => _locationRepository.GetAllAsync(A<Expression<Func<Location, object>>[]>.Ignored))
            .Returns(locations);
        A.CallTo(() => _mapper.Map<IReadOnlyList<Location>, IReadOnlyList<LocationDTO>>(A<IReadOnlyList<Location>>._))
            .Returns(dtos);
        var controller = new LocationController(_unitOfWork, _mapper);

        var result = await controller.GetAllLocations();

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<LocationDTO>>(ok.Value);
        Assert.Single(payload);
        Assert.Equal("Thu Duc", payload[0].AreaName);
    }

    [Fact]
    public async Task GetAllLocations_WhenNoLocationsExist_ReturnsNotFound()
    {
        A.CallTo(() => _locationRepository.GetAllAsync(A<Expression<Func<Location, object>>[]>.Ignored))
            .Returns(new List<Location>());
        var controller = new LocationController(_unitOfWork, _mapper);

        var result = await controller.GetAllLocations();

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BaseCommentResponse>(notFound.Value);
        Assert.Equal(404, response.Statuscodes);
    }

    [Fact]
    public async Task GetAllLocations_WhenDatabaseError_ReturnsInternalServerError()
    {
        A.CallTo(() => _locationRepository.GetAllAsync(A<Expression<Func<Location, object>>[]>.Ignored))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var controller = new LocationController(_unitOfWork, _mapper);

        var result = await controller.GetAllLocations();

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
        var response = Assert.IsType<BaseCommentResponse>(error.Value);
        Assert.Equal(500, response.Statuscodes);
    }
}
