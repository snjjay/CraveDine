using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Redemption;
using EatKath.API.Entities;
using EatKath.API.Enums;
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
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == dto.DealId);

            if (deal == null)
                throw new Exception("Offer not found.");

            if (!deal.IsActive)
                throw new Exception("Offer is inactive.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < deal.StartDate || today > deal.EndDate)
                throw new Exception("Offer is not available today.");

            if (!deal.Restaurant.IsActive)
                throw new Exception("Restaurant is inactive.");

            if (dto.GuestCount > deal.MaximumGuests)
                throw new Exception($"Maximum {deal.MaximumGuests} guests allowed.");

            var redemption = new Redemption
            {
                DealId = deal.Id,
                UserId = _currentUser.UserId,
                ArrivalDate = dto.ArrivalDate,
                ArrivalTime = dto.ArrivalTime,
                GuestCount = dto.GuestCount,

                Status = RedemptionStatus.Redeemed,

                RedeemedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Redemptions.Add(redemption);

            await _context.SaveChangesAsync();

            await _context.Entry(redemption)
                .Reference(r => r.Deal)
                .LoadAsync();

            await _context.Entry(redemption)
                .Reference(r => r.User)
                .LoadAsync();

            return _mapper.Map<RedemptionDto>(redemption);
        }

        public async Task<IEnumerable<RedemptionDto>> GetMyHistoryAsync()
        {
            var items = await _context.Redemptions
                .Include(r => r.Deal)
                .Include(r => r.User)
                .Where(r => r.UserId == _currentUser.UserId)
                .OrderByDescending(r => r.RedeemedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RedemptionDto>>(items);
        }

        public async Task<IEnumerable<RedemptionDto>> GetRestaurantRedemptionsAsync(int restaurantId)
        {
            var items = await _context.Redemptions
                .Include(r => r.Deal)
                .Include(r => r.User)
                .Where(r => r.Deal.RestaurantId == restaurantId)
                .OrderByDescending(r => r.RedeemedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RedemptionDto>>(items);
        }

        public async Task<RedemptionDto?> GetByIdAsync(int id)
        {
            var redemption = await _context.Redemptions
                .Include(r => r.Deal)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (redemption == null)
                return null;

            return _mapper.Map<RedemptionDto>(redemption);
        }
    }
}