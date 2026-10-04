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
        private readonly ICurrentUserService _currentUser;
        private readonly TimeProvider _timeProvider;

        public DealService(
            ApplicationDbContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUser,
            TimeProvider timeProvider)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
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
                throw new NotFoundException("Restaurant not found.");

            if (!_currentUser.IsAdmin &&
                restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to add deals to this restaurant.");
            }

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

        public async Task<DealDto> UpdateAsync(
            int id,
            UpdateDealDto dto)
        {
            var deal = await _context.Deals
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                throw new NotFoundException("Deal not found.");

            if (!_currentUser.IsAdmin &&
                deal.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to modify this deal.");
            }

            _mapper.Map(dto, deal);

            deal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<DealDto>(deal);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return false;

            if (!_currentUser.IsAdmin &&
                deal.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to delete this deal.");
            }

            deal.IsActive = false;
            deal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Customer-facing deal list for a restaurant, including how many
        // walk-in offers are left (see DealCapacityCalculator).
        public async Task<IEnumerable<DealDto>> GetByRestaurantAsync(
            int restaurantId)
        {
            var deals = await _context.Deals
                .Include(d => d.Restaurant)
                .Where(d =>
                    d.RestaurantId == restaurantId &&
                    d.IsActive)
                .ToListAsync();

            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

            var result = new List<DealDto>();

            foreach (var deal in deals)
            {
                var dto = _mapper.Map<DealDto>(deal);

                if (deal.EndDate < today)
                {
                    // Ended - nothing left to claim.
                    dto.RemainingOffers = 0;
                }
                else
                {
                    var availabilityDate = deal.StartDate > today ? deal.StartDate : today;

                    var capacity = await DealCapacityCalculator.GetCapacityAsync(
                        _context,
                        deal,
                        availabilityDate);

                    dto.AvailabilityDate = availabilityDate;
                    dto.RemainingOffers = capacity.Remaining;
                    dto.IsSoldOut = capacity.TotalRemaining == 0;
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task<IEnumerable<DealDto>> GetByOwnerAsync(
            int ownerId,
            int? restaurantId = null)
        {
            var query = _context.Deals
                .Where(d =>
                    d.Restaurant.OwnerId == ownerId &&
                    d.IsActive);

            if (restaurantId.HasValue)
            {
                query = query.Where(d =>
                    d.RestaurantId == restaurantId.Value);
            }

            return await query
                .ProjectTo<DealDto>(
                    _mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}