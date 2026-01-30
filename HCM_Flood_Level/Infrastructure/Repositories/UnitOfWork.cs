using AutoMapper;
using Core.Interfaces;
using Core.Interfaces.Admin;
using Core.Interfaces.Sensors;
using Infrastructure.DBContext;
using Infrastructure.Repositories.Admin;
using Infrastructure.Repositories.Sensors;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ManageDBContext _context;
        private readonly EventsDBContext _eventsContext;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public IManageStaffRepository ManageStaffRepository { get;  }

        public IManageSensorRepository ManageSensorRepository { get; }

        public UnitOfWork(ManageDBContext context, EventsDBContext eventsContext, IFileProvider fileProvider, IMapper mapper)
        {
            _context = context;
            _eventsContext = eventsContext;
            _fileProvider = fileProvider;
            _mapper = mapper;
            ManageStaffRepository = new ManageStaffRepository(_context, _fileProvider, _mapper);
            ManageSensorRepository = new ManageSensorRepository(_context, _fileProvider, _mapper);
        }
    }
}
