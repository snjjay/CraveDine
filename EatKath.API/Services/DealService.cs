using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.Deal;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


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

        public async Task<IEnumerable<DealDto>> GetAllAsync()
        {
            var deals = await _context.Deals
                .Include(d => d.Restaurant)
                .OrderBy(d => d.Title)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DealDto>>(deals);
        }

        public async Task<DealDto?> GetByIdAsync(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return null;

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<DealDto> CreateAsync(CreateDealDto dto)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == dto.RestaurantId);

            if (restaurant == null)
                throw new Exception("Restaurant not found.");

            var deal = _mapper.Map<Deal>(dto);

            deal.CreatedAt = DateTime.UtcNow;
            deal.UpdatedAt = DateTime.UtcNow;

            _context.Deals.Add(deal);

            await _context.SaveChangesAsync();

            await _context.Entry(deal)
                .Reference(d => d.Restaurant)
                .LoadAsync();

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<DealDto> UpdateAsync(int id, UpdateDealDto dto)
        {
            var deal = await _context.Deals
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                throw new Exception("Deal not found.");

            _mapper.Map(dto, deal);

            deal.UpdatedAt = DateTime.UtcNow;

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

        public async Task<IEnumerable<DealDto>> GetByRestaurantAsync(int restaurantId)
        {
            return await _context.Deals
                .Where(d => d.RestaurantId == restaurantId && d.IsActive)
                .ProjectTo<DealDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}