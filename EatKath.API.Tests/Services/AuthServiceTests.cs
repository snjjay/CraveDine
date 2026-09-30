using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Auth;
using EatKath.API.Entities;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Services;

[TestClass]
public class AuthServiceTests
{
    private const string TestSecretKey =
        "UnitTest-Super-Secret-Key-For-HmacSha256-Signing-At-Least-32-Bytes-Long!!";

    private const string TestIssuer = "EatKath.Test.Issuer";
    private const string TestAudience = "EatKath.Test.Audience";
    private const string TestExpiryMinutes = "60";

    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private AuthService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _service = new AuthService(
            _context,
            BuildConfiguration(TestExpiryMinutes),
            _mapper);
    }

    // -----------------------------
    // AuthService reads JwtSettings via plain IConfiguration
    // (not the strongly-typed Options pattern), so a real
    // in-memory IConfiguration is enough - no mocking needed.
    // -----------------------------
    private static IConfiguration BuildConfiguration(string expiryMinutes)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = TestSecretKey,
                ["JwtSettings:Issuer"] = TestIssuer,
                ["JwtSettings:Audience"] = TestAudience,
                ["JwtSettings:ExpiryInMinutes"] = expiryMinutes
            })
            .Build();
    }

    // -----------------------------
    // Uses the real PasswordHasher<User> - the same type
    // AuthService itself uses - so VerifyHashedPassword behaves
    // exactly as it does in production, rather than depending
    // on a hand-written fake hash string.
    // -----------------------------
    private static string HashPassword(User user, string password) =>
        new PasswordHasher<User>().HashPassword(user, password);


    // -----------------------------
    // RegisterAsync
    // -----------------------------

    [TestMethod]
    public async Task RegisterAsync_ShouldThrowBusinessRuleException_WhenEmailAlreadyExists()
    {
        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@test.com",
            PasswordHash = "irrelevant-hash",
            PhoneNumber = "0400000000",
            RoleId = 1,
            IsActive = true
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto
        {
            FirstName = "New",
            LastName = "Person",
            Email = "existing@test.com",
            Password = "Password123",
            PhoneNumber = "0400111222"
        };

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.RegisterAsync(dto));

        ex.Message.Should().Be("Email already exists.");
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken_WhenRequestIsValid()
    {
        var customerRole = new Role { Name = "Customer" };

        _context.Roles.Add(customerRole);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto
        {
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane.diner@test.com",
            Password = "Password123",
            PhoneNumber = "0400111222"
        };

        var result = await _service.RegisterAsync(dto);

        var savedUser = _context.Users.Single();

        savedUser.IsActive.Should().BeTrue();
        savedUser.RoleId.Should().Be(customerRole.Id);
        savedUser.PasswordHash.Should().NotBeNullOrEmpty();
        savedUser.PasswordHash.Should().NotBe(dto.Password);

        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(savedUser.Id);
        result.Email.Should().Be(dto.Email);
        result.Role.Should().Be("Customer");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldSucceed_WhenPhoneNumberIsNull()
    {
        var customerRole = new Role { Name = "Customer" };

        _context.Roles.Add(customerRole);
        await _context.SaveChangesAsync();

        var dto = new RegisterDto
        {
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane.nophone@test.com",
            Password = "Password123",
            PhoneNumber = null
        };

        var result = await _service.RegisterAsync(dto);

        var savedUser = _context.Users.Single();

        result.Should().NotBeNull();
        savedUser.PhoneNumber.Should().Be(string.Empty);
    }


    // -----------------------------
    // LoginAsync
    // -----------------------------

    [TestMethod]
    public async Task LoginAsync_ShouldThrowAuthenticationException_WhenEmailDoesNotExist()
    {
        var dto = new LoginDto
        {
            Email = "nobody@test.com",
            Password = "Whatever123"
        };

        var ex = await Assert.ThrowsExceptionAsync<AuthenticationException>(
            () => _service.LoginAsync(dto));

        ex.Message.Should().Be("Invalid email or password.");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrowAuthenticationException_WhenAccountIsInactive()
    {
        var role = new Role { Name = "Customer" };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            FirstName = "Inactive",
            LastName = "User",
            Email = "inactive@test.com",
            PhoneNumber = "0400111222",
            RoleId = role.Id,
            IsActive = false
        };

        user.PasswordHash = HashPassword(user, "Password123");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto
        {
            Email = "inactive@test.com",
            Password = "Password123"
        };

        var ex = await Assert.ThrowsExceptionAsync<AuthenticationException>(
            () => _service.LoginAsync(dto));

        ex.Message.Should().Be("User account is inactive.");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldThrowAuthenticationException_WhenPasswordIsWrong()
    {
        var role = new Role { Name = "Customer" };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            FirstName = "Active",
            LastName = "User",
            Email = "active@test.com",
            PhoneNumber = "0400111222",
            RoleId = role.Id,
            IsActive = true
        };

        user.PasswordHash = HashPassword(user, "CorrectPassword123");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto
        {
            Email = "active@test.com",
            Password = "WrongPassword123"
        };

        var ex = await Assert.ThrowsExceptionAsync<AuthenticationException>(
            () => _service.LoginAsync(dto));

        ex.Message.Should().Be("Invalid email or password.");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var role = new Role { Name = "Customer" };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane.login@test.com",
            PhoneNumber = "0400111222",
            RoleId = role.Id,
            IsActive = true
        };

        user.PasswordHash = HashPassword(user, "Password123");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto
        {
            Email = "jane.login@test.com",
            Password = "Password123"
        };

        var result = await _service.LoginAsync(dto);

        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be("Customer");
    }

    [TestMethod]
    public async Task LoginAsync_ShouldIncludeCorrectClaims_WhenCredentialsAreValid()
    {
        var role = new Role { Name = "Customer" };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane.claims@test.com",
            PhoneNumber = "0400111222",
            RoleId = role.Id,
            IsActive = true
        };

        user.PasswordHash = HashPassword(user, "Password123");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto
        {
            Email = "jane.claims@test.com",
            Password = "Password123"
        };

        var result = await _service.LoginAsync(dto);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
            .Should().Be(user.Id.ToString());

        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value
            .Should().Be(user.Email);

        jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value
            .Should().Be($"{user.FirstName} {user.LastName}");

        jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value
            .Should().Be("Customer");

        jwt.Issuer.Should().Be(TestIssuer);
        jwt.Audiences.Should().Contain(TestAudience);
    }

    [TestMethod]
    public async Task LoginAsync_ShouldSetExpiresAt_ConsistentWithConfiguredExpiry()
    {
        const int expiryMinutes = 45;

        _service = new AuthService(
            _context,
            BuildConfiguration(expiryMinutes.ToString()),
            _mapper);

        var role = new Role { Name = "Customer" };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var user = new User
        {
            FirstName = "Jane",
            LastName = "Diner",
            Email = "jane.expiry@test.com",
            PhoneNumber = "0400111222",
            RoleId = role.Id,
            IsActive = true
        };

        user.PasswordHash = HashPassword(user, "Password123");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var dto = new LoginDto
        {
            Email = "jane.expiry@test.com",
            Password = "Password123"
        };

        var beforeLogin = DateTime.UtcNow;

        var result = await _service.LoginAsync(dto);

        var expectedExpiry = beforeLogin.AddMinutes(expiryMinutes);

        result.ExpiresAt.Should()
            .BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }
}
