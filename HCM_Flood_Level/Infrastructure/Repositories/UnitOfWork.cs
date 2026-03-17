using AutoMapper;
using Core.Interfaces;
using Infrastructure.DBContext;
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
        private readonly IMapsService _mapsService;

        public IManageUserRepository ManageUserRepository{ get;  }

        public IManageSensorRepository ManageSensorRepository { get; }

        public IMaintenanceScheduleRepository ManageMaintenanceScheduleRepository { get; }

        public ILocationRepository LocationRepository { get; }

        public UnitOfWork(ManageDBContext context, EventsDBContext eventsContext, IFileProvider fileProvider, IMapper mapper, IMapsService mapsService)
        {
            _context = context;
            _eventsContext = eventsContext;
            _fileProvider = fileProvider;
            _mapper = mapper;
            _mapsService = mapsService;
            ManageUserRepository = new ManageUserRepository(_context, _fileProvider, _mapper);
            ManageMaintenanceScheduleRepository = new MaintenanceScheduleRepository(_context, _fileProvider, _mapper);
            ManageSensorRepository = new ManageSensorRepository(_context, _eventsContext, _fileProvider, _mapper, _mapsService, ManageMaintenanceScheduleRepository);
            LocationRepository = new LocationRepository(_context);
        }
    }
}
