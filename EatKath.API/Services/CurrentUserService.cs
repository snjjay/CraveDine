using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EatKath.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var claims = _httpContextAccessor.HttpContext?.User?.Claims;

                Console.WriteLine("========== JWT Claims ==========");

                foreach (var claim in claims ?? Enumerable.Empty<Claim>())
                {
                    Console.WriteLine($"{claim.Type} = {claim.Value}");
                }

                Console.WriteLine("================================");

                var userId = _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                Console.WriteLine($"NameIdentifier = {userId}");

                return int.TryParse(userId, out var id) ? id : 0;
            }
        }

        public string Role
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            }
        }

        public bool IsAdmin => Role == "Admin";
    }
}