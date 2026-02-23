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
    public interface IManageUserRepository : IGenericRepository<User>
    {
        Task<IEnumerable<User>> GetAllStaffAsync(EntityParam entityParam);
        Task<bool> AddNewStaffAsync(CreateUserDTO dto);
        Task<bool> DeleteStaffAsync(int id);
        Task<bool> UpdateStaffAsync(int id, UpdateUserDTO dto);
    }
}
