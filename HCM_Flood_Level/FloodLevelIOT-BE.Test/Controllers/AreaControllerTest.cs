using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.Errors;

namespace FloodLevelIOT_BE.Test.Controllers;

public class AreaControllerTest
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAreaRepository _areaRepository;
    private readonly IMapper _mapper;

    public AreaControllerTest()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _areaRepository = A.Fake<IAreaRepository>();
        _mapper = A.Fake<IMapper>();
        A.CallTo(() => _unitOfWork.AreaRepository).Returns(_areaRepository);
    }

    // Get All Areas Tests (2 tests)
    [Fact]
    public async Task GetAllAreas_WhenAreasExist_ReturnsOkWithAreasList()
    {
        //Arrange
        var areas = new List<Area> { new Area() };
        A.CallTo(() => _areaRepository.GetAllAsync()).Returns(areas);
        var controller = new AreaController(_unitOfWork, _mapper);

        //Act
        var result = await controller.GetAll();

        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(areas, okResult.Value);
    }

    [Fact]
    public async Task GetAllAreas_WhenNoAreasExist_ReturnsNotFound()
    {
        // Arrange
        A.CallTo(() => _areaRepository.GetAllAsync()).Returns(Task.FromResult<IReadOnlyList<Area>>(null!));
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetAll();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<BaseCommentResponse>(notFoundResult.Value);
        Assert.Equal(404, response.Statuscodes);
    }

    [Fact]
    public async Task GetAllAreas_WhenAreasListIsEmpty_ReturnsOkWithEmptyList()
    {
        // Arrange
        var emptyAreas = new List<Area>();
        A.CallTo(() => _areaRepository.GetAllAsync()).Returns(emptyAreas);
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<IReadOnlyList<Area>>(okResult.Value);
        Assert.Empty(value);
    }

    [Fact]
    public async Task GetAllAreas_WhenAreasListHasMultipleItems_ReturnsOkWithAreasList()
    {
        // Arrange
        var areas = new List<Area> { new Area(), new Area(), new Area() };
        A.CallTo(() => _areaRepository.GetAllAsync()).Returns(areas);
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = Assert.IsAssignableFrom<IReadOnlyList<Area>>(okResult.Value);
        Assert.Equal(3, value.Count);
    }

    [Fact]
    public async Task GetAllAreas_WhenRepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        A.CallTo(() => _areaRepository.GetAllAsync()).Throws(new Exception("Database error"));
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetAll();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Get Area Details Tests (3 tests)
    [Fact]
    public async Task GetAreaDetails_WithValidAreaId_ReturnsOkWithAreaSensorReadings()
    {
        // Arrange
        var areaId = 1;
        var readings = new List<AreaDTO> { new AreaDTO { AreaId = areaId, SensorId = 10 } };
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, A<CancellationToken>._))
            .Returns(readings);
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(areaId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(readings, okResult.Value);
    }

    [Fact]
    public async Task GetAreaDetails_WithInvalidAreaId_ReturnsNotFound()
    {
        // Arrange
        var invalidAreaId = -1;
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(invalidAreaId, A<CancellationToken>._))
            .Returns(new List<AreaDTO>());
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(invalidAreaId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<BaseCommentResponse>(notFoundResult.Value);
        Assert.Equal(404, response.Statuscodes);
    }

    [Fact]
    public async Task GetAreaDetails_WithZeroAreaId_ReturnsNotFound()
    {
        // Arrange
        var areaId = 0;
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, A<CancellationToken>._))
            .Returns(new List<AreaDTO>());
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(areaId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<BaseCommentResponse>(notFoundResult.Value);
        Assert.Equal(404, response.Statuscodes);
    }

    [Fact]
    public async Task GetAreaDetails_WithMaxIntAreaId_ReturnsNotFound()
    {
        // Arrange
        var areaId = int.MaxValue;
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, A<CancellationToken>._))
            .Returns(new List<AreaDTO>());
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(areaId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<BaseCommentResponse>(notFoundResult.Value);
        Assert.Equal(404, response.Statuscodes);
    }

    [Fact]
    public async Task GetAreaDetails_WithAreaHavingNoSensors_ReturnsNotFound()
    {
        // Arrange
        var areaId = 999;
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, A<CancellationToken>._))
            .Returns(new List<AreaDTO>());
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(areaId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<BaseCommentResponse>(notFoundResult.Value);
        Assert.Equal(404, response.Statuscodes);
    }

    [Fact]
    public async Task GetAreaDetails_WithCancellationToken_ReturnsOkWithAreaSensorReadings()
    {
        // Arrange
        var areaId = 2;
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var readings = new List<AreaDTO> { new AreaDTO { AreaId = areaId, SensorId = 22 } };
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, token))
            .Returns(readings);
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(areaId, token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(readings, okResult.Value);
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, token)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAreaDetails_WhenRepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var areaId = 1;
        A.CallTo(() => _areaRepository.GetAreaSensorReadingsAsync(areaId, A<CancellationToken>._))
            .Throws(new Exception("Database error"));
        var controller = new AreaController(_unitOfWork, _mapper);

        // Act
        var result = await controller.GetDetail(areaId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
