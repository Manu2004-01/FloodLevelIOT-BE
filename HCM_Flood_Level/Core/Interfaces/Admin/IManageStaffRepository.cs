using Core.DTOs.Admin;
using Core.Entities;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Admin
{
    public interface IManageStaffRepository : IGenericRepository<Staff>
    {
        Task<IEnumerable<Staff>> GetAllStaffAsync(EntityParam entityParam);
        Task<bool> AddNewStaffAsync(CreateStaffDTO dto);
        Task<bool> DeleteStaffAsync(int id);
        Task<bool> UpdateStaffAsync(int id, UpdateStaffDTO dto);
    }
}
