using EatKath.API.Data;
using EatKath.API.Data.Demo;
using EatKath.API.Data.Seeders;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Data;

[TestClass]
public class DemoSeedGuardTests
{
    private sealed class FakeEnvironment : IHostEnvironment
    {
        public FakeEnvironment(string name) => EnvironmentName = name;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "EatKath.API";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private const string LocalDb = "Server=(localdb)\\MSSQLLocalDB;Database=EatKathDB;Trusted_Connection=True;";
    private const string AzureSql = "Server=tcp:example.database.windows.net,1433;Database=EatKathDB;User ID=app;Password=x;";

    [TestMethod]
    public void EnsureAllowed_ShouldRefuse_InProduction()
    {
        var act = () => DemoSeedGuard.EnsureAllowed(new FakeEnvironment("Production"), LocalDb);

        act.Should().Throw<InvalidOperationException>().WithMessage("*only allowed in the Development environment*Production*");
    }

    [TestMethod]
    public void EnsureAllowed_ShouldRefuse_InStaging()
    {
        var act = () => DemoSeedGuard.EnsureAllowed(new FakeEnvironment("Staging"), LocalDb);

        act.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void EnsureAllowed_ShouldRefuse_InDevelopment_WhenDatabaseIsNotLocalDb()
    {
        var act = () => DemoSeedGuard.EnsureAllowed(new FakeEnvironment("Development"), AzureSql);

        act.Should().Throw<InvalidOperationException>().WithMessage("*LocalDB*");
    }

    [TestMethod]
    public void EnsureAllowed_ShouldRefuse_WhenConnectionStringIsMissing()
    {
        var act = () => DemoSeedGuard.EnsureAllowed(new FakeEnvironment("Development"), null);

        act.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void EnsureAllowed_ShouldAllow_DevelopmentWithLocalDb()
    {
        var act = () => DemoSeedGuard.EnsureAllowed(new FakeEnvironment("Development"), LocalDb);

        act.Should().NotThrow();
    }

    [DataTestMethod]
    [DataRow("Server=(localdb)\\MSSQLLocalDB;Database=x", true)]
    [DataRow("Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=x", true)]
    [DataRow("Server=localhost;Database=x", false)]
    [DataRow("Server=tcp:prod.database.windows.net;Database=(localdb)\\x", false)]
    [DataRow("", false)]
    public void IsLocalDb_ShouldOnlyAcceptLocalDbServers(string connectionString, bool expected)
    {
        DemoSeedGuard.IsLocalDb(connectionString).Should().Be(expected);
    }

    [TestMethod]
    public void RequirePassword_ShouldRefuse_WhenNotConfigured()
    {
        var act = () => DemoSeedGuard.RequirePassword(null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*DemoSeed:OwnerPassword*");
    }
}

[TestClass]
public class CuisineSeederTests
{
    [TestMethod]
    public async Task SeedAsync_ShouldCreateFullCatalogue_OnEmptyDatabase()
    {
        using var context = TestDbContextFactory.Create();

        await CuisineSeeder.SeedAsync(context);

        context.Cuisines.Select(c => c.Name).Should().BeEquivalentTo(CuisineSeeder.Catalogue);
        CuisineSeeder.Catalogue.Should().HaveCount(34).And.OnlyHaveUniqueItems();
    }

    [TestMethod]
    public async Task SeedAsync_ShouldOnlyAddMissingCuisines_AndKeepExistingOnes()
    {
        using var context = TestDbContextFactory.Create();

        context.Cuisines.AddRange(CuisineSeeder.Catalogue.Take(20).Select(n => new Cuisine { Name = n }));
        context.Cuisines.Add(new Cuisine { Name = "Fusion" }); // added by an admin
        await context.SaveChangesAsync();
        var existingIds = context.Cuisines.ToDictionary(c => c.Name, c => c.Id);

        await CuisineSeeder.SeedAsync(context);

        context.Cuisines.Should().HaveCount(35);
        foreach (var (name, id) in existingIds)
            context.Cuisines.Single(c => c.Name == name).Id.Should().Be(id);
        context.Cuisines.Select(c => c.Name).Should().Contain(new[] { "Newari", "Thakali", "Momo & Dumplings", "Burgers", "Fusion" });
    }

    [TestMethod]
    public async Task SeedAsync_ShouldNotDuplicate_WhenRunTwiceOrNamesDifferInCase()
    {
        using var context = TestDbContextFactory.Create();

        context.Cuisines.Add(new Cuisine { Name = "pizza" });
        await context.SaveChangesAsync();

        await CuisineSeeder.SeedAsync(context);
        await CuisineSeeder.SeedAsync(context);

        context.Cuisines.Should().HaveCount(34);
        context.Cuisines.Count(c => c.Name.ToLower() == "pizza").Should().Be(1);
    }
}

[TestClass]
public class DemoDataSeederTests
{
    private const string Password = "Test-Demo-Password-1";
    private static readonly DateOnly Today = new(2026, 10, 5);

    private ApplicationDbContext _context = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _context = TestDbContextFactory.Create();

        _context.Roles.AddRange(new Role { Name = "Admin" }, new Role { Name = "Owner" }, new Role { Name = "Customer" });
        _context.Areas.AddRange(DemoCatalog.Restaurants.Select(r => r.Area).Distinct().Select(n => new Area { Name = n }));
        await _context.SaveChangesAsync();
        await CuisineSeeder.SeedAsync(_context);
    }

    [TestCleanup]
    public void Cleanup() => _context.Dispose();

    // Three images for every category the catalogue uses.
    private static IReadOnlyDictionary<string, IReadOnlyList<string>> Images() =>
        DemoCatalog.Restaurants
            .SelectMany(r => r.GalleryCategories.Append(r.CoverCategory))
            .Distinct()
            .ToDictionary(c => c, c => (IReadOnlyList<string>)Enumerable.Range(1, 3).Select(n => $"/uploads/demo/{c}-{n:D2}.jpg").ToList());

    private DemoDataSeeder Seeder() => new(_context, new PasswordHasher<User>());

    private async Task<Restaurant> SeedExistingDataAsync()
    {
        var ownerRole = _context.Roles.Single(r => r.Name == "Owner");
        var owner = new User { FirstName = "Real", LastName = "Owner", Email = "owner@eatkath.com", RoleId = ownerRole.Id, PasswordHash = "existing-hash" };
        var restaurant = new Restaurant
        {
            Owner = owner,
            Name = "Existing Restaurant",
            AreaId = _context.Areas.First().Id,
            CoverImageUrl = "/uploads/restaurants/1/cover.jpg",
            CurrencyCode = "NPR"
        };
        restaurant.Deals.Add(new Deal { Title = "Existing deal", DiscountPercentage = 20, OfferType = OfferType.DineIn, StartDate = Today, EndDate = Today.AddDays(5), StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(14, 0), MaximumGuests = 4 });
        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();
        return restaurant;
    }

    [TestMethod]
    public async Task SeedAsync_ShouldCreate48DemoRestaurants_EachWithItsOwnDemoOwner()
    {
        var result = await Seeder().SeedAsync(Password, Images(), Today);

        result.RestaurantsCreated.Should().Be(48);
        result.OwnersCreated.Should().Be(48);

        var demo = _context.Restaurants.Include(r => r.Owner).Where(r => r.IsDemo).ToList();
        demo.Should().HaveCount(48);
        demo.Select(r => r.Owner.Email).Should().BeEquivalentTo(Enumerable.Range(1, 48).Select(i => $"cdowner{i}@cd.com"));
        demo.GroupBy(r => r.OwnerId).Should().OnlyContain(g => g.Count() == 1);
        demo.Select(r => r.Owner.Role).Should().OnlyContain(role => role == null || role.Name == "Owner");
        _context.Users.Where(u => u.Email.StartsWith("cdowner")).Should().OnlyContain(u => _context.Roles.Single(r => r.Name == "Owner").Id == u.RoleId);
    }

    [TestMethod]
    public async Task SeedAsync_ShouldCoverEveryAreaTwice_AndEveryCuisineOnce()
    {
        await Seeder().SeedAsync(Password, Images(), Today);

        var perArea = _context.Restaurants.Where(r => r.IsDemo).ToList().GroupBy(r => r.AreaId).ToDictionary(g => g.Key, g => g.Count());
        _context.Areas.ToList().Should().OnlyContain(a => perArea.ContainsKey(a.Id) && perArea[a.Id] >= 2);

        var usedCuisines = _context.RestaurantCuisines.Select(rc => rc.CuisineId).Distinct().ToHashSet();
        _context.Cuisines.ToList().Should().OnlyContain(c => usedCuisines.Contains(c.Id), "every catalogue cuisine needs a demo restaurant");

        _context.Restaurants.Include(r => r.RestaurantCuisines).Where(r => r.IsDemo).ToList()
            .Should().OnlyContain(r => r.RestaurantCuisines.Count >= 1 && r.RestaurantCuisines.Count <= 3);
    }

    [TestMethod]
    public async Task SeedAsync_ShouldGiveEachRestaurantHoursMenuOffersAndPhotos()
    {
        await Seeder().SeedAsync(Password, Images(), Today);

        var demo = _context.Restaurants
            .Include(r => r.OpeningHours)
            .Include(r => r.MenuCategories).ThenInclude(c => c.MenuItems)
            .Include(r => r.Deals)
            .Include(r => r.Images)
            .Where(r => r.IsDemo)
            .ToList();

        foreach (var r in demo)
        {
            r.CurrencyCode.Should().Be("NPR");
            r.IsActive.Should().BeTrue();
            r.PhoneNumber.Should().BeEmpty();
            r.Email.Should().BeEmpty();
            r.Website.Should().BeEmpty();

            r.OpeningHours.Should().HaveCount(7).And.OnlyContain(h => !h.IsClosed && h.CloseTime > h.OpenTime);
            r.MenuCategories.Count.Should().BeInRange(3, 5);
            r.MenuCategories.SelectMany(c => c.MenuItems).Should().OnlyContain(i => i.Price >= 50 && i.Price <= 1500 && i.IsAvailable && i.RestaurantId == r.Id);

            r.Deals.Should().Contain(d => d.OfferType == OfferType.DineIn && d.IsActive);
            r.Deals.Should().OnlyContain(d =>
                d.IsActive && d.OfferType != OfferType.Delivery &&
                d.DiscountPercentage >= 10 && d.DiscountPercentage <= 40 &&
                d.StartDate <= Today && d.EndDate >= Today.AddDays(60) &&
                d.EndTime > d.StartTime && d.MaximumGuests >= 1);

            // Offer windows fit inside the restaurant's opening hours.
            var shortest = r.OpeningHours.Min(h => h.CloseTime);
            var open = r.OpeningHours.Max(h => h.OpenTime);
            r.Deals.Should().OnlyContain(d => d.StartTime >= open && d.EndTime <= shortest);

            r.CoverImageUrl.Should().StartWith("/uploads/demo/");
            r.Images.Should().HaveCount(3);
            r.Images.Select(i => i.ImageUrl).Should().OnlyHaveUniqueItems().And.NotContain(r.CoverImageUrl);
            r.Images.Should().OnlyContain(i => i.Caption == "Illustrative photo");
        }

        var withTakeaway = demo.Count(r => r.Deals.Any(d => d.OfferType == OfferType.Takeaway));
        ((double)withTakeaway / demo.Count).Should().BeInRange(0.6, 0.8);
    }

    [TestMethod]
    public async Task SeedAsync_ShouldHashTheDemoPassword_WithTheIdentityHasher()
    {
        await Seeder().SeedAsync(Password, Images(), Today);

        var hasher = new PasswordHasher<User>();
        var owner = _context.Users.Single(u => u.Email == "cdowner7@cd.com");

        owner.PasswordHash.Should().NotBe(Password);
        hasher.VerifyHashedPassword(owner, owner.PasswordHash, Password).Should().NotBe(PasswordVerificationResult.Failed);
        hasher.VerifyHashedPassword(owner, owner.PasswordHash, "wrong").Should().Be(PasswordVerificationResult.Failed);
    }

    [TestMethod]
    public async Task SeedAsync_ShouldBeIdempotent_WhenRunTwice()
    {
        await Seeder().SeedAsync(Password, Images(), Today);
        var counts = (Users: _context.Users.Count(), Restaurants: _context.Restaurants.Count(), Deals: _context.Deals.Count(),
            Items: _context.MenuItems.Count(), Images: _context.RestaurantImages.Count(), Links: _context.RestaurantCuisines.Count(),
            Hours: _context.RestaurantOpeningHours.Count());

        var second = await Seeder().SeedAsync(Password, Images(), Today);

        second.RestaurantsCreated.Should().Be(0);
        second.OwnersCreated.Should().Be(0);
        second.RestaurantsAlreadyPresent.Should().Be(48);
        (_context.Users.Count(), _context.Restaurants.Count(), _context.Deals.Count(), _context.MenuItems.Count(),
            _context.RestaurantImages.Count(), _context.RestaurantCuisines.Count(), _context.RestaurantOpeningHours.Count())
            .Should().Be(counts);
    }

    [TestMethod]
    public async Task SeedAsync_ShouldLeaveExistingRestaurantsUsersAndDealsUnchanged()
    {
        var existing = await SeedExistingDataAsync();

        await Seeder().SeedAsync(Password, Images(), Today);

        _context.ChangeTracker.Clear();
        var reloaded = _context.Restaurants.Include(r => r.Owner).Include(r => r.Deals).Single(r => r.Id == existing.Id);
        reloaded.IsDemo.Should().BeFalse();
        reloaded.Name.Should().Be("Existing Restaurant");
        reloaded.CoverImageUrl.Should().Be("/uploads/restaurants/1/cover.jpg");
        reloaded.Owner.Email.Should().Be("owner@eatkath.com");
        reloaded.Owner.PasswordHash.Should().Be("existing-hash");
        reloaded.Deals.Should().ContainSingle(d => d.Title == "Existing deal" && d.DiscountPercentage == 20);
    }

    [TestMethod]
    public async Task SeedAsync_ShouldRefuseToTouch_AnExistingNonOwnerAccountWithADemoEmail()
    {
        var customerRole = _context.Roles.Single(r => r.Name == "Customer");
        _context.Users.Add(new User { FirstName = "Someone", LastName = "Else", Email = "cdowner1@cd.com", RoleId = customerRole.Id, PasswordHash = "theirs" });
        await _context.SaveChangesAsync();

        var act = () => Seeder().SeedAsync(Password, Images(), Today);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cdowner1@cd.com*not an Owner*");
        _context.Restaurants.Should().BeEmpty();
        _context.Users.Single(u => u.Email == "cdowner1@cd.com").PasswordHash.Should().Be("theirs");
    }

    [TestMethod]
    public async Task SeedAsync_ShouldWriteNothing_WhenImagesAreMissing()
    {
        var images = Images().Where(kv => kv.Key != "momo").ToDictionary(kv => kv.Key, kv => kv.Value);

        var act = () => Seeder().SeedAsync(Password, images, Today);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No demo images for category 'momo'*");
        _context.Restaurants.Should().BeEmpty();
        _context.Users.Should().BeEmpty();
    }

    [TestMethod]
    public async Task SeedAsync_ShouldWriteNothing_WhenACatalogueCuisineIsMissing()
    {
        _context.Cuisines.Remove(_context.Cuisines.Single(c => c.Name == "Newari"));
        await _context.SaveChangesAsync();

        var act = () => Seeder().SeedAsync(Password, Images(), Today);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Cuisine 'Newari' does not exist*");
        _context.Restaurants.Should().BeEmpty();
    }

    [TestMethod]
    public void Catalogue_ShouldUseRealisticNprMenus_With3To5Categories()
    {
        DemoCatalog.Kitchens.Values.Should().OnlyContain(k => k.Menu.Length >= 3 && k.Menu.Length <= 5);
        DemoCatalog.Kitchens.Values.SelectMany(k => k.Menu).SelectMany(c => c.Items)
            .Should().OnlyContain(i => i.Price >= 50 && i.Price <= 1500);
        DemoCatalog.Restaurants.Select(r => r.Name).Should().OnlyHaveUniqueItems();
    }
}
