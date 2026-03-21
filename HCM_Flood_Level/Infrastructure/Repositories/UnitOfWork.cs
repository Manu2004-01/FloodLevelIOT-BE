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

        public IUserRepository ManageUserRepository{ get;  }

        public ISensorRepository ManageSensorRepository { get; }

        public IScheduleRepository ManageMaintenanceScheduleRepository { get; }

        public ILocationRepository LocationRepository { get; }

        public IRequestRepository ManageRequestRepository { get; }

        public IAreaRepository AreaRepository { get; }

        public UnitOfWork(ManageDBContext context, EventsDBContext eventsContext, IFileProvider fileProvider, IMapper mapper, IMapsService mapsService)
        {
            _context = context;
            _eventsContext = eventsContext;
            _fileProvider = fileProvider;
            _mapper = mapper;
            _mapsService = mapsService;
            ManageUserRepository = new UserRepository(_context, _fileProvider, _mapper);
            ManageMaintenanceScheduleRepository = new ScheduleRepository(_context, _fileProvider, _mapper);
            ManageSensorRepository = new SensorRepository(_context, _eventsContext, _fileProvider, _mapper, _mapsService, ManageMaintenanceScheduleRepository);
            LocationRepository = new LocationRepository(_context);
            ManageRequestRepository = new RequestRepository(_context, _fileProvider, _mapper);
            AreaRepository = new AreaRepository(_context, _eventsContext);
        }
    }
}
