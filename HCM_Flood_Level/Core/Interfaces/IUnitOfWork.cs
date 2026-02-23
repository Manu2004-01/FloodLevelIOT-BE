using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        IManageUserRepository ManageUserRepository { get; }
        IManageSensorRepository ManageSensorRepository { get; }
        IMaintenanceScheduleRepository ManageMaintenanceScheduleRepository { get; }
        ILocationRepository LocationRepository { get; }
        IAreaRepository AreaRepository { get; }
    }
}
