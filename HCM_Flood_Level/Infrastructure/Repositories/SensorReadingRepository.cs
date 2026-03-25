using Core.Entities;
using Core.Interfaces;
using Infrastructure.DBContext;
using System;

namespace Infrastructure.Repositories
{
    public class SensorReadingRepository : ISensorReadingRepository
    {
        


        //private readonly ManageDBContext _context;
        private readonly EventsDBContext _eventsContext;

        //public SensorReadingRepository(ManageDBContext context)
        //{
        //    _context = context;
        //}

        public SensorReadingRepository(EventsDBContext eventsDBContext)
        {
            eventsDBContext = eventsDBContext;
        }
        public async Task AddAsync(SensorReading reading)
        {
            
            //await _context.SensorReading.AddAsync(reading);
            //await _context.SaveChangesAsync();

            await _eventsContext.SensorReadings.AddAsync(reading);
            await _eventsContext.SaveChangesAsync();
        }
    }
}