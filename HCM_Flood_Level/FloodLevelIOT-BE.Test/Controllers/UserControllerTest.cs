using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Sharing;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.Errors;
using WebAPI.Helpers;

namespace FloodLevelIOT_BE.Test.Controllers;

public class UserControllerTest
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IRequestRepository _requestRepository;

    public UserControllerTest()
    {
        _mapper = A.Fake<IMapper>();
        _unitOfWork = A.Fake<IUnitOfWork>();
        _userRepository = A.Fake<IUserRepository>();
        _scheduleRepository = A.Fake<IScheduleRepository>();
        _requestRepository = A.Fake<IRequestRepository>();
        A.CallTo(() => _unitOfWork.ManageUserRepository).Returns(_userRepository);
        A.CallTo(() => _unitOfWork.ManageMaintenanceScheduleRepository).Returns(_scheduleRepository);
        A.CallTo(() => _unitOfWork.ManageRequestRepository).Returns(_requestRepository);
    }

    // Get All Users Tests (2 tests)
    [Fact]
    public async Task GetAllUsers_WithValidPagination_ReturnsOkWithPaginatedUsers()
    {
        var users = new List<User> { new() { UserId = 1, FullName = "A", Email = "a@test.com" } };
        var summaries = new List<UserSummaryDTO> { new() { UserId = 1, FullName = "A", Email = "a@test.com" } };
        A.CallTo(() => _userRepository.GetAllUserAsync(A<EntityParam>._)).Returns(users);
        A.CallTo(() => _userRepository.CountUserAsync(A<EntityParam>._)).Returns(1);
        A.CallTo(() => _mapper.Map<List<UserSummaryDTO>>(users)).Returns(summaries);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAllAcc(1, 10, null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var page = Assert.IsType<Pagination<UserSummaryDTO>>(ok.Value);
        Assert.Equal(1, page.TotalCount);
        Assert.Single(page.Data);
    }

    [Fact]
    public async Task GetAllUsers_WithRoleFilter_ReturnsOkWithFilteredUsersByRole()
    {
        var users = new List<User>();
        A.CallTo(() => _userRepository.GetAllUserAsync(A<EntityParam>.That.Matches(p => p.RoleId == 2))).Returns(users);
        A.CallTo(() => _userRepository.CountUserAsync(A<EntityParam>.That.Matches(p => p.RoleId == 2))).Returns(0);
        A.CallTo(() => _mapper.Map<List<UserSummaryDTO>>(users)).Returns(new List<UserSummaryDTO>());
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAllAcc(1, 10, null, roleid: 2);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllUsers_WithSearchFilter_ReturnsOkWithFilteredUsersByName()
    {
        var users = new List<User>();
        A.CallTo(() => _userRepository.GetAllUserAsync(A<EntityParam>.That.Matches(p => p.Search == "john"))).Returns(users);
        A.CallTo(() => _userRepository.CountUserAsync(A<EntityParam>.That.Matches(p => p.Search == "john"))).Returns(0);
        A.CallTo(() => _mapper.Map<List<UserSummaryDTO>>(users)).Returns(new List<UserSummaryDTO>());
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAllAcc(1, 10, search: "John", null);

        Assert.IsType<OkObjectResult>(result);
    }

    // Get User By ID Tests (3 tests)
    [Fact]
    public async Task GetUserById_WithValidId_ReturnsOkWithUserDetails()
    {
        var user = new User
        {
            UserId = 1,
            FullName = "Citizen",
            Email = "c@test.com",
            Role = new Role { RoleName = "Citizen" }
        };
        var dto = new UserDTO { UserId = 1, FullName = "Citizen", RoleName = "Citizen" };
        A.CallTo(() => _userRepository.GetByIdAsync(1, A<System.Linq.Expressions.Expression<Func<User, object>>[]>.Ignored))
            .Returns(user);
        A.CallTo(() => _mapper.Map<UserDTO>(user)).Returns(dto);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAccById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<UserDTO>(ok.Value);
        Assert.Equal(1, payload.UserId);
        Assert.Null(payload.Schedule);
        Assert.Null(payload.Request);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ReturnsBadRequest()
    {
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAccById(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetUserById_WithNonexistentId_ReturnsNotFound()
    {
        A.CallTo(() => _userRepository.GetByIdAsync(99, A<System.Linq.Expressions.Expression<Func<User, object>>[]>.Ignored))
            .Returns((User)null!);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAccById(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetUserById_WithTechnicianRole_ReturnsOkWithSchedulesAndRequests()
    {
        var user = new User
        {
            UserId = 5,
            FullName = "Tech",
            Email = "t@test.com",
            Role = new Role { RoleName = "Technician" }
        };
        var schedules = new List<MaintenanceSchedule> { new() { ScheduleId = 1, SensorId = 1 } };
        var requests = new List<MaintenanceRequest> { new() { RequestId = 1, SensorId = 1, PriorityId = 1 } };
        var dto = new UserDTO { UserId = 5, FullName = "Tech", RoleName = "Technician" };
        var scheduleDtos = new List<ScheduleDTO> { new() { SensorName = "S1" } };
        var requestDtos = new List<RequestDTO> { new() { SensorName = "S1" } };

        A.CallTo(() => _userRepository.GetByIdAsync(5, A<System.Linq.Expressions.Expression<Func<User, object>>[]>.Ignored))
            .Returns(user);
        A.CallTo(() => _mapper.Map<UserDTO>(user)).Returns(dto);
        A.CallTo(() => _scheduleRepository.GetByAssignedTechnicianIdAsync(5)).Returns(schedules);
        A.CallTo(() => _requestRepository.GetByAssignedTechnicianIdAsync(5)).Returns(requests);
        A.CallTo(() => _mapper.Map<List<ScheduleDTO>>(schedules)).Returns(scheduleDtos);
        A.CallTo(() => _mapper.Map<List<RequestDTO>>(requests)).Returns(requestDtos);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.GetAccById(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<UserDTO>(ok.Value);
        Assert.Same(scheduleDtos, payload.Schedule);
        Assert.Same(requestDtos, payload.Request);
    }

    // Create Technician Tests (2 tests)
    [Fact]
    public async Task CreateTechnician_WithValidData_ReturnsOkAndCreatesTechnician()
    {
        A.CallTo(() => _userRepository.AddNewStaffAsync(A<CreateUserDTO>._)).Returns(true);
        var controller = new UserController(_unitOfWork, _mapper);
        var dto = new CreateUserDTO { FullName = "T", Email = "t@test.com", Password = "secret" };

        var result = await controller.CreateAcc(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateTechnician_WithMissingRequiredFields_ReturnsBadRequest()
    {
        var controller = new UserController(_unitOfWork, _mapper);
        controller.ModelState.AddModelError("Email", "Required");

        var result = await controller.CreateAcc(new CreateUserDTO { FullName = "T", Email = "", Password = "x" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTechnician_WithDuplicateEmail_ReturnsBadRequest()
    {
        A.CallTo(() => _userRepository.AddNewStaffAsync(A<CreateUserDTO>._)).Returns(false);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.CreateAcc(new CreateUserDTO { FullName = "T", Email = "dup@test.com", Password = "x" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Update Technician Tests (2 tests)
    [Fact]
    public async Task UpdateTechnician_WithValidRoleChange_ReturnsOkAndUpdatesTechnician()
    {
        A.CallTo(() => _userRepository.UpdateStaffAsync(1, A<UpdateUserDTO>._)).Returns(true);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.UpdateAcc(1, new UpdateUserDTO { RoleId = 2 });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTechnician_WithInvalidId_ReturnsBadRequest()
    {
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.UpdateAcc(0, new UpdateUserDTO { RoleId = 2 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTechnician_WithNonexistentId_ReturnsNotFound()
    {
        A.CallTo(() => _userRepository.UpdateStaffAsync(999, A<UpdateUserDTO>._)).Returns(false);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.UpdateAcc(999, new UpdateUserDTO { RoleId = 2 });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTechnician_WithNoFieldsToUpdate_ReturnsBadRequest()
    {
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.UpdateAcc(1, new UpdateUserDTO());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // Delete Technician Tests (2 tests)
    [Fact]
    public async Task DeleteTechnician_WithValidId_ReturnsOkAndDeletesTechnician()
    {
        A.CallTo(() => _userRepository.DeleteStaffAsync(1)).Returns(StaffDeleteUserResult.Success);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.DeleteAcc(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTechnician_WithInvalidId_ReturnsBadRequest()
    {
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.DeleteAcc(0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTechnician_WithNonexistentId_ReturnsNotFound()
    {
        A.CallTo(() => _userRepository.DeleteStaffAsync(99)).Returns(StaffDeleteUserResult.UserNotFound);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.DeleteAcc(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTechnician_WithIncompleteMaintenanceRequests_ReturnsBadRequest()
    {
        A.CallTo(() => _userRepository.DeleteStaffAsync(2)).Returns(StaffDeleteUserResult.TechnicianHasIncompleteWork);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.DeleteAcc(2);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseCommentResponse>(bad.Value);
        Assert.Equal(400, response.Statuscodes);
    }

    [Fact]
    public async Task DeleteTechnician_WithIncompleteSchedules_ReturnsBadRequest()
    {
        A.CallTo(() => _userRepository.DeleteStaffAsync(3)).Returns(StaffDeleteUserResult.TechnicianHasIncompleteWork);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.DeleteAcc(3);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTechnician_WithNonTechnicianRole_ReturnsBadRequest()
    {
        A.CallTo(() => _userRepository.DeleteStaffAsync(4)).Returns(StaffDeleteUserResult.TargetNotTechnician);
        var controller = new UserController(_unitOfWork, _mapper);

        var result = await controller.DeleteAcc(4);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<BaseCommentResponse>(bad.Value);
        Assert.Equal(400, response.Statuscodes);
    }
}
