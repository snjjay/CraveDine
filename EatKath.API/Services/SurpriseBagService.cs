using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.Entities;
using EatKath.API.Interfaces;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EatKath.API.Services
{
    // Surprise Bag listings: public browsing and owner/admin management.
    // Reservations are handled separately; here they only count toward
    // stock (see SurpriseBagAvailability).
    public class SurpriseBagService : ISurpriseBagService
    {
        // SQL Server error raised when a transaction is chosen as a
        // deadlock victim.
        private const int SqlDeadlockErrorNumber = 1205;

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly TimeProvider _timeProvider;
        private readonly IValidator<CreateSurpriseBagDto> _createValidator;
        private readonly IValidator<UpdateSurpriseBagDto> _updateValidator;

        public SurpriseBagService(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUser,
            TimeProvider timeProvider,
            IValidator<CreateSurpriseBagDto> createValidator,
            IValidator<UpdateSurpriseBagDto> updateValidator)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
            _timeProvider = timeProvider;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        // ==========================================================
        // Public
        // ==========================================================

        public async Task<IEnumerable<SurpriseBagDto>> GetPublicByRestaurantAsync(int restaurantId)
        {
            var (today, now) = BusinessNow();

            var bags = await _context.SurpriseBags
                .Include(b => b.Restaurant)
                .PubliclyVisible(today, now)
                .Where(b => b.RestaurantId == restaurantId)
                .OrderBy(b => b.PickupDate)
                .ThenBy(b => b.PickupStartTime)
                .ToListAsync();

            var usage = await SurpriseBagAvailability.GetUsageAsync(_context, bags.Select(b => b.Id).ToList());

            return bags
                .Select(b => ToPublicDto(b, usage.GetValueOrDefault(b.Id, SurpriseBagUsage.None)))
                .ToList();
        }

        public async Task<SurpriseBagDto?> GetPublicByIdAsync(int id)
        {
            var (today, now) = BusinessNow();

            var bag = await _context.SurpriseBags
                .Include(b => b.Restaurant)
                .PubliclyVisible(today, now)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bag == null)
                return null;

            return ToPublicDto(bag, await SurpriseBagAvailability.GetUsageAsync(_context, bag.Id));
        }

        // ==========================================================
        // Owner / Admin
        // ==========================================================

        public async Task<IEnumerable<OwnerSurpriseBagDto>> GetByOwnerAsync(int ownerId, int? restaurantId = null)
        {
            var query = _context.SurpriseBags
                .Include(b => b.Restaurant)
                .Where(b => b.Restaurant.OwnerId == ownerId);

            if (restaurantId.HasValue)
                query = query.Where(b => b.RestaurantId == restaurantId.Value);

            var bags = await query
                .OrderByDescending(b => b.PickupDate)
                .ThenByDescending(b => b.PickupStartTime)
                .ToListAsync();

            var usage = await SurpriseBagAvailability.GetUsageAsync(_context, bags.Select(b => b.Id).ToList());

            return bags
                .Select(b => ToOwnerDto(b, usage.GetValueOrDefault(b.Id, SurpriseBagUsage.None)))
                .ToList();
        }

        // One bag for its owner (or an Admin), whatever its state.
        public async Task<OwnerSurpriseBagDto?> GetManagedByIdAsync(int id)
        {
            var bag = await FindWithRestaurantAsync(id);

            if (bag == null)
                return null;

            EnsureCanManage(bag.Restaurant, "view this Surprise Bag");

            return ToOwnerDto(bag, await SurpriseBagAvailability.GetUsageAsync(_context, bag.Id));
        }

        public async Task<OwnerSurpriseBagDto> CreateAsync(CreateSurpriseBagDto dto)
        {
            await ValidateAsync(_createValidator, dto);

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == dto.RestaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureCanManage(restaurant, "add Surprise Bags to this restaurant");

            var (today, now) = BusinessNow();

            if (SurpriseBagAvailability.HasPickupEnded(dto.PickupDate, dto.PickupEndTime, today, now))
                throw new BusinessRuleException("The pickup window has already ended.");

            var bag = _mapper.Map<SurpriseBag>(dto);

            bag.Restaurant = restaurant;

            _context.SurpriseBags.Add(bag);

            await _context.SaveChangesAsync();

            return ToOwnerDto(bag, SurpriseBagUsage.None);
        }

        // SERIALIZABLE so the quantity and pickup-lock checks can't be
        // undermined by a reservation made at the same moment. If SQL Server
        // resolves a conflict by choosing this request as a deadlock victim,
        // nothing was saved and the owner is asked to try again.
        public async Task<OwnerSurpriseBagDto> UpdateAsync(int id, UpdateSurpriseBagDto dto)
        {
            await ValidateAsync(_updateValidator, dto);

            try
            {
                return await UpdateInTransactionAsync(id, dto);
            }
            catch (Exception ex) when (IsDeadlock(ex))
            {
                throw new BusinessRuleException("This Surprise Bag is being reserved right now. Please try again.");
            }
        }

        private async Task<OwnerSurpriseBagDto> UpdateInTransactionAsync(int id, UpdateSurpriseBagDto dto)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var bag = await FindWithRestaurantAsync(id);

            if (bag == null)
                throw new NotFoundException("Surprise Bag not found.");

            EnsureCanManage(bag.Restaurant, "modify this Surprise Bag");

            var usage = await SurpriseBagAvailability.GetUsageAsync(_context, bag.Id);

            if (dto.QuantityTotal < usage.UsedQuantity)
            {
                throw new BusinessRuleException(
                    $"Quantity cannot be less than the {usage.UsedQuantity} bags already reserved or collected.");
            }

            var largestCustomerQuantity =
                await SurpriseBagAvailability.GetLargestCustomerQuantityAsync(_context, bag.Id);

            if (dto.MaxPerCustomer < largestCustomerQuantity)
            {
                throw new BusinessRuleException(
                    $"Max per customer cannot be less than the {largestCustomerQuantity} bags one customer already holds.");
            }

            var pickupChanged =
                dto.PickupDate != bag.PickupDate ||
                dto.PickupStartTime != bag.PickupStartTime ||
                dto.PickupEndTime != bag.PickupEndTime;

            if (pickupChanged)
            {
                // Customers planned around the window they reserved.
                if (usage.UsedQuantity > 0)
                {
                    throw new BusinessRuleException(
                        "Pickup date and times cannot be changed once bags have been reserved or collected.");
                }

                var (today, now) = BusinessNow();

                if (SurpriseBagAvailability.HasPickupEnded(dto.PickupDate, dto.PickupEndTime, today, now))
                    throw new BusinessRuleException("The pickup window has already ended.");
            }

            // Price changes are fine: each reservation keeps its UnitPrice.
            _mapper.Map(dto, bag);

            bag.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return ToOwnerDto(bag, usage);
        }

        // Stops new reservations and hides the bag publicly; existing
        // reservations stay as they are.
        public async Task<bool> DeactivateAsync(int id)
        {
            var bag = await FindWithRestaurantAsync(id);

            if (bag == null)
                return false;

            EnsureCanManage(bag.Restaurant, "deactivate this Surprise Bag");

            bag.IsActive = false;
            bag.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================================
        // Helpers
        // ==========================================================

        private async Task<SurpriseBag?> FindWithRestaurantAsync(int id)
        {
            return await _context.SurpriseBags
                .Include(b => b.Restaurant)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        private void EnsureCanManage(Restaurant restaurant, string action)
        {
            if (!_currentUser.IsAdmin &&
                restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException($"You are not authorized to {action}.");
            }
        }

        private static async Task ValidateAsync<T>(IValidator<T> validator, T dto)
        {
            var validation = await validator.ValidateAsync(dto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
        }

        // Kathmandu business date and time.
        private (DateOnly Today, TimeOnly Now) BusinessNow()
        {
            var local = _timeProvider.GetLocalNow().DateTime;

            return (DateOnly.FromDateTime(local), TimeOnly.FromDateTime(local));
        }

        private SurpriseBagDto ToPublicDto(SurpriseBag bag, SurpriseBagUsage usage)
        {
            var dto = _mapper.Map<SurpriseBagDto>(bag);

            dto.RemainingQuantity = usage.RemainingOf(bag.QuantityTotal);
            dto.IsSoldOut = dto.RemainingQuantity == 0;

            return dto;
        }

        private OwnerSurpriseBagDto ToOwnerDto(SurpriseBag bag, SurpriseBagUsage usage)
        {
            var dto = _mapper.Map<OwnerSurpriseBagDto>(bag);

            dto.RemainingQuantity = usage.RemainingOf(bag.QuantityTotal);
            dto.IsSoldOut = dto.RemainingQuantity == 0;
            dto.ReservedQuantity = usage.ReservedQuantity;
            dto.CollectedQuantity = usage.CollectedQuantity;
            dto.IsPickupLocked = usage.UsedQuantity > 0;

            return dto;
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
