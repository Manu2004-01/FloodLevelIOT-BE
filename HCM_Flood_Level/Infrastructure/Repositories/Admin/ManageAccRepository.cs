using AutoMapper;
using Core.DTOs.Admin;
using Core.Entities;
using Core.Interfaces.Admin;
using Core.Sharing;
using Infrastructure.DBContext;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Admin
{
    public class ManageAccRepository : GenericRepository<User>, IManageAccRepository
    {
        private readonly ManageDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public ManageAccRepository(ManageDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddNewAccAsync(CreateAccDTO dto)
        {
            var acc = _mapper.Map<User>(dto);
            
            // Set default values for Status and CreatedAt
            acc.Status = "Active";
            acc.CreatedAt = DateTime.UtcNow;

            await _context.Users.AddAsync(acc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllAccAsync(EntityParam entityParam)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(entityParam.Search))
            {
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(entityParam.Search) ||
                    u.Username.ToLower().Contains(entityParam.Search) ||
                    u.Email.ToLower().Contains(entityParam.Search));
            }

            query = query
                .Skip((entityParam.Pagenumber - 1) * entityParam.Pagesize)
                .Take(entityParam.Pagesize);

            return await query.ToListAsync();
        }
    }
}
