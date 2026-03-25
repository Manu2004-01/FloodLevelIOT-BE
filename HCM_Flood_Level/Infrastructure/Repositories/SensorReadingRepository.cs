using Core.Entities;
using Core.Interfaces;
using Infrastructure.DBContext;
using System;

namespace Infrastructure.Repositories
{
    public class SensorReadingRepository : ISensorReadingRepository
    {
        


        private readonly ManageDBContext _context;
        private readonly EventsDBContext _eventsContext;



        public SensorReadingRepository(EventsDBContext eventsDBContext)
        {
            _eventsContext = eventsDBContext; 
        }
        public async Task AddAsync(SensorReading reading)
        {
            await _eventsContext.SensorReadings.AddAsync(reading);
            await _eventsContext.SaveChangesAsync();
        }
    }
}