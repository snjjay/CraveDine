using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Auth;
using EatKath.API.Interfaces;
using EatKath.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EatKath.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<Entities.User> _passwordHasher;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;

            _passwordHasher = new PasswordHasher<Entities.User>();
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (existingUser)
                throw new Exception("Email already exists.");

            // Automatically assign the User role
            var userRole = await _context.Roles
                .FirstAsync(r => r.Name == "Customer");

            var user = new Entities.User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                RoleId = userRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user.PasswordHash = HashPassword(user, dto.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            await _context.Entry(user)
                .Reference(u => u.Role)
                .LoadAsync();

            return GenerateJwtToken(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("Invalid email or password.");

            if (!user.IsActive)
                throw new Exception("User account is inactive.");

            var isPasswordValid = VerifyPassword(
                user,
                dto.Password,
                user.PasswordHash);

            if (!isPasswordValid)
                throw new Exception("Invalid email or password.");

            return GenerateJwtToken(user);
        }

        private string HashPassword(Entities.User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        private bool VerifyPassword(
            Entities.User user,
            string password,
            string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                passwordHash,
                password);

            return result == PasswordVerificationResult.Success;
        }

        private AuthResponseDto GenerateJwtToken(Entities.User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(jwtSettings["ExpiryInMinutes"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new AuthResponseDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.Name,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            };
        }
    }
}