using EatKath.API.Data;
using EatKath.API.DTOs.Owner;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class OwnerDashboardService : IOwnerDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public OwnerDashboardService(
            ApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<OwnerDashboardDto> GetDashboardAsync()
        {
            var ownerId = _currentUser.UserId;

            var restaurantIds = await _context.Restaurants
                .Where(r => r.OwnerId == ownerId)
                .Select(r => r.Id)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var activeDeals = await _context.Deals
                .CountAsync(d =>
                    restaurantIds.Contains(d.RestaurantId) &&
                    d.IsActive);

            var pendingRedemptions = await _context.Redemptions
                .CountAsync(r =>
                    restaurantIds.Contains(r.Deal.RestaurantId) &&
                    r.Status == RedemptionStatus.Redeemed);

            var completedToday = await _context.Redemptions
                .CountAsync(r =>
                    restaurantIds.Contains(r.Deal.RestaurantId) &&
                    r.Status == RedemptionStatus.Completed &&
                    r.CompletedAt.HasValue &&
                    DateOnly.FromDateTime(r.CompletedAt.Value) == today);

            var todayRevenue = await _context.Redemptions
                .Where(r =>
                    restaurantIds.Contains(r.Deal.RestaurantId) &&
                    r.Status == RedemptionStatus.Completed &&
                    r.CompletedAt.HasValue &&
                    DateOnly.FromDateTime(r.CompletedAt.Value) == today)
                .SumAsync(r => r.FinalAmount ?? 0);

            var customersServedToday = await _context.Redemptions
                .Where(r =>
                    restaurantIds.Contains(r.Deal.RestaurantId) &&
                    r.Status == RedemptionStatus.Completed &&
                    r.CompletedAt.HasValue &&
                    DateOnly.FromDateTime(r.CompletedAt.Value) == today)
                .Select(r => r.UserId)
                .Distinct()
                .CountAsync();

            return new OwnerDashboardDto
            {
                ActiveDeals = activeDeals,
                PendingRedemptions = pendingRedemptions,
                CompletedToday = completedToday,
                TodayRevenue = todayRevenue,
                CustomersServedToday = customersServedToday
            };
        }
    }
}