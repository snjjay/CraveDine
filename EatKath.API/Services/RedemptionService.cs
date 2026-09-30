using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Redemption;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public RedemptionService(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<RedemptionDto> CompleteRedemptionAsync(
            int redemptionId,
            CompleteRedemptionDto dto)
        {
            var redemption = await _context.Redemptions
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == redemptionId);

            if (redemption == null)
                throw new NotFoundException("Redemption not found.");

            if (redemption.Status == RedemptionStatus.Completed)
                throw new BusinessRuleException("Redemption has already been completed.");

            if (!_currentUser.IsAdmin &&
                redemption.Deal.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to complete redemptions for this restaurant.");
            }

            // Every redemption created by the current flow
            // (ReservationService.CreateAsync) carries the exact
            // ReservationId link. Only pre-existing/legacy rows can have
            // ReservationId == null (e.g. seed data); those have no
            // reservation to validate against, so they are completed
            // without any reservation-status check - guessing at a
            // matching reservation by UserId/DealId/date/time is no
            // longer done, since that match is inherently ambiguous and
            // could silently touch an unrelated reservation.
            Reservation? reservation = null;

            if (redemption.ReservationId.HasValue)
            {
                reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == redemption.ReservationId.Value);

                // Active MVP lifecycle: a reservation-linked redemption
                // may only be completed while its reservation is still
                // Pending (Pending -> Completed is the only active
                // completion transition; Confirmed/Arrived/Rejected are
                // not active workflow states, and NoShow/Cancelled/
                // Completed are terminal).
                if (reservation == null || reservation.Status != ReservationStatus.Pending)
                {
                    throw new BusinessRuleException("This redemption cannot be completed because the reservation is not pending.");
                }
            }

            redemption.BillAmount = dto.BillAmount;

            redemption.DiscountAmount =
                Math.Round(
                    dto.BillAmount * redemption.Deal.DiscountPercentage / 100m,
                    2);

            redemption.FinalAmount =
                dto.BillAmount - redemption.DiscountAmount;

            redemption.Status = RedemptionStatus.Completed;

            if (reservation != null)
            {
                reservation.Status = ReservationStatus.Completed;
            }

            redemption.CompletedAt = DateTime.UtcNow;
            redemption.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(x => x.Id == restaurantId);

            if (restaurant != null &&
                !_currentUser.IsAdmin &&
                restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to view redemptions for this restaurant.");
            }

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
                    .ThenInclude(d => d.Restaurant)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (redemption == null)
                return null;

            var isOwnRedemption = redemption.UserId == _currentUser.UserId;
            var isOwnRestaurant = redemption.Deal.Restaurant.OwnerId == _currentUser.UserId;

            if (!_currentUser.IsAdmin && !isOwnRedemption && !isOwnRestaurant)
                return null;

            return _mapper.Map<RedemptionDto>(redemption);
        }
    }
}