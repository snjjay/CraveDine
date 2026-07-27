using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Area;
using EatKath.API.Entities;
using EatKath.API.Exceptions;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EatKath.API.Tests.Services;

[TestClass]
public class AreaServiceTests
{
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IValidator<CreateAreaDto>> _createValidator = null!;
    private Mock<IValidator<UpdateAreaDto>> _updateValidator = null!;
    private AreaService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _context = TestDbContextFactory.Create();

        _mapper = MapperFactory.Create();

        _createValidator = new Mock<IValidator<CreateAreaDto>>();
        _updateValidator = new Mock<IValidator<UpdateAreaDto>>();

        _service = new AreaService(
            _context,
            _mapper,
            _createValidator.Object,
            _updateValidator.Object);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldCreateArea_WhenAreaIsValid()
    {
        // Arrange
        var dto = new CreateAreaDto
        {
            Name = "Brisbane"
        };

        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Brisbane");

        _context.Areas.Count().Should().Be(1);
        _context.Areas.First().Name.Should().Be("Brisbane");
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowDuplicateEntityException_WhenAreaAlreadyExists()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Simulate a valid request where
        // an Area named "Brisbane"
        // already exists in the database.
        // -----------------------------

        var dto = new CreateAreaDto
        {
            Name = "Brisbane"
        };

        // Mock validator to return success.
        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        // Seed the in-memory database with an existing Area.
        _context.Areas.Add(new Area
        {
            Name = "Brisbane"
        });

        await _context.SaveChangesAsync();

        // -----------------------------
        // Act & Assert
        // Purpose:
        // Calling CreateAsync should
        // throw DuplicateEntityException
        // because the Area already exists.
        // -----------------------------

        await Assert.ThrowsExceptionAsync<DuplicateEntityException>(
            () => _service.CreateAsync(dto));
    }

    [TestMethod]
    public async Task CreateAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // -----------------------------
        // Arrange
        // Purpose:
        // Simulate an invalid request.
        // The validator should fail and
        // the service should throw
        // ValidationException.
        // -----------------------------

        var dto = new CreateAreaDto
        {
            Name = ""
        };

        var failures = new List<ValidationFailure>
    {
        new ValidationFailure("Name", "Area name is required.")
    };

        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(failures));

        // -----------------------------
        // Act & Assert
        // Purpose:
        // Verify that validation failure
        // prevents the Area from being created.
        // -----------------------------

        await Assert.ThrowsExceptionAsync<ValidationException>(
            () => _service.CreateAsync(dto));

        // Verify nothing was saved.
        _context.Areas.Count().Should().Be(0);
    }

}