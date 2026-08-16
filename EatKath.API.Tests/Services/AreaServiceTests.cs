using AutoMapper;
using EatKath.API.Data;
using EatKath.API.DTOs.Area;
using EatKath.API.Entities;
using EatKath.API.Exceptions;
using EatKath.API.Interfaces;
using EatKath.API.Services;
using EatKath.API.Tests.Helpers; //Gives you your test helpers such as  TestDbContextFactory, MapperFactory
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results; //Gives you easy assertions such as //result.Should().NotBeNull(); //result.Name.Should().Be("Brisbane");
using Microsoft.VisualStudio.TestTools.UnitTesting; //Gives you [TestClass], [TestMethod], [TestInitialize], etc.
using Moq; //Allows you to create fake dependencies (mocks).

namespace EatKath.API.Tests.Services;

[TestClass]
public class AreaServiceTests
{
    //These are the things AreaService needs.
    private ApplicationDbContext _context = null!;
    private IMapper _mapper = null!;
    private Mock<IValidator<CreateAreaDto>> _createValidator = null!; //Create a fake validator for this test
    private Mock<IValidator<UpdateAreaDto>> _updateValidator = null!;
    private AreaService _service = null!;

    [TestInitialize]
    public void Setup() //Run this Setup before every test.
    {
        _context = TestDbContextFactory.Create();  //Give this test its own temporary database

        _mapper = MapperFactory.Create(); //Creates AutoMapper for the test. Think Give AreaService a working AutoMapper

        _createValidator = new Mock<IValidator<CreateAreaDto>>(); //Creating fake validators
        _updateValidator = new Mock<IValidator<UpdateAreaDto>>();

        _service = new AreaService( //Creating the AreaService// You are sayin Create AreaService here is your database>automapper>fake Created/Updatevalidator
            _context,
            _mapper,
            _createValidator.Object, //Give me the actual mocked object that AreaService can use
            _updateValidator.Object);
    }

    [TestMethod] //"This is a test that should be run
    public async Task CreateAsync_ShouldCreateArea_WhenAreaIsValid()
    {
        // Arrange
        var dto = new CreateAreaDto
        {
            Name = "Brisbane"
        };

        //When AreaService asks you to validate this DTO, pretend validation succeeded. SO AreaService>"Is Brisbane valid?">Fake Validator>Yes>Because this test is testing successful Area creation, not the validator.
        _createValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.CreateAsync(dto);//Now actually call AreaService.

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Brisbane"); //Did the service return an Area?

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

        await Assert.ThrowsExceptionAsync<DuplicateEntityException>( //I expect CreateAsync to throw DuplicateEntityException.
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


// ==========================================================
// TESTING - WHAT I NEED TO KNOW
// ==========================================================
//
// 🧪 Tests check whether my application works correctly.
//
// In EatKath:
// → Main focus is currently Service testing.
//
// Example:
// AreaService
//      ↑
// AreaServiceTests
// "Does AreaService work correctly?"
//
// ----------------------------------------------------------
//
// 🔑 TEST PATTERN:
//
// Arrange → prepare test data / mocks
//     ↓
// Act     → call the method being tested
//     ↓
// Assert  → check whether the result is correct
//
// ----------------------------------------------------------
//
// 🧩 Mocks
// → Fake dependencies so I can test one piece separately.
//
// ----------------------------------------------------------
//
// Common things to test:
//
// → Successful operation
// → Not found
// → Invalid input
// → Duplicate data
// → Business-rule failure
//
// ----------------------------------------------------------
//
// ▶️ Run tests:
// Visual Studio → Test Explorer → Run All
// ==========================================================



//🧪 AreaServiceTests
//        │
//        ├── Setup()
//        │     ├── Test Database
//        │     ├── AutoMapper
//        │     └── Fake Validators
//        │
//        ├── Test 1
//        │     "Valid Area"
//        │          ↓
//        │     CreateAsync()
//        │          ↓
//        │     ✅ Area created
//        │
//        ├── Test 2
//        │     "Duplicate Area"
//        │          ↓
//        │     CreateAsync()
//        │          ↓
//        │     ❌ DuplicateEntityException
//        │
//        └── Test 3
//              "Invalid Area"
//                   ↓
//              CreateAsync()
//                   ↓
//              ❌ ValidationException