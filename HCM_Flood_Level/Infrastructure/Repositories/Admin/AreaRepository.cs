using Core.Entities;
using Core.Interfaces.Admin;
using Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Admin
{
    public class AreaRepository : GenericRepository<Area>, IAreaRepository
    {
        public AreaRepository(ManageDBContext context) : base(context)
        {
        }
    }
}
