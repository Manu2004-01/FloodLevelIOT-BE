using Core.Interfaces.Admin;
using Core.Interfaces.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        IManageStaffRepository ManageStaffRepository { get; }
        IManageSensorRepository ManageSensorRepository { get; }
    }
}
