using Core.DTOs;
using Core.Entities;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<IEnumerable<User>> GetAllUserAsync(EntityParam entityParam);
        Task<int> CountUserAsync(EntityParam entityParam);
        Task<bool> AddNewStaffAsync(CreateUserDTO dto);
        Task<StaffDeleteUserResult> DeleteStaffAsync(int id);
        Task<bool> UpdateStaffAsync(int id, UpdateUserDTO dto);
        Task<bool> UpdateProfileAsync(int id, UpdateProfileDTO dto);
    }
}
