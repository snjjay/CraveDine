using EatKath.API.DTOs.Restaurant;
using EatKath.API.Validators.Restaurant;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Validators;

// Cuisine rules on the restaurant create/update validators. (RestaurantService
// enforces the same rules, plus that the ids exist.)
[TestClass]
public class RestaurantValidatorTests
{
    private readonly CreateRestaurantValidator _createValidator = new();
    private readonly UpdateRestaurantValidator _updateValidator = new();

    private static CreateRestaurantDto ValidCreateDto() => new()
    {
        OwnerId = 1,
        Name = "Spice Kitchen",
        Address = "Thamel, Kathmandu",
        AreaId = 1,
        PhoneNumber = "0400000000",
        Email = "spice@test.com",
        CuisineIds = new List<int> { 1, 2 }
    };

    private static UpdateRestaurantDto ValidUpdateDto() => new()
    {
        Name = "Spice Kitchen",
        Address = "Thamel, Kathmandu",
        AreaId = 1,
        PhoneNumber = "0400000000",
        Email = "spice@test.com"
    };

    [TestMethod]
    public void Create_ShouldPass_WithDistinctCuisines()
    {
        _createValidator.TestValidate(ValidCreateDto()).ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Create_ShouldFail_WhenNoCuisineIsSelected()
    {
        var dto = ValidCreateDto();
        dto.CuisineIds = new List<int>();

        _createValidator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.CuisineIds)
            .WithErrorMessage("Select at least one cuisine.");
    }

    [TestMethod]
    public void Create_ShouldFail_WhenACuisineIsSelectedTwice()
    {
        var dto = ValidCreateDto();
        dto.CuisineIds = new List<int> { 1, 1 };

        _createValidator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.CuisineIds)
            .WithErrorMessage("Each cuisine can only be selected once.");
    }

    [TestMethod]
    public void Update_ShouldPass_WhenCuisineIdsAreOmitted()
    {
        _updateValidator.TestValidate(ValidUpdateDto()).ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Update_ShouldPass_WithDistinctCuisines()
    {
        var dto = ValidUpdateDto();
        dto.CuisineIds = new List<int> { 3 };

        _updateValidator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Update_ShouldFail_WhenCuisineListIsSuppliedEmpty()
    {
        var dto = ValidUpdateDto();
        dto.CuisineIds = new List<int>();

        _updateValidator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.CuisineIds)
            .WithErrorMessage("Select at least one cuisine.");
    }

    [TestMethod]
    public void Update_ShouldFail_WhenACuisineIsSelectedTwice()
    {
        var dto = ValidUpdateDto();
        dto.CuisineIds = new List<int> { 3, 3 };

        _updateValidator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.CuisineIds)
            .WithErrorMessage("Each cuisine can only be selected once.");
    }
}
