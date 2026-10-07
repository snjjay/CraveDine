using EatKath.API.Data;
using EatKath.API.Data.Seeders;
using EatKath.API.DTOs.Auth;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Data;

// Startup seeding split: reference data in every environment, sample
// users/restaurants/deals only in Development. (DatabaseSeeder.SeedAsync
// itself also runs MigrateAsync, which the InMemory provider cannot, so
// the two groups are tested directly.)
[TestClass]
public class DatabaseSeederTests
{
    private sealed class FakeEnvironment : IHostEnvironment
    {
        public FakeEnvironment(string name) => EnvironmentName = name;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "EatKath.API";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private ApplicationDbContext _context = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();
    }

    [TestMethod]
    public async Task SeedReferenceDataAsync_ShouldSeedOnlyLookupData()
    {
        await DatabaseSeeder.SeedReferenceDataAsync(_context);

        _context.Roles.Select(r => r.Name).Should()
            .BeEquivalentTo(new[] { "Admin", "Owner", "Customer" });
        _context.Areas.Should().HaveCount(20);
        _context.DiningTypes.Should().HaveCount(10);
        _context.Cuisines.Should().HaveCount(CuisineSeeder.Catalogue.Length);

        _context.Users.Should().BeEmpty();
        _context.Restaurants.Should().BeEmpty();
        _context.Deals.Should().BeEmpty();
        _context.MenuCategories.Should().BeEmpty();
        _context.MenuItems.Should().BeEmpty();
        _context.RestaurantImages.Should().BeEmpty();
        _context.UserFavorites.Should().BeEmpty();
        _context.Redemptions.Should().BeEmpty();
    }

    [TestMethod]
    public async Task SeedReferenceDataAsync_ShouldBeIdempotent()
    {
        await DatabaseSeeder.SeedReferenceDataAsync(_context);
        await DatabaseSeeder.SeedReferenceDataAsync(_context);

        _context.Roles.Should().HaveCount(3);
        _context.Areas.Should().HaveCount(20);
        _context.DiningTypes.Should().HaveCount(10);
        _context.Cuisines.Should().HaveCount(CuisineSeeder.Catalogue.Length);
    }

    [TestMethod]
    public async Task SeedDevelopmentDataAsync_AfterReferenceData_ShouldSeedSampleData()
    {
        await DatabaseSeeder.SeedReferenceDataAsync(_context);
        await DatabaseSeeder.SeedDevelopmentDataAsync(_context);

        _context.Users.Should().HaveCount(10);
        _context.Restaurants.Should().HaveCount(50);
        _context.Deals.Should().HaveCount(100);
        _context.RestaurantOpeningHours.Should().NotBeEmpty();
        _context.RestaurantCuisines.Should().NotBeEmpty();
        _context.RestaurantDiningTypes.Should().NotBeEmpty();
        _context.MenuCategories.Should().NotBeEmpty();
        _context.MenuItems.Should().NotBeEmpty();
        _context.UserFavorites.Should().NotBeEmpty();
        _context.Redemptions.Should().NotBeEmpty();
    }

    [TestMethod]
    [DataRow("Development", true)]
    [DataRow("Production", false)]
    [DataRow("Staging", false)]
    [DataRow("", false)]
    public void ShouldSeedDevelopmentData_ShouldBeTrueOnlyInDevelopment(string environmentName, bool expected)
    {
        DatabaseSeeder.ShouldSeedDevelopmentData(new FakeEnvironment(environmentName))
            .Should().Be(expected);
    }

    // Production has reference data only: registration must still work
    // (it assigns the seeded "Customer" role).
    [TestMethod]
    public async Task RegisterAsync_ShouldCreateCustomer_WithReferenceDataOnly()
    {
        await DatabaseSeeder.SeedReferenceDataAsync(_context);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "UnitTest-Super-Secret-Key-For-HmacSha256-Signing-At-Least-32-Bytes-Long!!",
                ["JwtSettings:Issuer"] = "EatKath.Test.Issuer",
                ["JwtSettings:Audience"] = "EatKath.Test.Audience",
                ["JwtSettings:ExpiryInMinutes"] = "60"
            })
            .Build();

        var service = new AuthService(_context, configuration, MapperFactory.Create());

        var result = await service.RegisterAsync(new RegisterDto
        {
            FirstName = "Asha",
            LastName = "Shrestha",
            Email = "asha@example.com",
            Password = "Register-Test-1"
        });

        result.Role.Should().Be("Customer");
        _context.Users.Should().ContainSingle(u => u.Email == "asha@example.com");
    }
}
