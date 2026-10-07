using EatKath.API.Data;
using EatKath.API.Data.Seeders;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Data;

// The normal startup seeders on a fresh database: no external
// placeholder images (picsum etc.), no broken gallery paths, and the
// fictional seed restaurants are listed as demo data.
[TestClass]
public class StartupSeederImageTests
{
    private ApplicationDbContext _context = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _context = TestDbContextFactory.Create();

        await RoleSeeder.SeedAsync(_context);
        await UserSeeder.SeedAsync(_context);
        await AreaSeeder.SeedAsync(_context);
        await RestaurantSeeder.SeedAsync(_context);
        await DealSeeder.SeedAsync(_context);
        await RestaurantImageSeeder.SeedAsync(_context);
    }

    [TestMethod]
    public void RestaurantSeeder_ShouldCreateDemoRestaurants_WithoutImages()
    {
        var restaurants = _context.Restaurants.ToList();

        restaurants.Should().HaveCount(50);
        restaurants.Should().OnlyContain(r => r.IsDemo);
        restaurants.Should().OnlyContain(r => r.LogoUrl == string.Empty);
        restaurants.Should().OnlyContain(r => r.CoverImageUrl == null);
    }

    [TestMethod]
    public void DealSeeder_ShouldNotSetPromoImages()
    {
        var deals = _context.Deals.ToList();

        deals.Should().HaveCount(100);
        deals.Should().OnlyContain(d => d.PromoImageUrl == string.Empty);
    }

    [TestMethod]
    public void RestaurantImageSeeder_ShouldNotCreateGalleryReferences()
    {
        _context.RestaurantImages.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Seeders_ShouldStayIdempotent_WhenRunAgain()
    {
        await RestaurantSeeder.SeedAsync(_context);
        await DealSeeder.SeedAsync(_context);
        await RestaurantImageSeeder.SeedAsync(_context);

        _context.Restaurants.Should().HaveCount(50);
        _context.Deals.Should().HaveCount(100);
        _context.RestaurantImages.Should().BeEmpty();
    }

    [TestMethod]
    public void Seeders_ShouldNotUseExternalPlaceholderImages()
    {
        var urls = _context.Restaurants.Select(r => r.LogoUrl)
            .Concat(_context.Restaurants.Select(r => r.CoverImageUrl ?? ""))
            .Concat(_context.Deals.Select(d => d.PromoImageUrl))
            .ToList();

        urls.Should().NotContain(u => u.Contains("picsum", StringComparison.OrdinalIgnoreCase));
        urls.Should().NotContain(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase));
    }
}
