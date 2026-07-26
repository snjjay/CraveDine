using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Redemption;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRedemptionDto> _validator;

        public RedemptionService(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IMapper mapper,
            IValidator<CreateRedemptionDto> validator)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<RedemptionDto> RedeemAsync(CreateRedemptionDto dto)
        {
            var validation = await _validator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var deal = await _context.Deals
                .Include(x => x.Restaurant)
                .FirstOrDefaultAsync(x => x.Id == dto.DealId);

            if (deal == null)
                throw new Exception("Deal not found.");

            if (!deal.IsActive)
                throw new Exception("Deal is not active.");

            if (deal.EndDate < DateTime.UtcNow)
                throw new Exception("Deal has expired.");

            if (!deal.Restaurant.IsActive)
                throw new Exception("Restaurant is inactive.");

            var redemption = new Redemption
            {
                DealId = deal.Id,
                UserId = _currentUser.UserId,
                RedemptionAmount = deal.DiscountedPrice,
                RedeemedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Redemptions.Add(redemption);

            await _context.SaveChangesAsync();

            await _context.Entry(redemption)
                .Reference(x => x.Deal)
                .LoadAsync();

            await _context.Entry(redemption)
                .Reference(x => x.User)
                .LoadAsync();

            return _mapper.Map<RedemptionDto>(redemption);
        }

        public async Task<IEnumerable<RedemptionDto>> GetMyHistoryAsync()
        {
            var items = await _context.Redemptions
                .Include(x => x.Deal)
                .Include(x => x.User)
                .Where(x => x.UserId == _currentUser.UserId)
                .OrderByDescending(x => x.RedeemedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RedemptionDto>>(items);
        }

        public async Task<IEnumerable<RedemptionDto>> GetRestaurantRedemptionsAsync(int restaurantId)
        {
            var items = await _context.Redemptions
                .Include(x => x.Deal)
                .Include(x => x.User)
                .Where(x => x.Deal.RestaurantId == restaurantId)
                .OrderByDescending(x => x.RedeemedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RedemptionDto>>(items);
        }

        public async Task<RedemptionDto?> GetByIdAsync(int id)
        {
            var redemption = await _context.Redemptions
                .Include(x => x.Deal)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (redemption == null)
                return null;

            return _mapper.Map<RedemptionDto>(redemption);
        }
    }
}