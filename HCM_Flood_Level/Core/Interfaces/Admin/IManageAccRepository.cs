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
    public interface IManageAccRepository : IGenericRepository<User>
    {
        Task<IEnumerable<User>> GetAllAccAsync(EntityParam entityParam);
        Task<bool> AddNewAccAsync(CreateAccDTO dto);
    }
}
