using EatKath.API.Entities;
using EatKath.API.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Demo
{
    public sealed record DemoSeedEntry(int Index, string OwnerEmail, string RestaurantName, string Area, bool Created);

    public sealed class DemoSeedResult
    {
        public int OwnersCreated { get; set; }
        public int RestaurantsCreated { get; set; }
        public int RestaurantsAlreadyPresent { get; set; }
        public int CuisineLinks { get; set; }
        public int OpeningHours { get; set; }
        public int MenuCategories { get; set; }
        public int MenuItems { get; set; }
        public int Deals { get; set; }
        public int Images { get; set; }
        public List<DemoSeedEntry> Entries { get; } = new();
    }

    // Creates the development demo dataset (DemoCatalog): one demo owner
    // (cdownerN@cd.com) per demo restaurant, with cuisines, opening hours,
    // menu, dine-in/takeaway offers and photos.
    //
    // - Idempotent: owners are matched by email and a demo restaurant by its
    //   owner; anything already present is skipped, never updated.
    // - Never modifies or deletes existing data.
    // - Each restaurant is saved in one SaveChanges call (owner + restaurant
    //   + children together), so an interrupted run resumes cleanly.
    // - Only reachable through the dev-only `seed-demo` command
    //   (DemoSeedCommand), never through normal startup seeding.
    public sealed class DemoDataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public DemoDataSeeder(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // imagesByCategory: category -> image URLs ("/uploads/demo/momo-01.jpg"),
        // see DemoImageLibrary.
        public async Task<DemoSeedResult> SeedAsync(
            string ownerPassword,
            IReadOnlyDictionary<string, IReadOnlyList<string>> imagesByCategory,
            DateOnly today)
        {
            if (string.IsNullOrWhiteSpace(ownerPassword))
                throw new InvalidOperationException("A demo owner password is required.");

            var ownerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Owner")
                ?? throw new InvalidOperationException("The 'Owner' role does not exist.");

            var areas = await _context.Areas.ToListAsync();
            var cuisines = await _context.Cuisines.ToListAsync();

            Validate(areas, cuisines, imagesByCategory);

            var picker = new ImagePicker(imagesByCategory);
            var result = new DemoSeedResult();

            for (var i = 0; i < DemoCatalog.Restaurants.Length; i++)
            {
                var index = i + 1;
                var definition = DemoCatalog.Restaurants[i];
                var email = string.Format(DemoCatalog.OwnerEmailPattern, index);

                // Choose this restaurant's photos even when it already exists,
                // so later restaurants always get the same images.
                var cover = picker.Next(definition.CoverCategory);
                var gallery = definition.GalleryCategories
                    .Select(category => picker.Next(category, exclude: cover))
                    .ToList();

                var owner = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (owner != null)
                {
                    if (owner.RoleId != ownerRole.Id)
                    {
                        throw new InvalidOperationException(
                            $"User '{email}' already exists but is not an Owner; it will not be modified. Remove or rename it before seeding.");
                    }

                    var existing = await _context.Restaurants
                        .FirstOrDefaultAsync(r => r.IsDemo && r.OwnerId == owner.Id);

                    if (existing != null)
                    {
                        result.RestaurantsAlreadyPresent++;
                        result.Entries.Add(new DemoSeedEntry(index, email, existing.Name, definition.Area, false));
                        continue;
                    }
                }
                else
                {
                    owner = new User
                    {
                        FirstName = "Demo",
                        LastName = $"Owner {index}",
                        Email = email,
                        PhoneNumber = string.Empty,
                        RoleId = ownerRole.Id,
                        IsActive = true
                    };

                    owner.PasswordHash = _passwordHasher.HashPassword(owner, ownerPassword);

                    _context.Users.Add(owner);
                    result.OwnersCreated++;
                }

                var restaurant = BuildRestaurant(definition, index, owner, areas, cuisines, cover, gallery, today, result);

                _context.Restaurants.Add(restaurant);

                await _context.SaveChangesAsync();

                result.RestaurantsCreated++;
                result.Entries.Add(new DemoSeedEntry(index, email, restaurant.Name, definition.Area, true));
            }

            return result;
        }

        // Everything the dataset needs must exist before anything is written.
        private static void Validate(
            List<Area> areas,
            List<Cuisine> cuisines,
            IReadOnlyDictionary<string, IReadOnlyList<string>> imagesByCategory)
        {
            var areaNames = areas.Select(a => a.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var cuisineNames = cuisines.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var problems = new List<string>();

            foreach (var r in DemoCatalog.Restaurants)
            {
                if (!areaNames.Contains(r.Area))
                    problems.Add($"Area '{r.Area}' does not exist.");

                problems.AddRange(r.Cuisines
                    .Where(c => !cuisineNames.Contains(c))
                    .Select(c => $"Cuisine '{c}' does not exist."));

                if (!DemoCatalog.Kitchens.ContainsKey(r.Kitchen))
                    problems.Add($"Kitchen '{r.Kitchen}' is not defined.");

                foreach (var category in r.GalleryCategories.Append(r.CoverCategory))
                {
                    if (!imagesByCategory.TryGetValue(category, out var images) || images.Count == 0)
                        problems.Add($"No demo images for category '{category}'.");
                }
            }

            if (problems.Count > 0)
            {
                throw new InvalidOperationException(
                    "Demo seed data is incomplete: " + string.Join(" ", problems.Distinct()));
            }
        }

        private static Restaurant BuildRestaurant(
            DemoRestaurant definition,
            int index,
            User owner,
            List<Area> areas,
            List<Cuisine> cuisines,
            string cover,
            List<string> gallery,
            DateOnly today,
            DemoSeedResult result)
        {
            var kitchen = DemoCatalog.Kitchens[definition.Kitchen];

            var restaurant = new Restaurant
            {
                Owner = owner,
                Name = definition.Name,
                Description = definition.Description,
                Address = definition.Address,
                AreaId = areas.First(a => a.Name.Equals(definition.Area, StringComparison.OrdinalIgnoreCase)).Id,
                // Synthetic listing: no phone, email or website.
                PhoneNumber = string.Empty,
                Email = string.Empty,
                Website = string.Empty,
                LogoUrl = string.Empty,
                CoverImageUrl = cover,
                CurrencyCode = "NPR",
                IsActive = true,
                IsDemo = true
            };

            foreach (var name in definition.Cuisines)
            {
                restaurant.RestaurantCuisines.Add(new RestaurantCuisine
                {
                    Restaurant = restaurant,
                    Cuisine = cuisines.First(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                });
                result.CuisineLinks++;
            }

            for (var day = 0; day < 7; day++)
            {
                var dayOfWeek = (DayOfWeek)day;
                var weekend = dayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;

                restaurant.OpeningHours.Add(new RestaurantOpeningHour
                {
                    Restaurant = restaurant,
                    DayOfWeek = dayOfWeek,
                    OpenTime = kitchen.Opens,
                    CloseTime = weekend ? kitchen.WeekendCloses : kitchen.Closes,
                    IsClosed = false
                });
                result.OpeningHours++;
            }

            var order = 1;
            foreach (var category in kitchen.Menu)
            {
                var menuCategory = new MenuCategory
                {
                    Restaurant = restaurant,
                    Name = category.Name,
                    DisplayOrder = order++
                };

                foreach (var item in category.Items)
                {
                    menuCategory.MenuItems.Add(new MenuItem
                    {
                        Restaurant = restaurant,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        IsAvailable = true,
                        IsFeatured = false
                    });
                    result.MenuItems++;
                }

                restaurant.MenuCategories.Add(menuCategory);
                result.MenuCategories++;
            }

            foreach (var deal in BuildDeals(index, kitchen, cover, today))
            {
                deal.Restaurant = restaurant;
                restaurant.Deals.Add(deal);
                result.Deals++;
            }

            for (var g = 0; g < gallery.Count; g++)
            {
                restaurant.Images.Add(new RestaurantImage
                {
                    Restaurant = restaurant,
                    ImageUrl = gallery[g],
                    Caption = "Illustrative photo",
                    DisplayOrder = g + 1,
                    IsPrimary = false
                });
                result.Images++;
            }

            // The cover counts as an image too.
            result.Images++;

            return restaurant;
        }

        private static readonly int[] DineInDiscounts = { 25, 20, 30, 15, 35, 20, 25, 40, 10, 30 };
        private static readonly int[] TakeawayDiscounts = { 15, 20, 10, 25, 15, 20, 10 };
        private static readonly int[] MaxGuests = { 4, 6, 2, 8, 4, 6 };
        private static readonly int[] DailyLimits = { 0, 10, 20, 15, 0, 25 };
        private static readonly int[] TotalLimits = { 0, 0, 150, 0, 200, 0 };

        // Every restaurant: an active dine-in offer. About 70%: a takeaway
        // offer. Every 4th: a second dine-in offer at another time.
        // Valid from 3 days ago to 90 days ahead, inside opening hours.
        public static List<Deal> BuildDeals(int index, DemoKitchen kitchen, string promoImage, DateOnly today)
        {
            var deals = new List<Deal>();
            var start = today.AddDays(-3);
            var end = today.AddDays(90);
            var slot = index % 6;

            // Lunch/dinner windows stay inside the kitchen's weekday hours.
            var lunchStart = kitchen.Opens > new TimeOnly(11, 30) ? kitchen.Opens : new TimeOnly(11, 30);
            var dinnerEnd = kitchen.Closes < new TimeOnly(21, 0) ? kitchen.Closes : new TimeOnly(21, 0);
            (TimeOnly, TimeOnly, string) lunch = (lunchStart, new TimeOnly(15, 0), "lunch");
            (TimeOnly, TimeOnly, string) dinner = (new TimeOnly(18, 0), dinnerEnd, "dinner");

            (TimeOnly From, TimeOnly To, string Label) primary = kitchen.CafeStyle
                ? (new TimeOnly(14, 0), new TimeOnly(17, 30), "afternoon")
                : index % 2 == 0 ? lunch : dinner;

            deals.Add(NewDeal(OfferType.DineIn, DineInDiscounts[index % DineInDiscounts.Length], primary, promoImage, start, end, slot));

            if (index % 10 is not (2 or 5 or 8))
            {
                (TimeOnly, TimeOnly, string) takeaway = kitchen.CafeStyle
                    ? (new TimeOnly(8, 0), new TimeOnly(11, 0), "morning")
                    : (new TimeOnly(15, 0), new TimeOnly(19, 0), "afternoon");

                deals.Add(NewDeal(OfferType.Takeaway, TakeawayDiscounts[index % TakeawayDiscounts.Length], takeaway, promoImage, start, end, slot));
            }

            if (index % 4 == 0 && !kitchen.CafeStyle)
            {
                var second = primary.Label == "lunch" ? dinner : lunch;

                deals.Add(NewDeal(OfferType.DineIn, 15, second, promoImage, start, end, (slot + 1) % 6));
            }

            return deals;
        }

        private static Deal NewDeal(
            OfferType type,
            int discount,
            (TimeOnly From, TimeOnly To, string Label) window,
            string promoImage,
            DateOnly start,
            DateOnly end,
            int slot)
        {
            var kind = type == OfferType.DineIn ? "dine-in" : "takeaway";

            return new Deal
            {
                Title = $"{discount}% off {kind} {window.Label}",
                Description = $"Walk-in {kind} offer: {discount}% off your bill when you arrive between {window.From:HH\\:mm} and {window.To:HH\\:mm}.",
                DiscountPercentage = discount,
                OfferType = type,
                PromoImageUrl = promoImage,
                TermsAndConditions = "Demo offer for development and testing only.",
                StartDate = start,
                EndDate = end,
                StartTime = window.From,
                EndTime = window.To,
                MaximumGuests = MaxGuests[slot],
                DailyRedemptionLimit = DailyLimits[slot],
                ReservationLimit = TotalLimits[slot],
                IsActive = true
            };
        }

        // Hands out images per category in a fixed order (cover first), so
        // covers are unique while the category has unused images and the
        // same restaurant never repeats an image.
        private sealed class ImagePicker
        {
            private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _images;
            private readonly Dictionary<string, int> _next = new();

            public ImagePicker(IReadOnlyDictionary<string, IReadOnlyList<string>> images) => _images = images;

            public string Next(string category, string? exclude = null)
            {
                var list = _images[category];
                var position = _next.GetValueOrDefault(category);

                for (var attempt = 0; attempt < list.Count; attempt++)
                {
                    var candidate = list[(position + attempt) % list.Count];

                    if (candidate != exclude)
                    {
                        _next[category] = position + attempt + 1;
                        return candidate;
                    }
                }

                return list[position % list.Count];
            }
        }
    }
}
