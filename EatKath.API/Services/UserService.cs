using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.User;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using EatKath.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EatKath.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly PasswordHasher<User> _passwordHasher = new();

        private const string AdminRoleName = "Admin";

        public UserService(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
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
                throw new BusinessRuleException("Email already exists.");

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
                throw new BusinessRuleException("Email already exists.");

            // Self-protection and last-active-admin safeguards only
            // apply when the user being updated is currently an Admin.
            // Resolved by RoleId (not the Role navigation property) so
            // a user whose RoleId has no matching Role row - legacy/
            // test data - is safely treated as not-Admin rather than
            // risking an unresolved-navigation query translation.
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == AdminRoleName);

            var userIsAdmin = adminRole != null && user.RoleId == adminRole.Id;

            if (userIsAdmin)
            {
                var staysAdmin = dto.RoleId == adminRole!.Id;
                var isSelf = id == _currentUser.UserId;

                if (isSelf)
                {
                    if (!dto.IsActive)
                        throw new BusinessRuleException("You cannot deactivate your own account.");

                    if (!staysAdmin)
                        throw new BusinessRuleException("You cannot change your own role away from Admin.");
                }

                var wouldLoseAdminStatus = !dto.IsActive || !staysAdmin;

                if (wouldLoseAdminStatus)
                {
                    var otherActiveAdmins = await _context.Users
                        .CountAsync(u =>
                            u.Id != id &&
                            u.IsActive &&
                            u.RoleId == adminRole.Id);

                    if (otherActiveAdmins == 0)
                    {
                        throw new BusinessRuleException(
                            !dto.IsActive
                                ? "Cannot deactivate the last remaining active Admin."
                                : "Cannot change the role of the last remaining active Admin.");
                    }
                }
            }

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

            if (id == _currentUser.UserId)
                throw new BusinessRuleException("You cannot delete your own account.");

            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == AdminRoleName);

            var userIsActiveAdmin =
                adminRole != null &&
                user.RoleId == adminRole.Id &&
                user.IsActive;

            if (userIsActiveAdmin)
            {
                var otherActiveAdmins = await _context.Users
                    .CountAsync(u =>
                        u.Id != id &&
                        u.IsActive &&
                        u.RoleId == adminRole!.Id);

                if (otherActiveAdmins == 0)
                    throw new BusinessRuleException("Cannot delete the last remaining active Admin.");
            }

            var hasDependentRecords =
                await _context.Reservations.AnyAsync(r => r.UserId == id) ||
                await _context.Redemptions.AnyAsync(r => r.UserId == id) ||
                await _context.Restaurants.AnyAsync(r => r.OwnerId == id);

            if (hasDependentRecords)
            {
                throw new BusinessRuleException(
                    "Cannot delete this user because they have existing reservations, redemptions, or restaurants.");
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}