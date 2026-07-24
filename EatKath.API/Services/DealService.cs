using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Deal;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;


namespace EatKath.API.Services
{
    public class DealService : IDealService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

     public DealService(
             ApplicationDbContext context,
             IMapper mapper,
             IHttpContextAccessor httpContextAccessor)
                {
                    _context = context;
                    _mapper = mapper;
                    _httpContextAccessor = httpContextAccessor;
                }

        public async Task<DealDto> CreateAsync(CreateDealDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var role = user.FindFirst(ClaimTypes.Role)!.Value;

            var restaurantExists = await _context.Restaurants
                .AnyAsync(r => r.Id == dto.RestaurantId);

            if (!restaurantExists)
                throw new Exception("Restaurant not found.");

            var deal = _mapper.Map<Deal>(dto);

            _context.Deals.Add(deal);

            await _context.SaveChangesAsync();

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return false;

            _context.Deals.Remove(deal);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<DealDto>> GetAllAsync()
        {
            var deals = await _context.Deals
                .ToListAsync();

            return _mapper.Map<IEnumerable<DealDto>>(deals);
        }

        public async Task<DealDto?> GetByIdAsync(int id)
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return null;

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<DealDto> UpdateAsync(int id, UpdateDealDto dto)
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                throw new Exception("Deal not found.");

            var restaurantExists = await _context.Restaurants
                .AnyAsync(r => r.Id == dto.RestaurantId);

            if (!restaurantExists)
                throw new Exception("Restaurant not found.");

            _mapper.Map(dto, deal);

            await _context.SaveChangesAsync();

            return _mapper.Map<DealDto>(deal);
        }
    }
}