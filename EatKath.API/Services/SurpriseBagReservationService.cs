using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.SurpriseBagReservation;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EatKath.API.Services
{
    // Customer Surprise Bag reservations: reserve now, pay the restaurant at
    // pickup. Reserved and Collected reservations hold stock; Cancelled and
    // NoShow release it (see SurpriseBagAvailability). Times are Kathmandu
    // business time.
    //
    // Status changes: Reserved -> Cancelled (customer, before the pickup
    // window starts; or owner/Admin), Reserved -> Collected (owner/Admin, on
    // the pickup date), Reserved -> NoShow (owner/Admin, after the pickup
    // window ends). Every other change is rejected.
    public class SurpriseBagReservationService : ISurpriseBagReservationService
    {
        // SQL Server error raised when a transaction is chosen as a
        // deadlock victim.
        private const int SqlDeadlockErrorNumber = 1205;

        private const int MaxCodeAttempts = 3;

        // Unique indexes from the AddSurpriseBags migration, recognised by
        // name so only these exact violations become business errors.
        private const string ActiveReservationIndexName = "IX_SurpriseBagReservations_SurpriseBagId_UserId";
        private const string ConfirmationCodeIndexName = "IX_SurpriseBagReservations_ConfirmationCode";

        private const string DuplicateReservationMessage = "You already have a reservation for this Surprise Bag.";
        private const string HighDemandMessage = "This Surprise Bag is in high demand right now. Please try again.";
        private const string CodeUnavailableMessage = "We couldn't create your reservation right now. Please try again.";
        private const string ConcurrentChangeMessage = "This reservation was just updated. Please refresh and try again.";
        private const string ReservationNotFoundMessage = "Reservation not found.";
        private const string CodeNotFoundMessage = "No reservation was found with this confirmation code.";

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly TimeProvider _timeProvider;
        private readonly IConfirmationCodeGenerator _codeGenerator;
        private readonly IValidator<CreateSurpriseBagReservationDto> _createValidator;
        private readonly IValidator<CollectSurpriseBagReservationDto> _collectValidator;

        public SurpriseBagReservationService(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUser,
            TimeProvider timeProvider,
            IConfirmationCodeGenerator codeGenerator,
            IValidator<CreateSurpriseBagReservationDto> createValidator,
            IValidator<CollectSurpriseBagReservationDto> collectValidator)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
            _codeGenerator = codeGenerator;
            _createValidator = createValidator;
            _collectValidator = collectValidator;
        }

        // ==========================================================
        // Customer
        // ==========================================================

        // SERIALIZABLE, and the bag row is locked first (UPDLOCK, HOLDLOCK),
        // so competing reservations for the same bag queue up: each sees the
        // committed stock and per-customer totals, so the bag can't be
        // overbooked. The unique indexes are the database backstop; a
        // deadlock (e.g. with an owner editing the bag) asks the customer to
        // try again - nothing was saved.
        public async Task<SurpriseBagReservationDto> CreateAsync(CreateSurpriseBagReservationDto dto)
        {
            await ValidateAsync(_createValidator, dto);

            for (var attempt = 1; attempt <= MaxCodeAttempts; attempt++)
            {
                try
                {
                    return await CreateInTransactionAsync(dto);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex, ConfirmationCodeIndexName))
                {
                    // Another reservation took the same code at the same
                    // moment: nothing was saved; try again with a new code.
                    _context.ChangeTracker.Clear();
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex, ActiveReservationIndexName))
                {
                    throw new BusinessRuleException(DuplicateReservationMessage);
                }
                catch (Exception ex) when (IsDeadlock(ex))
                {
                    throw new BusinessRuleException(HighDemandMessage);
                }
            }

            throw new BusinessRuleException(CodeUnavailableMessage);
        }

        private async Task<SurpriseBagReservationDto> CreateInTransactionAsync(CreateSurpriseBagReservationDto dto)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            await LockRowsAsync($"SELECT 1 FROM SurpriseBags WITH (UPDLOCK, HOLDLOCK) WHERE Id = {dto.SurpriseBagId}");

            var bag = await _context.SurpriseBags
                .Include(b => b.Restaurant)
                .FirstOrDefaultAsync(b => b.Id == dto.SurpriseBagId);

            if (bag == null)
                throw new NotFoundException("Surprise Bag not found.");

            if (!bag.IsActive || !bag.Restaurant.IsActive)
                throw new BusinessRuleException("This Surprise Bag is not available.");

            var (today, now) = BusinessNow();

            if (SurpriseBagAvailability.HasPickupEnded(bag.PickupDate, bag.PickupEndTime, today, now))
                throw new BusinessRuleException("The pickup window for this Surprise Bag has ended.");

            var customerId = _currentUser.UserId;

            // One active (Reserved) reservation per customer per bag - also
            // enforced by the filtered unique index.
            var hasActiveReservation = await _context.SurpriseBagReservations
                .AnyAsync(r =>
                    r.SurpriseBagId == bag.Id &&
                    r.UserId == customerId &&
                    r.Status == SurpriseBagReservationStatus.Reserved);

            if (hasActiveReservation)
                throw new BusinessRuleException(DuplicateReservationMessage);

            // MaxPerCustomer covers what the customer already collected.
            var customerQuantity = await _context.SurpriseBagReservations
                .Where(r =>
                    r.SurpriseBagId == bag.Id &&
                    r.UserId == customerId &&
                    (r.Status == SurpriseBagReservationStatus.Reserved ||
                     r.Status == SurpriseBagReservationStatus.Collected))
                .SumAsync(r => r.Quantity);

            if (customerQuantity + dto.Quantity > bag.MaxPerCustomer)
            {
                throw new BusinessRuleException(customerQuantity == 0
                    ? $"You can reserve at most {bag.MaxPerCustomer} of this Surprise Bag."
                    : $"You can reserve at most {bag.MaxPerCustomer} of this Surprise Bag (you already have {customerQuantity}).");
            }

            var remaining = (await SurpriseBagAvailability.GetUsageAsync(_context, bag.Id))
                .RemainingOf(bag.QuantityTotal);

            if (remaining == 0)
                throw new BusinessRuleException("This Surprise Bag is sold out.");

            if (dto.Quantity > remaining)
                throw new BusinessRuleException($"Only {remaining} left.");

            var reservation = new SurpriseBagReservation
            {
                SurpriseBagId = bag.Id,
                SurpriseBag = bag,
                UserId = customerId,
                Quantity = dto.Quantity,

                // The price stays what was reserved, whatever happens to the bag.
                UnitPrice = bag.Price,
                ConfirmationCode = await NewConfirmationCodeAsync(),
                Status = SurpriseBagReservationStatus.Reserved
            };

            _context.SurpriseBagReservations.Add(reservation);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return ToCustomerDto(reservation);
        }

        // Current and past reservations, newest first - including bags that
        // are now inactive or past, with the price that was reserved.
        public async Task<IEnumerable<SurpriseBagReservationDto>> GetMyAsync()
        {
            var reservations = await WithDetails()
                .Where(r => r.UserId == _currentUser.UserId)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            return reservations.Select(ToCustomerDto).ToList();
        }

        public async Task<SurpriseBagReservationDto> CancelMineAsync(int id)
        {
            return await InTransitionAsync(async () =>
            {
                await LockRowsAsync($"SELECT 1 FROM SurpriseBagReservations WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}");

                var reservation = await WithDetails().FirstOrDefaultAsync(r => r.Id == id);

                if (reservation == null || reservation.UserId != _currentUser.UserId)
                    throw new NotFoundException(ReservationNotFoundMessage);

                EnsureReserved(reservation);

                var (today, now) = BusinessNow();

                if (HasPickupStarted(reservation.SurpriseBag, today, now))
                    throw new BusinessRuleException("Reservations can only be cancelled before the pickup window starts.");

                MarkCancelled(reservation);

                await _context.SaveChangesAsync();

                return ToCustomerDto(reservation);
            });
        }

        // The customer sees their own view; the restaurant's owner or an
        // Admin also sees the customer's details. Anyone else: null (404).
        public async Task<SurpriseBagReservationDto?> GetByIdAsync(int id)
        {
            var reservation = await WithDetails().FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return null;

            if (reservation.UserId == _currentUser.UserId)
                return ToCustomerDto(reservation);

            if (CanManage(reservation.SurpriseBag.Restaurant))
                return ToOwnerDto(reservation);

            return null;
        }

        // ==========================================================
        // Restaurant owner / Admin
        // ==========================================================

        public async Task<IEnumerable<OwnerSurpriseBagReservationDto>> GetForRestaurantAsync(
            int restaurantId,
            DateOnly? pickupDate = null,
            SurpriseBagReservationStatus? status = null)
        {
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null || !CanManage(restaurant))
                throw new NotFoundException("Restaurant not found.");

            var query = WithDetails()
                .Where(r => r.SurpriseBag.RestaurantId == restaurantId);

            if (pickupDate.HasValue)
                query = query.Where(r => r.SurpriseBag.PickupDate == pickupDate.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var reservations = await query
                .OrderByDescending(r => r.SurpriseBag.PickupDate)
                .ThenBy(r => r.SurpriseBag.PickupStartTime)
                .ThenBy(r => r.CreatedAt)
                .ToListAsync();

            return reservations.Select(ToOwnerDto).ToList();
        }

        // E.g. when a bag is deactivated or the restaurant runs short. The
        // cancellation is recorded (CancelledAt) and the stock released.
        public async Task<OwnerSurpriseBagReservationDto> CancelAsync(int id)
        {
            return await InTransitionAsync(async () =>
            {
                var reservation = await LockAndLoadManagedAsync(id);

                EnsureReserved(reservation);

                MarkCancelled(reservation);

                await _context.SaveChangesAsync();

                return ToOwnerDto(reservation);
            });
        }

        // The customer pays at the counter; staff enter their code. Only the
        // caller's own restaurants are searched, and an unknown code and
        // another restaurant's code give the same answer.
        public async Task<OwnerSurpriseBagReservationDto> CollectAsync(CollectSurpriseBagReservationDto dto)
        {
            await ValidateAsync(_collectValidator, dto);

            var code = ConfirmationCodeGenerator.Normalize(dto.ConfirmationCode);

            return await InTransitionAsync(async () =>
            {
                // Lock the reservation row by Id - the same lock a cancel or
                // no-show takes - so a collection and a cancellation of one
                // reservation queue instead of deadlocking.
                var id = await _context.SurpriseBagReservations
                    .Where(r => r.ConfirmationCode == code)
                    .Select(r => (int?)r.Id)
                    .FirstOrDefaultAsync();

                if (id == null)
                    throw new NotFoundException(CodeNotFoundMessage);

                await LockRowsAsync($"SELECT 1 FROM SurpriseBagReservations WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id.Value}");

                var reservation = await WithDetails().FirstOrDefaultAsync(r => r.Id == id.Value);

                if (reservation == null || !CanManage(reservation.SurpriseBag.Restaurant))
                    throw new NotFoundException(CodeNotFoundMessage);

                EnsureReserved(reservation);

                var (today, _) = BusinessNow();
                var pickupDate = reservation.SurpriseBag.PickupDate;

                if (today < pickupDate)
                    throw new BusinessRuleException($"This reservation can only be collected on its pickup date ({pickupDate:yyyy-MM-dd}).");

                if (today > pickupDate)
                    throw new BusinessRuleException("The pickup date for this reservation has passed. Mark it as a no-show instead.");

                reservation.Status = SurpriseBagReservationStatus.Collected;
                reservation.CollectedAt = DateTime.UtcNow;
                reservation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ToOwnerDto(reservation);
            });
        }

        public async Task<OwnerSurpriseBagReservationDto> MarkNoShowAsync(int id)
        {
            return await InTransitionAsync(async () =>
            {
                var reservation = await LockAndLoadManagedAsync(id);

                EnsureReserved(reservation);

                var (today, now) = BusinessNow();
                var bag = reservation.SurpriseBag;

                if (!SurpriseBagAvailability.HasPickupEnded(bag.PickupDate, bag.PickupEndTime, today, now))
                    throw new BusinessRuleException("A reservation can only be marked as a no-show after the pickup window ends.");

                reservation.Status = SurpriseBagReservationStatus.NoShow;
                reservation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ToOwnerDto(reservation);
            });
        }

        // ==========================================================
        // Helpers
        // ==========================================================

        private IQueryable<SurpriseBagReservation> WithDetails()
        {
            return _context.SurpriseBagReservations
                .Include(r => r.SurpriseBag)
                    .ThenInclude(b => b.Restaurant)
                .Include(r => r.User);
        }

        // Missing and other restaurants' reservations look the same: 404.
        private async Task<SurpriseBagReservation> LockAndLoadManagedAsync(int id)
        {
            await LockRowsAsync($"SELECT 1 FROM SurpriseBagReservations WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}");

            var reservation = await WithDetails().FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null || !CanManage(reservation.SurpriseBag.Restaurant))
                throw new NotFoundException(ReservationNotFoundMessage);

            return reservation;
        }

        // Status changes run SERIALIZABLE with the reservation row locked
        // first, so two changes to one reservation (e.g. a cancel and a
        // collection) happen one after the other: the second sees the first
        // and is rejected by the status check.
        private async Task<T> InTransitionAsync<T>(Func<Task<T>> transition)
        {
            try
            {
                await using var transaction = await _context.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable);

                var result = await transition();

                await transaction.CommitAsync();

                return result;
            }
            catch (Exception ex) when (IsDeadlock(ex))
            {
                throw new BusinessRuleException(ConcurrentChangeMessage);
            }
        }

        // SQL Server row lock held until the transaction ends. The InMemory
        // test provider has no locking (or SQL), so it is skipped there.
        private async Task LockRowsAsync(FormattableString lockQuery)
        {
            if (_context.Database.IsRelational())
                await _context.Database.ExecuteSqlInterpolatedAsync(lockQuery);
        }

        private async Task<string> NewConfirmationCodeAsync()
        {
            for (var attempt = 1; attempt <= MaxCodeAttempts; attempt++)
            {
                var code = _codeGenerator.Generate();

                if (!await _context.SurpriseBagReservations.AnyAsync(r => r.ConfirmationCode == code))
                    return code;
            }

            throw new BusinessRuleException(CodeUnavailableMessage);
        }

        private static void EnsureReserved(SurpriseBagReservation reservation)
        {
            switch (reservation.Status)
            {
                case SurpriseBagReservationStatus.Reserved:
                    return;
                case SurpriseBagReservationStatus.Collected:
                    throw new BusinessRuleException("This reservation has already been collected.");
                case SurpriseBagReservationStatus.Cancelled:
                    throw new BusinessRuleException("This reservation has already been cancelled.");
                case SurpriseBagReservationStatus.NoShow:
                    throw new BusinessRuleException("This reservation has already been marked as a no-show.");
                default:
                    throw new BusinessRuleException("This reservation can no longer be changed.");
            }
        }

        private static void MarkCancelled(SurpriseBagReservation reservation)
        {
            reservation.Status = SurpriseBagReservationStatus.Cancelled;
            reservation.CancelledAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;
        }

        private bool CanManage(Restaurant restaurant)
        {
            return _currentUser.IsAdmin || restaurant.OwnerId == _currentUser.UserId;
        }

        private static bool HasPickupStarted(SurpriseBag bag, DateOnly today, TimeOnly now)
        {
            return bag.PickupDate < today || (bag.PickupDate == today && bag.PickupStartTime <= now);
        }

        // Kathmandu business date and time.
        private (DateOnly Today, TimeOnly Now) BusinessNow()
        {
            var local = _timeProvider.GetLocalNow().DateTime;

            return (DateOnly.FromDateTime(local), TimeOnly.FromDateTime(local));
        }

        private bool CanCustomerCancel(SurpriseBagReservation reservation)
        {
            var (today, now) = BusinessNow();

            return reservation.Status == SurpriseBagReservationStatus.Reserved &&
                !HasPickupStarted(reservation.SurpriseBag, today, now);
        }

        private SurpriseBagReservationDto ToCustomerDto(SurpriseBagReservation reservation)
        {
            var dto = _mapper.Map<SurpriseBagReservationDto>(reservation);

            dto.CanCancel = CanCustomerCancel(reservation);

            return dto;
        }

        private OwnerSurpriseBagReservationDto ToOwnerDto(SurpriseBagReservation reservation)
        {
            var dto = _mapper.Map<OwnerSurpriseBagReservationDto>(reservation);

            dto.CanCancel = CanCustomerCancel(reservation);

            return dto;
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T dto)
        {
            var validation = await validator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
        }

        private static bool IsUniqueViolation(DbUpdateException ex, string indexName)
        {
            return ex.InnerException is SqlException sqlEx &&
                (sqlEx.Number == 2601 || sqlEx.Number == 2627) &&
                sqlEx.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase);
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
    }
}
