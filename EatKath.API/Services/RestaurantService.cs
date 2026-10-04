using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Constants;
using EatKath.API.Data;
using EatKath.API.DTOs.Restaurant;
using EatKath.API.DTOs.RestaurantOpeningHour;
using EatKath.API.Entities;
using EatKath.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly FileStorageService _fileStorage;
        private readonly ICurrentUserService _currentUser;

        public RestaurantService(
            ApplicationDbContext context,
            IMapper mapper,
            FileStorageService fileStorage,
            ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _currentUser = currentUser;
        }

        private void EnsureOwnership(Restaurant restaurant, string action)
        {
            if (!_currentUser.IsAdmin &&
                restaurant.OwnerId != _currentUser.UserId)
            {
                throw new BusinessRuleException($"You are not authorized to {action} this restaurant.");
            }
        }

        public async Task<IEnumerable<RestaurantDto>> GetAllAsync()
        {
            var restaurants = await _context.Restaurants
                .Include(r => r.Area)
.Include(r => r.Deals)
.Include(r => r.RestaurantCuisines)
    .ThenInclude(rc => rc.Cuisine)
.Include(r => r.RestaurantDiningTypes)
    .ThenInclude(rd => rd.DiningType)
                .ToListAsync();

            return restaurants.Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Address = r.Address,
                PhoneNumber = r.PhoneNumber,
                Email = r.Email,
                Website = r.Website,
                LogoUrl = r.LogoUrl,
                CurrencyCode = r.CurrencyCode,
                CoverImageUrl = r.CoverImageUrl,
                MenuPdfUrl = r.MenuPdfUrl,
                IsActive = r.IsActive,
                AreaId = r.AreaId,
                AreaName = r.Area.Name,

                ActiveDeals = r.Deals.Count(d => d.IsActive),

                Cuisines = r.RestaurantCuisines
                .Select(x => x.Cuisine.Name)
                .ToList(),

                DiningTypes = r.RestaurantDiningTypes
                .Select(x => x.DiningType.Name)
                .ToList(),

                BestDiscount = r.Deals
                    .Where(d => d.IsActive)
                    .Select(d => (decimal?)d.DiscountPercentage)
                    .DefaultIfEmpty()
                    .Max()
            });
        }

        public async Task<RestaurantDto?> GetByIdAsync(int id)
        {
            var restaurant = await _context.Restaurants
                            .Include(r => r.Area)
                             .Include(r => r.Deals)
            .Include(r => r.RestaurantCuisines)
                .ThenInclude(rc => rc.Cuisine)
            .Include(r => r.RestaurantDiningTypes)
                .ThenInclude(rd => rd.DiningType)
            .Include(r => r.OpeningHours)
                            .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return null;

            return new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                Email = restaurant.Email,
                Website = restaurant.Website,
                LogoUrl = restaurant.LogoUrl,
                CurrencyCode = restaurant.CurrencyCode,
                CoverImageUrl = restaurant.CoverImageUrl,
                MenuPdfUrl = restaurant.MenuPdfUrl,
                IsActive = restaurant.IsActive,
                AreaId = restaurant.AreaId,
                AreaName = restaurant.Area.Name,

                ActiveDeals = restaurant.Deals.Count(d => d.IsActive),

                BestDiscount = restaurant.Deals
        .Where(d => d.IsActive)
        .Select(d => (decimal?)d.DiscountPercentage)
        .DefaultIfEmpty()
        .Max(),

                Cuisines = restaurant.RestaurantCuisines
        .Select(x => x.Cuisine.Name)
        .ToList(),

                DiningTypes = restaurant.RestaurantDiningTypes
        .Select(x => x.DiningType.Name)
        .ToList(),

                // Scoped to this exact restaurant only (loaded via
                // Include against the single entity matched above) -
                // no other restaurant's data can leak through here.
                OpeningHours = restaurant.OpeningHours
                    .OrderBy(h => h.DayOfWeek)
                    .Select(h => new RestaurantOpeningHourDto
                    {
                        Id = h.Id,
                        RestaurantId = h.RestaurantId,
                        DayOfWeek = h.DayOfWeek,
                        OpenTime = h.OpenTime,
                        CloseTime = h.CloseTime,
                        IsClosed = h.IsClosed
                    })
                    .ToList()
            };
        }

        public async Task<IEnumerable<RestaurantDto>> GetByOwnerIdAsync(int ownerId)
        {
            var restaurants = await _context.Restaurants
                .Include(r => r.Area)
                .Include(r => r.Deals)
                .Include(r => r.RestaurantCuisines)
                    .ThenInclude(rc => rc.Cuisine)
                .Include(r => r.RestaurantDiningTypes)
                    .ThenInclude(rd => rd.DiningType)
                .Where(r => r.OwnerId == ownerId)
                .ToListAsync();

            return restaurants.Select(restaurant => new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                Email = restaurant.Email,
                Website = restaurant.Website,
                LogoUrl = restaurant.LogoUrl,
                CurrencyCode = restaurant.CurrencyCode,
                CoverImageUrl = restaurant.CoverImageUrl,
                MenuPdfUrl = restaurant.MenuPdfUrl,
                IsActive = restaurant.IsActive,
                AreaId = restaurant.AreaId,
                AreaName = restaurant.Area.Name,

                ActiveDeals = restaurant.Deals.Count(d => d.IsActive),

                Cuisines = restaurant.RestaurantCuisines
                    .Select(x => x.Cuisine.Name)
                    .ToList(),

                DiningTypes = restaurant.RestaurantDiningTypes
                    .Select(x => x.DiningType.Name)
                    .ToList(),

                BestDiscount = restaurant.Deals
                    .Where(d => d.IsActive)
                    .Select(d => (decimal?)d.DiscountPercentage)
                    .DefaultIfEmpty()
                    .Max()
            });
        }



        public async Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto)
        {
            if (!SupportedCurrencies.Codes.Contains(dto.CurrencyCode))
            {
                throw new BusinessRuleException($"'{dto.CurrencyCode}' is not a supported currency.");
            }

            var restaurant = _mapper.Map<Restaurant>(dto);

            if (!_currentUser.IsAdmin)
            {
                restaurant.OwnerId = _currentUser.UserId;
            }

            _context.Restaurants.Add(restaurant);

            // Every new restaurant gets a default Monday-Sunday opening
            // hours schedule, so the Owner Opening Hours page always has
            // rows to display/edit instead of an empty table. Linked via
            // the Restaurant navigation property (not RestaurantId
            // directly) so EF Core resolves the new restaurant's
            // identity value as part of this same SaveChangesAsync()
            // call - both inserts commit together.
            _context.RestaurantOpeningHours.AddRange(
                BuildDefaultOpeningHours(restaurant));

            await _context.SaveChangesAsync();

            await _context.Entry(restaurant)
                .Reference(r => r.Area)
                .LoadAsync();

            return _mapper.Map<RestaurantDto>(restaurant);
        }

        // -----------------------------
        // Mirrors the generic (non-bakery/non-cafe) default schedule
        // from RestaurantOpeningHourSeeder.cs: Mon-Thu 10:00-21:00,
        // Fri-Sat 10:00-22:00, Sun 11:00-20:00, never closed. The
        // seeder's bakery/cafe name-based overrides are demo-data
        // flourishes, not a convention a brand-new restaurant's name
        // can reliably be matched against, so only the generic default
        // is reused here.
        // -----------------------------
        private static List<RestaurantOpeningHour> BuildDefaultOpeningHours(Restaurant restaurant)
        {
            var openingHours = new List<RestaurantOpeningHour>();

            for (int day = 0; day < 7; day++)
            {
                var dayOfWeek = (DayOfWeek)day;

                var openingHour = new RestaurantOpeningHour
                {
                    Restaurant = restaurant,
                    DayOfWeek = dayOfWeek,
                    IsClosed = false
                };

                switch (dayOfWeek)
                {
                    case DayOfWeek.Friday:
                    case DayOfWeek.Saturday:
                        openingHour.OpenTime = new TimeOnly(10, 0);
                        openingHour.CloseTime = new TimeOnly(22, 0);
                        break;

                    case DayOfWeek.Sunday:
                        openingHour.OpenTime = new TimeOnly(11, 0);
                        openingHour.CloseTime = new TimeOnly(20, 0);
                        break;

                    default:
                        openingHour.OpenTime = new TimeOnly(10, 0);
                        openingHour.CloseTime = new TimeOnly(21, 0);
                        break;
                }

                openingHours.Add(openingHour);
            }

            return openingHours;
        }

        public async Task<RestaurantDto?> UpdateAsync(int id, UpdateRestaurantDto dto)
        {
            var restaurant = await _context.Restaurants
            .Include(r => r.Area)
            .Include(r => r.Deals)
            .Include(r => r.RestaurantCuisines)
                .ThenInclude(rc => rc.Cuisine)
            .Include(r => r.RestaurantDiningTypes)
                .ThenInclude(rd => rd.DiningType)
            .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return null;

            EnsureOwnership(restaurant, "update");

            if (!SupportedCurrencies.Codes.Contains(dto.CurrencyCode))
            {
                throw new BusinessRuleException($"'{dto.CurrencyCode}' is not a supported currency.");
            }

            // Changing currency after real money has been recorded
            // against this restaurant would silently relabel those
            // historical amounts under a different currency. A
            // completed Redemption (BillAmount set) is the only place
            // a real monetary amount is ever recorded, so that is the
            // narrowest accurate signal that "transactions exist" -
            // Pending/Cancelled redemptions and reservations carry no
            // monetary amount and are not blocked.
            if (!string.Equals(restaurant.CurrencyCode, dto.CurrencyCode, StringComparison.OrdinalIgnoreCase))
            {
                var hasMonetaryTransactions = await _context.Redemptions
                    .AnyAsync(r => r.Deal.RestaurantId == id && r.BillAmount != null);

                if (hasMonetaryTransactions)
                {
                    throw new BusinessRuleException(
                        "Cannot change currency because this restaurant already has completed transactions with recorded monetary amounts.");
                }
            }

            _mapper.Map(dto, restaurant);

            await _context.SaveChangesAsync();

            await _context.Entry(restaurant)
                .Reference(r => r.Area)
                .LoadAsync();

            return new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Description = restaurant.Description,
                Address = restaurant.Address,
                PhoneNumber = restaurant.PhoneNumber,
                Email = restaurant.Email,
                Website = restaurant.Website,
                LogoUrl = restaurant.LogoUrl,
                CurrencyCode = restaurant.CurrencyCode,
                CoverImageUrl = restaurant.CoverImageUrl,
                MenuPdfUrl = restaurant.MenuPdfUrl,
                IsActive = restaurant.IsActive,
                AreaId = restaurant.AreaId,
                AreaName = restaurant.Area.Name,

                ActiveDeals = restaurant.Deals.Count(d => d.IsActive),

                BestDiscount = restaurant.Deals
                .Where(d => d.IsActive)
                .Select(d => (decimal?)d.DiscountPercentage)
                .DefaultIfEmpty()
                .Max(),

                        Cuisines = restaurant.RestaurantCuisines
                .Select(x => x.Cuisine.Name)
                .ToList(),

                        DiningTypes = restaurant.RestaurantDiningTypes
                .Select(x => x.DiningType.Name)
                .ToList()
                    };
                }

        public async Task<bool> DeleteAsync(int id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null)
                return false;

            EnsureOwnership(restaurant, "delete");

            var hasDependentRecords =
                await _context.MenuItems.AnyAsync(m => m.RestaurantId == id) ||
                await _context.Redemptions.AnyAsync(r => r.Deal.RestaurantId == id);

            if (hasDependentRecords)
            {
                throw new BusinessRuleException(
                    "Cannot delete this restaurant because it has existing menu items or redeemed deals.");
            }

            _context.Restaurants.Remove(restaurant);

            await _context.SaveChangesAsync();

            return true;
        }

        // ============================
        // Upload Logo
        // ============================

        public async Task<string> UploadLogoAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureOwnership(restaurant, "modify");

            var path = await _fileStorage.SaveImageAsync(
                file,
                $"uploads/restaurants/{restaurantId}",
                "logo");

            restaurant.LogoUrl = path;

            await _context.SaveChangesAsync();

            return path;
        }

        // ============================
        // Upload Cover
        // ============================

        public async Task<string> UploadCoverAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureOwnership(restaurant, "modify");

            var path = await _fileStorage.SaveImageAsync(
                file,
                $"uploads/restaurants/{restaurantId}",
                "cover");

            restaurant.CoverImageUrl = path;

            await _context.SaveChangesAsync();

            return path;
        }

        // ============================
        // Upload Menu PDF
        // ============================

        public async Task<string> UploadMenuPdfAsync(int restaurantId, IFormFile file)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureOwnership(restaurant, "modify");

            var path = await _fileStorage.SavePdfAsync(
                file,
                $"uploads/restaurants/{restaurantId}",
                "menu");

            restaurant.MenuPdfUrl = path;

            await _context.SaveChangesAsync();

            return path;
        }


        public async Task DeleteLogoAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureOwnership(restaurant, "modify");

            await _fileStorage.DeleteFileAsync(restaurant.LogoUrl);

            restaurant.LogoUrl = string.Empty;

            await _context.SaveChangesAsync();
        }



        public async Task DeleteCoverAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureOwnership(restaurant, "modify");

            await _fileStorage.DeleteFileAsync(restaurant.CoverImageUrl);

            restaurant.CoverImageUrl = string.Empty;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteMenuPdfAsync(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found.");

            EnsureOwnership(restaurant, "modify");

            await _fileStorage.DeleteFileAsync(restaurant.MenuPdfUrl);

            restaurant.MenuPdfUrl = string.Empty;

            await _context.SaveChangesAsync();
        }

    }
}