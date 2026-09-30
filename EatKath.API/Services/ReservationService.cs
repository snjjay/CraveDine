using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.Reservation;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EatKath.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public ReservationService(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        // -----------------------------
        // The exact composite unique index EF Core generated for the
        // active-reservation-uniqueness constraint (Phase 5O). Used to
        // recognize this SPECIFIC constraint violation and translate it
        // to the existing business exception, without swallowing any
        // other DbUpdateException.
        // -----------------------------
        private const string UniqueActiveReservationIndexName =
            "IX_Reservations_UserId_DealId_ReservationDate_ReservationTime";

        public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
        {
            // SERIALIZABLE ensures the duplicate-check and capacity-check
            // reads below take range locks that block a concurrent
            // transaction from inserting a conflicting Reservation for
            // this deal/slot until this transaction commits or rolls
            // back - closing the check-then-insert races identified in
            // the Phase 5O investigation. The whole operation (loading
            // the deal, both checks, both inserts) runs inside this one
            // transaction so Reservation and Redemption commit or roll
            // back together; if anything below throws before
            // CommitAsync() is reached, disposing the transaction here
            // rolls back everything written so far.
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var deal = await _context.Deals
                .Include(d => d.Restaurant)
                .FirstOrDefaultAsync(d => d.Id == dto.DealId);

            if (deal == null)
                throw new NotFoundException("Deal not found.");

            DealAvailabilityValidator.EnsureAvailable(
                deal,
                deal.Restaurant,
                dto.ReservationDate,
                dto.ReservationTime,
                dto.GuestCount);

            // A customer may only hold one active reservation for the
            // same deal/date/time. Cancelled/Rejected/NoShow reservations
            // don't count, so cancelling and rebooking the same slot
            // remains allowed. Guest count does not affect this check.
            // (Also enforced by a database filtered unique index as a
            // concurrency backstop - see the SaveChangesAsync try/catch
            // below.)
            var hasDuplicateActiveReservation = await _context.Reservations
                .AnyAsync(r =>
                    r.UserId == _currentUser.UserId &&
                    r.DealId == dto.DealId &&
                    r.ReservationDate == dto.ReservationDate &&
                    r.ReservationTime == dto.ReservationTime &&
                    r.Status != ReservationStatus.Cancelled &&
                    r.Status != ReservationStatus.Rejected &&
                    r.Status != ReservationStatus.NoShow);

            if (hasDuplicateActiveReservation)
                throw new BusinessRuleException("You already have a reservation for this deal at this date and time.");

            // Check reservation limit
            if (deal.ReservationLimit > 0)
            {
                var reservationCount = await _context.Reservations
                    .CountAsync(r =>
                        r.DealId == dto.DealId &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Rejected &&
                        r.Status != ReservationStatus.NoShow);

                if (reservationCount >= deal.ReservationLimit)
                    throw new BusinessRuleException("This deal is fully booked.");
            }

            var reservation = _mapper.Map<Reservation>(dto);

            reservation.UserId = _currentUser.UserId;
            _context.Reservations.Add(reservation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateActiveReservationViolation(ex))
            {
                // A concurrent request won the race and inserted the
                // matching active reservation first; the database's
                // filtered unique index rejected this one. Translate to
                // the same business error the application-level check
                // above would have thrown had it seen that row in time -
                // no SQL/constraint details are exposed to the caller.
                throw new BusinessRuleException("You already have a reservation for this deal at this date and time.");
            }

            // Automatically create redemption
            var redemption = new Redemption
            {
                ReservationId = reservation.Id,
                DealId = reservation.DealId,
                UserId = reservation.UserId,
                ArrivalDate = reservation.ReservationDate,
                ArrivalTime = reservation.ReservationTime,
                GuestCount = reservation.GuestCount,
                Status = RedemptionStatus.Redeemed,
                RedeemedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Redemptions.Add(redemption);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        // -----------------------------
        // Recognizes SQL Server's "duplicate key" errors (2601/2627)
        // specifically for the active-reservation unique index by name,
        // so only that exact constraint is translated into a business
        // exception - any other DbUpdateException (a different
        // constraint, a transient failure, etc.) is left to propagate
        // unchanged to the existing exception middleware.
        // -----------------------------
        private static bool IsDuplicateActiveReservationViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627) &&
                sqlEx.Message.Contains(
                    UniqueActiveReservationIndexName,
                    StringComparison.OrdinalIgnoreCase);
        }

        public async Task<IEnumerable<ReservationDto>> GetAllAsync()
        {
            return await _context.Reservations
                .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IEnumerable<OwnerReservationDto>> GetOwnerReservationsAsync(int ownerId)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .Where(r => r.Deal.Restaurant.OwnerId == ownerId)
                .ToListAsync();

            var result = new List<OwnerReservationDto>();

            foreach (var reservation in reservations)
            {
                // Every redemption is created with an exact ReservationId
                // link (ReservationService.CreateAsync). No fallback
                // match is attempted - a reservation with no linked
                // redemption simply reports RedemptionId = null rather
                // than guessing at an unrelated redemption.
                var redemption = await _context.Redemptions
                    .FirstOrDefaultAsync(r => r.ReservationId == reservation.Id);

                result.Add(new OwnerReservationDto
                {
                    Id = reservation.Id,
                    RedemptionId = redemption?.Id,
                    DealId = reservation.DealId,
                    DealTitle = reservation.Deal.Title,
                    CustomerName = reservation.CustomerName,
                    PhoneNumber = reservation.PhoneNumber,
                    Email = reservation.Email,
                    ReservationDate = reservation.ReservationDate,
                    ReservationTime = reservation.ReservationTime,
                    GuestCount = reservation.GuestCount,
                    Status = reservation.Status
                });
            }

            return result;
        }


        public async Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return null;

            var isOwnReservation = reservation.UserId == _currentUser.UserId;
            var isOwnRestaurant = reservation.Deal.Restaurant.OwnerId == _currentUser.UserId;

            if (!_currentUser.IsAdmin && !isOwnReservation && !isOwnRestaurant)
                return null;

            return _mapper.Map<ReservationDto>(reservation);
        }

        private async Task<Reservation?> FindWithRestaurantAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        private void EnsureOwnership(Reservation reservation, string action)
        {
            if (!_currentUser.IsAdmin &&
                reservation.Deal.Restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException($"You are not authorized to {action}.");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "delete this reservation");

            _context.Reservations.Remove(reservation);

            await _context.SaveChangesAsync();

            return true;
        }

        // -----------------------------
        // Confirmation is not part of the active MVP reservation
        // workflow (Pending -> Completed/NoShow/Cancelled only).
        // Ownership is still enforced before rejecting the call,
        // so authorization behavior is unchanged.
        // -----------------------------
        public async Task<bool> ConfirmReservationAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "confirm this reservation");

            throw new BusinessRuleException("Reservation confirmation is not part of the active reservation workflow.");
        }

        public async Task<bool> CancelReservationAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "cancel this reservation");

            if (reservation.Status != ReservationStatus.Pending)
                throw new BusinessRuleException("This reservation cannot be cancelled because it is not pending.");

            reservation.Status = ReservationStatus.Cancelled;

            await CancelLinkedRedemptionAsync(reservation.Id);

            await _context.SaveChangesAsync();

            return true;
        }

        // -----------------------------
        // Customer self-service cancellation.
        // Separate from CancelReservationAsync/EnsureOwnership,
        // which authorize on restaurant ownership - this checks
        // reservation.UserId instead, since a customer cancelling
        // their own booking has no relationship to who owns the
        // restaurant.
        // -----------------------------
        public async Task<bool> CancelMyReservationAsync(int reservationId, int userId)
        {
            var reservation = await FindWithRestaurantAsync(reservationId);

            if (reservation == null)
                throw new NotFoundException("Reservation not found.");

            if (reservation.UserId != userId)
                throw new BusinessRuleException("You are not authorized to cancel this reservation.");

            if (reservation.Status != ReservationStatus.Pending)
            {
                throw new BusinessRuleException("This reservation cannot be cancelled.");
            }

            reservation.Status = ReservationStatus.Cancelled;

            await CancelLinkedRedemptionAsync(reservation.Id);

            await _context.SaveChangesAsync();

            return true;
        }

        // -----------------------------
        // Cancels the Redemption linked to this reservation via
        // Redemption.ReservationId (the exact FK from Phase 3C),
        // never by UserId+DealId+Date+Time - that combination is
        // not guaranteed unique and could target the wrong row.
        // Silently does nothing if no redemption is linked (legacy
        // data, or a redemption that was never auto-created).
        //
        // Never touches a redemption that is already Completed -
        // a completed sale's record must not be retroactively
        // cancelled as a side effect of reservation cancellation.
        // -----------------------------
        private async Task CancelLinkedRedemptionAsync(int reservationId)
        {
            var redemption = await _context.Redemptions
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (redemption != null && redemption.Status != RedemptionStatus.Completed)
                redemption.Status = RedemptionStatus.Cancelled;
        }

        // -----------------------------
        // Rejection is not part of the active MVP reservation
        // workflow. See ConfirmReservationAsync.
        // -----------------------------
        public async Task<bool> RejectReservationAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "reject this reservation");

            throw new BusinessRuleException("Reservation rejection is not part of the active reservation workflow.");
        }

        // -----------------------------
        // Arrival status is not part of the active MVP reservation
        // workflow. See ConfirmReservationAsync.
        // -----------------------------
        public async Task<bool> ArriveReservationAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "mark this reservation as arrived");

            throw new BusinessRuleException("Arrival status is not part of the active reservation workflow.");
        }

        // -----------------------------
        // This endpoint takes no bill amount, but completing a linked
        // Redemption requires one (RedemptionService.CompleteRedemptionAsync
        // computes DiscountAmount/FinalAmount from it). Rather than
        // invent a bill amount or leave a "Completed" redemption with
        // no financial data, a Pending reservation with a linked
        // redemption must be completed through the redemption
        // completion flow (POST /api/redemption/{id}/complete), which
        // completes both together. This still requires the reservation
        // to be Pending, so if a linked redemption exists here, it can
        // only be in its initial Redeemed state (see
        // CancelLinkedRedemptionAsync/NoShowReservationAsync - every
        // other path that changes redemption status also moves the
        // reservation off Pending in the same call).
        // -----------------------------
        public async Task<bool> CompleteReservationAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "complete this reservation");

            if (reservation.Status != ReservationStatus.Pending)
                throw new BusinessRuleException("This reservation cannot be completed because it is not pending.");

            var linkedRedemption = await _context.Redemptions
                .FirstOrDefaultAsync(r => r.ReservationId == reservation.Id);

            if (linkedRedemption != null)
            {
                throw new BusinessRuleException("This reservation must be completed by completing its redemption, which requires a bill amount.");
            }

            reservation.Status = ReservationStatus.Completed;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> NoShowReservationAsync(int id)
        {
            var reservation = await FindWithRestaurantAsync(id);

            if (reservation == null)
                return false;

            EnsureOwnership(reservation, "mark this reservation as a no-show");

            if (reservation.Status != ReservationStatus.Pending)
                throw new BusinessRuleException("This reservation cannot be marked as no-show because it is not pending.");

            reservation.Status = ReservationStatus.NoShow;

            await CancelLinkedRedemptionAsync(reservation.Id);

            await _context.SaveChangesAsync();

            return true;
        }

    }
}