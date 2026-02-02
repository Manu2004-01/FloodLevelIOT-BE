using AutoMapper;
using Core.DTOs;
using Core.Entities;

namespace WebAPI.Models
{
    public class MappingUser : Profile
    {
        public MappingUser()
        {
            CreateMap<Staff, ManageStaffDTO>()
                .ForMember(a => a.UserId, a => a.MapFrom(b => b.StaffId))
                .ForMember(a => a.Status, a => a.MapFrom(b => b.IsActive))
                .ForMember(a => a.RoleName, a => a.MapFrom(b => b.Role != null ? b.Role.RoleName : string.Empty))
                .ForMember(a => a.Username, a => a.MapFrom(b => b.StaffAccName));

            CreateMap<Staff, StaffDTO>()
                .ForMember(a => a.UserId, a => a.MapFrom(b => b.StaffId))
                .ForMember(a => a.Status, a => a.MapFrom(b => b.IsActive))
                .ForMember(a => a.RoleName, a => a.MapFrom(b => b.Role != null ? b.Role.RoleName : string.Empty))
                .ForMember(a => a.Username, a => a.MapFrom(b => b.StaffAccName));

            CreateMap<CreateStaffDTO, Staff>()
                .ForMember(a => a.RoleId, a => a.MapFrom(b => b.RoleId))
                .ForMember(a => a.PasswordHash, a => a.Ignore()) 
                .ForMember(a => a.StaffAccName, a => a.MapFrom(b => b.Username))
                .ReverseMap();

            CreateMap<UpdateStaffDTO, Staff>()
                .ForMember(a => a.RoleId, a => a.MapFrom(b => b.RoleId))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
