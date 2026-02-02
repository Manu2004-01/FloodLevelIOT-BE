using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Core.Sharing;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ManageStaffRepository : GenericRepository<Staff>, IManageStaffRepository
    {
        private readonly ManageDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public ManageStaffRepository(ManageDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddNewStaffAsync(CreateStaffDTO dto)
        {
            var acc = _mapper.Map<Staff>(dto);
            
            if (!string.IsNullOrEmpty(dto.Password))
            {
                acc.PasswordHash = PasswordHelper.HashPassword(dto.Password);
            }
            
            acc.IsActive = true;
            acc.CreatedAt = DateTime.UtcNow;

            await _context.Staffs.AddAsync(acc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            var currentUser = await _context.Staffs.FindAsync(id);

            _context.Staffs.Remove(currentUser);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Staff>> GetAllStaffAsync(EntityParam entityParam)
        {
            var query = _context.Staffs
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(entityParam.Search))
            {
                query = query.Where(u => 
                    u.FullName.ToLower().Contains(entityParam.Search) ||
                    u.StaffAccName.ToLower().Contains(entityParam.Search) ||
                    u.Email.ToLower().Contains(entityParam.Search));
            }

            if (entityParam.RoleId.HasValue)
            {
                query = query.Where(u => u.RoleId == entityParam.RoleId.Value);
            }

            query = query.OrderBy(u => u.FullName)
                         .Skip((entityParam.Pagenumber - 1) * entityParam.Pagesize)
                         .Take(entityParam.Pagesize);

            return await query.ToListAsync();
        }

        public async Task<bool> UpdateStaffAsync(int id, UpdateStaffDTO dto)
        {
            var currentUser = await _context.Staffs.FindAsync(id);

            if (currentUser == null)
                return false;

            // Only update fields that are provided (partial update)
            if (dto.RoleId.HasValue)
            {
                currentUser.RoleId = dto.RoleId.Value;
            }

            if (dto.Status.HasValue)
                currentUser.IsActive = dto.Status.Value;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
