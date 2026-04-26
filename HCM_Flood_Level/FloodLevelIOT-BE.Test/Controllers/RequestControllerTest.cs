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
using WebAPI.Helpers;

namespace FloodLevelIOT_BE.Test.Controllers;

public class RequestControllerTest
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRequestRepository _requestRepository;

    public RequestControllerTest()
    {
        _mapper = A.Fake<IMapper>();
        _requestRepository = A.Fake<IRequestRepository>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        A.CallTo(() => _unitOfWork.ManageRequestRepository).Returns(_requestRepository);
    }

    private static void SetTechnicianClaims(RequestController controller, int technicianUserId)
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

    // Staff Create Request Tests (2 tests)
    [Fact]
    public async Task StaffCreateRequest_WithValidData_ReturnsOkAndCreatesRequest()
    {
        A.CallTo(() => _requestRepository.StaffCreateRequestAsync(A<StaffCreateRequestDTO>._)).Returns(true);
        var controller = new RequestController(_unitOfWork, _mapper);
        var dto = new StaffCreateRequestDTO { SensorId = 1, Priorityid = 1, AssignedTechnicianTo = 10 };

        var result = await controller.StaffCreateRequestAsync(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task StaffCreateRequest_WithMissingRequiredFields_ReturnsBadRequest()
    {
        var controller = new RequestController(_unitOfWork, _mapper);
        controller.ModelState.AddModelError("SensorId", "Required");

        var result = await controller.StaffCreateRequestAsync(new StaffCreateRequestDTO { SensorId = 0, Priorityid = 1, AssignedTechnicianTo = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task StaffCreateRequest_WithNullDto_ReturnsBadRequest()
    {
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffCreateRequestAsync(null!);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Staff Get Requests Tests (3 tests)
    [Fact]
    public async Task StaffGetRequests_WithValidPagination_ReturnsOkWithPaginatedRequests()
    {
        var requests = new List<MaintenanceRequest> { new() { RequestId = 1, SensorId = 1, Status = "Pending" } };
        var dtos = new List<RequestDTO> { new() { RequestId = 1, SensorName = "S1", Priority = "P1", Status = "Pending" } };
        A.CallTo(() => _requestRepository.StaffGetRequestAsync(A<EntityParam>._)).Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync()).Returns(1);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(dtos);
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffGetRequestAsync(1, 10, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<RequestDTO>>(ok.Value);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task StaffGetRequests_WithStatusFilter_ReturnsOkWithFilteredByStatus()
    {
        var requests = new List<MaintenanceRequest>();
        A.CallTo(() => _requestRepository.StaffGetRequestAsync(A<EntityParam>.That.Matches(p => p.RequestStatus == "Pending")))
            .Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(new List<RequestDTO>());
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffGetRequestAsync(1, 10, status: "Pending", null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task StaffGetRequests_WithPriorityFilter_ReturnsOkWithFilteredByPriority()
    {
        var requests = new List<MaintenanceRequest>();
        A.CallTo(() => _requestRepository.StaffGetRequestAsync(A<EntityParam>.That.Matches(p => p.RequestPriority == "High")))
            .Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync()).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(new List<RequestDTO>());
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffGetRequestAsync(1, 10, null, priority: "High");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task StaffGetRequests_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _requestRepository.StaffGetRequestAsync(A<EntityParam>._)).Throws(new Exception("DB Error"));
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffGetRequestAsync(1, 10, null, null);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Staff Delete Request Tests (1 test)
    [Fact]
    public async Task StaffDeleteRequest_WithValidId_ReturnsOkAndDeletesRequest()
    {
        A.CallTo(() => _requestRepository.StaffDeleteRequestAsync(1)).Returns(true);
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffDeleteRequestAsync(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task StaffDeleteRequest_WithInvalidId_ReturnsNotFound()
    {
        A.CallTo(() => _requestRepository.StaffDeleteRequestAsync(999)).Returns(false);
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffDeleteRequestAsync(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task StaffDeleteRequest_WithNegativeId_ReturnsNotFound()
    {
        A.CallTo(() => _requestRepository.StaffDeleteRequestAsync(-1)).Returns(false);
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffDeleteRequestAsync(-1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task StaffDeleteRequest_WithInternalServerError_ReturnsBadRequest()
    {
        A.CallTo(() => _requestRepository.StaffDeleteRequestAsync(1)).Throws(new Exception("Database Error"));
        var controller = new RequestController(_unitOfWork, _mapper);

        var result = await controller.StaffDeleteRequestAsync(1);

        var badRequest = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    // Technician Update Status Tests (3 tests)
    [Fact]
    public async Task TechnicianUpdateStatus_WithValidIdAndStatus_ReturnsOkAndUpdatesStatus()
    {
        var request = new MaintenanceRequest { RequestId = 1, AssignedTechnicianTo = 10, Status = "Pending" };
        A.CallTo(() => _requestRepository.GetByIdAsync(1)).Returns(request);
        A.CallTo(() => _requestRepository.TechnicianUpdateStatusAsync(1, A<TechnicianUpdateStatusDTO>._)).Returns(true);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(1, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithInvalidId_ReturnsNotFound()
    {
        A.CallTo(() => _requestRepository.GetByIdAsync(99)).Returns((MaintenanceRequest)null!);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(99, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithZeroId_ReturnsNotFound()
    {
        A.CallTo(() => _requestRepository.GetByIdAsync(0)).Returns((MaintenanceRequest)null!);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(0, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithNegativeId_ReturnsNotFound()
    {
        A.CallTo(() => _requestRepository.GetByIdAsync(-1)).Returns((MaintenanceRequest)null!);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(-1, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithMaxIntId_ReturnsNotFound()
    {
        A.CallTo(() => _requestRepository.GetByIdAsync(int.MaxValue)).Returns((MaintenanceRequest)null!);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(int.MaxValue, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithNullStatus_ReturnsBadRequest()
    {
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var emptyStatus = await controller.TechnicianUpdateStatusAsync(1, new TechnicianUpdateStatusDTO { Status = "" });
        Assert.IsType<BadRequestObjectResult>(emptyStatus);

        var nullDto = await controller.TechnicianUpdateStatusAsync(1, null!);
        Assert.IsType<BadRequestObjectResult>(nullDto);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithRequestNotAssignedToTechnician_ReturnsNotFound()
    {
        var request = new MaintenanceRequest { RequestId = 1, AssignedTechnicianTo = 99, Status = "Pending" };
        A.CallTo(() => _requestRepository.GetByIdAsync(1)).Returns(request);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(1, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithoutAuthorization_ReturnsUnauthorized()
    {
        var controller = new RequestController(_unitOfWork, _mapper);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.TechnicianUpdateStatusAsync(1, new TechnicianUpdateStatusDTO { Status = "Completed" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianUpdateStatus_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _requestRepository.GetByIdAsync(1)).Throws(new Exception("Database Error"));
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianUpdateStatusAsync(1, new TechnicianUpdateStatusDTO { Status = "Completed" });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    // Technician Get Requests Tests (2 tests)
    [Fact]
    public async Task TechnicianGetRequests_WithValidPagination_ReturnsOkWithAssignedRequests()
    {
        var requests = new List<MaintenanceRequest> { new() { RequestId = 1, AssignedTechnicianTo = 10 } };
        var dtos = new List<RequestDTO> { new() { RequestId = 1, SensorName = "S1", Priority = "P1", Status = "Pending" } };
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>._)).Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync(A<Expression<Func<MaintenanceRequest, bool>>>._)).Returns(1);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(dtos);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(1, 10, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<RequestDTO>>(ok.Value);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithStatusFilter_ReturnsOkWithFilteredAssignedRequests()
    {
        var requests = new List<MaintenanceRequest>();
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>.That.Matches(p => p.RequestStatus == "Completed")))
            .Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync(A<Expression<Func<MaintenanceRequest, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(new List<RequestDTO>());
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(1, 10, status: "Completed", null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithPriorityFilter_ReturnsOkWithFilteredAssignedRequests()
    {
        var requests = new List<MaintenanceRequest>();
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>.That.Matches(p => p.RequestPriority == "High")))
            .Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync(A<Expression<Func<MaintenanceRequest, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(new List<RequestDTO>());
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(1, 10, null, priority: "High");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithZeroPageNumber_ReturnsOk()
    {
        var requests = new List<MaintenanceRequest>();
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>.That.Matches(p => p.Pagenumber == 0 && p.Pagesize == 10)))
            .Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync(A<Expression<Func<MaintenanceRequest, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(new List<RequestDTO>());
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(0, 10, null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithZeroPageSize_ReturnsOk()
    {
        var requests = new List<MaintenanceRequest>();
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>.That.Matches(p => p.Pagenumber == 1 && p.Pagesize == 0)))
            .Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync(A<Expression<Func<MaintenanceRequest, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(new List<RequestDTO>());
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(1, 0, null, null);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithNoAssignedRequests_ReturnsOkWithEmptyPagination()
    {
        var requests = new List<MaintenanceRequest>();
        var dtos = new List<RequestDTO>();
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>._)).Returns(requests);
        A.CallTo(() => _requestRepository.CountAsync(A<Expression<Func<MaintenanceRequest, bool>>>._)).Returns(0);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(dtos);
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(1, 10, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<RequestDTO>>(ok.Value);
        Assert.Equal(0, page.TotalCount);
        Assert.Empty(page.Data);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithoutAuthorization_ReturnsUnauthorized()
    {
        var controller = new RequestController(_unitOfWork, _mapper);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.TechnicianGetRequestAsync(1, 10, null, null);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task TechnicianGetRequests_WithInternalServerError_Returns500()
    {
        A.CallTo(() => _requestRepository.TechnicianGetRequestAsync(10, A<EntityParam>._)).Throws(new Exception("Database Error"));
        var controller = new RequestController(_unitOfWork, _mapper);
        SetTechnicianClaims(controller, 10);

        var result = await controller.TechnicianGetRequestAsync(1, 10, null, null);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}
