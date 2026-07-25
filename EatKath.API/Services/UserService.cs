using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.User;
using EatKath.API.Entities;
using EatKath.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EatKath.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (exists)
                throw new Exception("Email already exists.");

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            await _context.Entry(user)
                .Reference(u => u.Role)
                .LoadAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (emailExists)
                throw new Exception("Email already exists.");

            _mapper.Map(dto, user);

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _context.Entry(user)
                .Reference(u => u.Role)
                .LoadAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return false;

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}