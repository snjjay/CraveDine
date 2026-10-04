using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Redemption;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EatKath.API.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly TimeProvider _timeProvider;

        // SQL Server error raised when a transaction is chosen as a
        // deadlock victim.
        private const int SqlDeadlockErrorNumber = 1205;

        public RedemptionService(
            ApplicationDbContext context,
            ICurrentUserService currentUser,
            IMapper mapper,
            TimeProvider timeProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _mapper = mapper;
            _timeProvider = timeProvider;
        }

        // ==========================================================
        // Walk-in offer redemption (customer claims a deal)
        // ==========================================================
        //
        // Creates a Redemption WITHOUT a Reservation. The customer's
        // name, phone and email come from their account (User), never
        // from the request.
        //
        // Runs in a SERIALIZABLE transaction so that concurrent claims
        // cannot both pass the duplicate/limit checks and overbook the
        // offer. If SQL Server resolves a concurrent conflict by
        // choosing this request as a deadlock victim, nothing was
        // saved and the customer is asked to try again.
        public async Task<RedemptionDto> RedeemAsync(CreateRedemptionDto dto)
        {
            try
            {
                return await RedeemInTransactionAsync(dto);
            }
            catch (Exception ex) when (IsDeadlock(ex))
            {
                throw new BusinessRuleException("This offer is in high demand right now. Please try again.");
            }
        }

        private async Task<RedemptionDto> RedeemInTransactionAsync(CreateRedemptionDto dto)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var deal = await _context.Deals
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == dto.DealId);

            if (deal == null)
                throw new NotFoundException("Deal not found.");

            if (dto.GuestCount < 1)
                throw new BusinessRuleException("At least 1 guest is required.");

            // Active deal/restaurant, arrival date within the deal
            // dates, arrival time within the deal's arrival window and
            // the deal's maximum guests.
            DealAvailabilityValidator.EnsureAvailable(
                deal,
                deal.Restaurant,
                dto.ArrivalDate,
                dto.ArrivalTime,
                dto.GuestCount);

            var now = _timeProvider.GetLocalNow().DateTime;
            var today = DateOnly.FromDateTime(now);

            if (dto.ArrivalDate < today)
                throw new BusinessRuleException("Arrival date cannot be in the past.");

            if (dto.ArrivalDate == today &&
                dto.ArrivalTime < TimeOnly.FromDateTime(now))
            {
                throw new BusinessRuleException("The selected arrival time has already passed.");
            }

            // One active claim per customer, deal and arrival date.
            var hasActiveClaim = await DealCapacityCalculator
                .ActiveRedemptions(_context, deal.Id)
                .AnyAsync(r =>
                    r.UserId == _currentUser.UserId &&
                    r.ArrivalDate == dto.ArrivalDate);

            if (hasActiveClaim)
                throw new BusinessRuleException("You have already redeemed this offer for the selected date.");

            var capacity = await DealCapacityCalculator.GetCapacityAsync(
                _context,
                deal,
                dto.ArrivalDate);

            if (capacity.TotalRemaining == 0)
                throw new BusinessRuleException("This offer has been fully redeemed.");

            if (capacity.DailyRemaining == 0)
                throw new BusinessRuleException("No offers are left for the selected date.");

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

            await transaction.CommitAsync();

            await _context.Entry(redemption)
                .Reference(r => r.User)
                .LoadAsync();

            return _mapper.Map<RedemptionDto>(redemption);
        }

        private static bool IsDeadlock(Exception ex)
        {
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current is SqlException sqlEx && sqlEx.Number == SqlDeadlockErrorNumber)
                    return true;
            }

            return false;
        }

        // Customer cancels their own claim before visiting.
        public async Task<RedemptionDto> CancelMyRedemptionAsync(int redemptionId)
        {
            var redemption = await FindWithDetailsAsync(redemptionId);

            if (redemption == null)
                throw new NotFoundException("Redemption not found.");

            if (redemption.UserId != _currentUser.UserId)
                throw new BusinessRuleException("You are not authorized to cancel this redemption.");

            return await CancelAsync(redemption);
        }

        // Restaurant owner (or Admin) cancels a claim, e.g. when the
        // customer never arrived.
        public async Task<RedemptionDto> CancelRedemptionAsync(int redemptionId)
        {
            var redemption = await FindWithDetailsAsync(redemptionId);

            if (redemption == null)
                throw new NotFoundException("Redemption not found.");

            if (!_currentUser.IsAdmin &&
                redemption.Deal.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to cancel redemptions for this restaurant.");
            }

            return await CancelAsync(redemption);
        }

        // Only an open (Redeemed) claim can be cancelled. A claim that
        // came from a legacy reservation keeps both records in sync by
        // cancelling its still-pending reservation too.
        private async Task<RedemptionDto> CancelAsync(Redemption redemption)
        {
            if (redemption.Status != RedemptionStatus.Redeemed)
                throw new BusinessRuleException("Only redeemed offers that have not been completed can be cancelled.");

            if (redemption.ReservationId.HasValue)
            {
                var reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == redemption.ReservationId.Value);

                if (reservation != null && reservation.Status == ReservationStatus.Pending)
                    reservation.Status = ReservationStatus.Cancelled;
            }

            redemption.Status = RedemptionStatus.Cancelled;
            redemption.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<RedemptionDto>(redemption);
        }

        private async Task<Redemption?> FindWithDetailsAsync(int redemptionId)
        {
            return await _context.Redemptions
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == redemptionId);
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

            if (redemption.Status != RedemptionStatus.Redeemed)
                throw new BusinessRuleException("This redemption has been cancelled or has expired and cannot be completed.");

            if (!_currentUser.IsAdmin &&
                redemption.Deal.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException("You are not authorized to complete redemptions for this restaurant.");
            }

            // Redemptions created by the legacy reservation flow
            // (ReservationService.CreateAsync) carry the exact
            // ReservationId link. Walk-in claims (RedeemAsync) and
            // seed data have ReservationId == null; those have no
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
                    .ThenInclude(d => d.Restaurant)
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
                    .ThenInclude(d => d.Restaurant)
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