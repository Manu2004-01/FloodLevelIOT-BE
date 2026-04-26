using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Sharing;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using WebAPI.Controllers;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace FloodLevelIOT_BE.Test.Controllers;

public class ScheduleControllerTest
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ISensorRepository _sensorRepository;

    public ScheduleControllerTest()
    {
        _mapper = A.Fake<IMapper>();
        _scheduleRepository = A.Fake<IScheduleRepository>();
        _sensorRepository = A.Fake<ISensorRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        A.CallTo(() => _unitOfWork.ManageMaintenanceScheduleRepository).Returns(_scheduleRepository);
        A.CallTo(() => _unitOfWork.ManageSensorRepository).Returns(_sensorRepository);
    }

    private static void SetTechnicianClaims(ScheduleController controller, int technicianUserId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, technicianUserId.ToString()) },
                        "test"))
            }
        };
    }

    private static CreateMaintenanceScheduleDTO ValidWeeklyDto() =>
        new()
        {
            SensorId = 1,
            ScheduleType = "Weekly",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 8),
            AssignedTechnicianId = 10
        };

    // Create Schedule Tests (3 tests)
    [Fact]
    public async Task CreateSchedule_WithValidData_ReturnsOkAndCreatesSchedule()
    {
        A.CallTo(() => _scheduleRepository.AddNewScheduleAsync(A<CreateMaintenanceScheduleDTO>._)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.CreateSchedule(ValidWeeklyDto());

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateSchedule_WithMissingRequiredFields_ReturnsBadRequest()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);
        var missingDates = new CreateMaintenanceScheduleDTO
        {
            SensorId = 1,
            ScheduleType = "Weekly",
            StartDate = null,
            EndDate = new DateTime(2025, 1, 8)
        };
        var missingDatesResult = await controller.CreateSchedule(missingDates);
        Assert.IsType<BadRequestObjectResult>(missingDatesResult);

        controller.ModelState.Clear();
        controller.ModelState.AddModelError("SensorId", "Invalid");
        var invalidModelResult = await controller.CreateSchedule(ValidWeeklyDto());
        Assert.IsType<BadRequestObjectResult>(invalidModelResult);
    }

    [Fact]
    public async Task CreateSchedule_WithInvalidScheduleType_ReturnsBadRequest()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);
        var dto = new CreateMaintenanceScheduleDTO
        {
            SensorId = 1,
            ScheduleType = "Daily",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 8)
        };

        var result = await controller.CreateSchedule(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateSchedule_WithSensorHavingIncompleteSchedule_ReturnsBadRequest()
    {
        A.CallTo(() => _scheduleRepository.AddNewScheduleAsync(A<CreateMaintenanceScheduleDTO>._)).Returns(false);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.CreateSchedule(ValidWeeklyDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Auto Schedule Tests (2 tests)
    [Fact]
    public async Task ExecuteAutoSchedule_WithValidSensorId_ReturnsOkAndCreatesSchedule()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(1)).Returns(new Sensor { SensorId = 1 });
        A.CallTo(() => _scheduleRepository.AddAutoScheduleAsync(1)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithNonexistentSensor_ReturnsNotFound()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(99)).Returns((Sensor)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithSensorHavingIncompleteSchedule_ReturnsBadRequest()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(2)).Returns(new Sensor { SensorId = 2 });
        A.CallTo(() => _scheduleRepository.AddAutoScheduleAsync(2)).Returns(false);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(2);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithZeroSensorId_ReturnsNotFound()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(0)).Returns((Sensor)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(0);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithNegativeSensorId_ReturnsNotFound()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(-1)).Returns((Sensor)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(-1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithMaxIntSensorIdAndIncompleteSchedule_ReturnsBadRequest()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(int.MaxValue)).Returns(new Sensor { SensorId = int.MaxValue });
        A.CallTo(() => _scheduleRepository.AddAutoScheduleAsync(int.MaxValue)).Returns(false);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(int.MaxValue);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithAnotherValidSensorId_ReturnsOk()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(3)).Returns(new Sensor { SensorId = 3 });
        A.CallTo(() => _scheduleRepository.AddAutoScheduleAsync(3)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(3);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ExecuteAutoSchedule_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _sensorRepository.GetByIdAsync(1)).Throws(new Exception("Database Error"));
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.ExecuteAutoScheduleForSensor(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Get All Schedules Tests (3 tests)
    [Fact]
    public async Task GetAllSchedules_WithValidPagination_ReturnsOkWithPaginatedSchedules()
    {
        var schedules = new List<MaintenanceSchedule> { new() { ScheduleId = 1, SensorId = 1, Status = "Scheduled" } };
        var dtos = new List<ScheduleDTO> { new() { ScheduleId = 1, SensorName = "S1", ScheduleType = "Weekly", ScheduleMode = "Manual", Status = "Scheduled", AssignedStaff = "T" } };
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>._)).Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(1);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(dtos);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 10, null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<ScheduleDTO>>(ok.Value);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task GetAllSchedules_WithStatusFilter_ReturnsOkWithFilteredByStatus()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>.That.Matches(p => p.ScheduleStatus == "Active")))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 10, status: "Active", null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllSchedules_WithTypeAndModeFilter_ReturnsOkWithFilteredSchedules()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>.That.Matches(p =>
                p.ScheduleType == "Monthly" && p.ScheduleMode == "Auto")))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 10, null, type: "Monthly", mode: "Auto");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllSchedules_WithZeroPageNumber_ReturnsOk()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>.That.Matches(p => p.Pagenumber == 0 && p.Pagesize == 10)))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(0, 10, null, null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllSchedules_WithZeroPageSize_ReturnsOk()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>.That.Matches(p => p.Pagenumber == 1 && p.Pagesize == 0)))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 0, null, null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllSchedules_WithNoData_ReturnsOkWithEmptyPagination()
    {
        var schedules = new List<MaintenanceSchedule>();
        var dtos = new List<ScheduleDTO>();
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>._)).Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(dtos);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 10, null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<ScheduleDTO>>(ok.Value);
        Assert.Equal(0, page.TotalCount);
        Assert.Empty(page.Data);
    }

    [Fact]
    public async Task GetAllSchedules_WithTypeFilterOnly_ReturnsOkWithFilteredByType()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>.That.Matches(p =>
                p.ScheduleType == "Monthly" && p.ScheduleMode == null && p.ScheduleStatus == null)))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 10, null, type: "Monthly", mode: null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllSchedules_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _scheduleRepository.GetAllSchedulesAsync(A<EntityParam>._)).Throws(new Exception("Database Error"));
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.GetAllSchedules(1, 10, null, null, null);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Update Schedule Tests (2 tests)
    [Fact]
    public async Task UpdateSchedule_WithValidData_ReturnsOkAndUpdatesSchedule()
    {
        var existing = new MaintenanceSchedule
        {
            ScheduleId = 1,
            ScheduleType = "Weekly",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 8)
        };
        A.CallTo(() => _scheduleRepository.GetByIdAsync(1)).Returns(existing);
        A.CallTo(() => _scheduleRepository.UpdateScheduleAsync(1, A<UpdateMaintenanceScheduleDTO>._)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        var dto = new UpdateMaintenanceScheduleDTO { Note = "Updated note" };

        var result = await controller.UpdateSchedule(1, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithInvalidId_ReturnsBadRequest()
    {
        var existing = new MaintenanceSchedule
        {
            ScheduleId = 5,
            ScheduleType = "Weekly",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 8)
        };
        A.CallTo(() => _scheduleRepository.GetByIdAsync(5)).Returns(existing);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        var dto = new UpdateMaintenanceScheduleDTO { ScheduleType = "InvalidType", StartDate = existing.StartDate, EndDate = existing.EndDate };

        var result = await controller.UpdateSchedule(5, dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithNonexistentId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(999)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.UpdateSchedule(999, new UpdateMaintenanceScheduleDTO { Note = "x" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithZeroId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(0)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.UpdateSchedule(0, new UpdateMaintenanceScheduleDTO { Note = "x" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithNegativeId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(-1)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.UpdateSchedule(-1, new UpdateMaintenanceScheduleDTO { Note = "x" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithMaxIntId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(int.MaxValue)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.UpdateSchedule(int.MaxValue, new UpdateMaintenanceScheduleDTO { Note = "x" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithNullDto_ReturnsBadRequest()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.UpdateSchedule(1, null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSchedule_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(1)).Throws(new Exception("Database Error"));
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.UpdateSchedule(1, new UpdateMaintenanceScheduleDTO { Note = "x" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Delete Schedule Tests (2 tests)
    [Fact]
    public async Task DeleteSchedule_WithValidId_ReturnsOkAndDeletesSchedule()
    {
        A.CallTo(() => _scheduleRepository.DeleteScheduleAsync(1)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithInvalidId_ReturnsBadRequest()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithNonexistentId_ReturnsBadRequest()
    {
        A.CallTo(() => _scheduleRepository.DeleteScheduleAsync(999)).Returns(false);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(999);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithNegativeId_ReturnsBadRequest()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(-1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithMaxIntId_ReturnsBadRequest()
    {
        A.CallTo(() => _scheduleRepository.DeleteScheduleAsync(int.MaxValue)).Returns(false);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(int.MaxValue);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithAnotherValidId_ReturnsOkAndDeletesSchedule()
    {
        A.CallTo(() => _scheduleRepository.DeleteScheduleAsync(2)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(2);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithAnotherNonexistentId_ReturnsBadRequest()
    {
        A.CallTo(() => _scheduleRepository.DeleteScheduleAsync(1000)).Returns(false);
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(1000);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSchedule_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _scheduleRepository.DeleteScheduleAsync(1)).Throws(new Exception("Database Error"));
        var controller = new ScheduleController(_unitOfWork, _mapper);

        var result = await controller.DeleteSchedule(1);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Technician Get Schedules Tests (2 tests)
    [Fact]
    public async Task GetMySchedules_WithValidPagination_ReturnsOkWithTechnicianSchedules()
    {
        var schedules = new List<MaintenanceSchedule> { new() { ScheduleId = 1, AssignedTechnicianId = 10 } };
        var dtos = new List<ScheduleDTO> { new() { ScheduleId = 1, SensorName = "S1", ScheduleType = "Weekly", ScheduleMode = "Manual", Status = "Scheduled", AssignedStaff = "Me" } };
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>._)).Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync(A<Expression<Func<MaintenanceSchedule, bool>>>._)).Returns(1);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(dtos);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(1, 10, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<ScheduleDTO>>(ok.Value);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task GetMySchedules_WithStatusFilter_ReturnsOkWithFilteredTechnicianSchedules()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>.That.Matches(p => p.ScheduleStatus == "Completed")))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync(A<Expression<Func<MaintenanceSchedule, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(1, 10, status: "Completed", null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMySchedules_WithTypeFilter_ReturnsOkWithFilteredTechnicianSchedules()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>.That.Matches(p => p.ScheduleType == "Weekly")))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync(A<Expression<Func<MaintenanceSchedule, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(1, 10, null, type: "Weekly");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMySchedules_WithZeroPageNumber_ReturnsOk()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>.That.Matches(p => p.Pagenumber == 0 && p.Pagesize == 10)))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync(A<Expression<Func<MaintenanceSchedule, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(0, 10, null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMySchedules_WithZeroPageSize_ReturnsOk()
    {
        var schedules = new List<MaintenanceSchedule>();
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>.That.Matches(p => p.Pagenumber == 1 && p.Pagesize == 0)))
            .Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync(A<Expression<Func<MaintenanceSchedule, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(new List<ScheduleDTO>());
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(1, 0, null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMySchedules_WithNoAssignedSchedules_ReturnsOkWithEmptyPagination()
    {
        var schedules = new List<MaintenanceSchedule>();
        var dtos = new List<ScheduleDTO>();
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>._)).Returns(schedules);
        A.CallTo(() => _scheduleRepository.CountAsync(A<Expression<Func<MaintenanceSchedule, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(dtos);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(1, 10, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<ScheduleDTO>>(ok.Value);
        Assert.Equal(0, page.TotalCount);
        Assert.Empty(page.Data);
    }

    [Fact]
    public async Task GetMySchedules_WithoutAuthorization_ReturnsUnauthorized()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.GetMySchedules(1, 10, null, null);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetMySchedules_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _scheduleRepository.GetSchedulesByTechnicianAsync(10, A<EntityParam>._)).Throws(new Exception("Database Error"));
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.GetMySchedules(1, 10, null, null);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Technician Update Status Tests (2 tests)
    [Fact]
    public async Task UpdateMyScheduleStatus_WithValidStatus_ReturnsOkAndUpdatesStatus()
    {
        var schedule = new MaintenanceSchedule { ScheduleId = 1, AssignedTechnicianId = 10, Status = "Scheduled" };
        A.CallTo(() => _scheduleRepository.GetByIdAsync(1)).Returns(schedule);
        A.CallTo(() => _scheduleRepository.UpdateScheduleStatusAsync(1, A<UpdateScheduleStatusDTO>._)).Returns(true);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(1, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithInvalidStatus_ReturnsBadRequest()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var empty = await controller.UpdateMyScheduleStatus(1, new UpdateScheduleStatusDTO { Status = "" });
        Assert.IsType<BadRequestObjectResult>(empty);

        var nullDto = await controller.UpdateMyScheduleStatus(1, null!);
        Assert.IsType<BadRequestObjectResult>(nullDto);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithNonexistentScheduleId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(999)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(999, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithZeroScheduleId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(0)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(0, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithNegativeScheduleId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(-1)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(-1, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithMaxIntScheduleId_ReturnsNotFound()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(int.MaxValue)).Returns((MaintenanceSchedule)null!);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(int.MaxValue, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithScheduleNotAssignedToTechnician_ReturnsNotFound()
    {
        var schedule = new MaintenanceSchedule { ScheduleId = 4, AssignedTechnicianId = 99, Status = "Scheduled" };
        A.CallTo(() => _scheduleRepository.GetByIdAsync(4)).Returns(schedule);
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(4, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithoutAuthorization_ReturnsUnauthorized()
    {
        var controller = new ScheduleController(_unitOfWork, _mapper);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.UpdateMyScheduleStatus(1, new UpdateScheduleStatusDTO { Status = "Completed" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMyScheduleStatus_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _scheduleRepository.GetByIdAsync(1)).Throws(new Exception("Database Error"));
        var controller = new ScheduleController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.UpdateMyScheduleStatus(1, new UpdateScheduleStatusDTO { Status = "Completed" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
