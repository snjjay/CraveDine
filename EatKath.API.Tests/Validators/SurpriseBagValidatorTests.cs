using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.Enums;
using EatKath.API.Validators.SurpriseBag;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Validators;

// Field rules on the Surprise Bag create/update validators. Rules needing the
// current time or existing reservations are tested in SurpriseBagServiceTests.
[TestClass]
public class SurpriseBagValidatorTests
{
    private readonly CreateSurpriseBagValidator _createValidator = new();
    private readonly UpdateSurpriseBagValidator _updateValidator = new();

    private static CreateSurpriseBagDto ValidCreateDto() => new()
    {
        RestaurantId = 1,
        Title = "End-of-day bakery bag",
        Category = SurpriseBagCategory.Bakery,
        Description = "Breads and pastries",
        Conditions = "Contains gluten",
        Price = 250m,
        EstimatedValue = 750m,
        QuantityTotal = 5,
        MaxPerCustomer = 2,
        PickupDate = new DateOnly(2026, 11, 10),
        PickupStartTime = new TimeOnly(18, 0),
        PickupEndTime = new TimeOnly(19, 30)
    };

    private static UpdateSurpriseBagDto ValidUpdateDto() => new()
    {
        Title = "End-of-day bakery bag",
        Category = SurpriseBagCategory.Bakery,
        Price = 250m,
        QuantityTotal = 5,
        MaxPerCustomer = 1,
        PickupDate = new DateOnly(2026, 11, 10),
        PickupStartTime = new TimeOnly(18, 0),
        PickupEndTime = new TimeOnly(19, 30),
        IsActive = false
    };

    [TestMethod]
    public void Create_ShouldPass_WithValidBag()
    {
        _createValidator.TestValidate(ValidCreateDto()).ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Update_ShouldPass_WithValidBag_WithoutEstimatedValue()
    {
        _updateValidator.TestValidate(ValidUpdateDto()).ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Create_ShouldFail_WhenRestaurantIdIsMissing()
    {
        var dto = ValidCreateDto();
        dto.RestaurantId = 0;

        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.RestaurantId);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void ShouldFail_WhenTitleIsEmpty(string title)
    {
        var dto = ValidCreateDto();
        dto.Title = title;

        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void ShouldEnforceTextLengths()
    {
        var atLimit = ValidCreateDto();
        atLimit.Title = new string('a', 150);
        atLimit.Description = new string('a', 1000);
        atLimit.Conditions = new string('a', 1000);
        _createValidator.TestValidate(atLimit).ShouldNotHaveAnyValidationErrors();

        var overLimit = ValidCreateDto();
        overLimit.Title = new string('a', 151);
        overLimit.Description = new string('a', 1001);
        overLimit.Conditions = new string('a', 1001);
        var result = _createValidator.TestValidate(overLimit);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Conditions);
    }

    [TestMethod]
    public void ShouldFail_WhenCategoryIsNotDefined()
    {
        var dto = ValidUpdateDto();
        dto.Category = (SurpriseBagCategory)99;

        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Category);
    }

    [TestMethod]
    public void Price_ShouldAllowZero_ButNotNegativeOrMoreThanTwoDecimals()
    {
        var free = ValidUpdateDto();
        free.Price = 0m;
        _updateValidator.TestValidate(free).ShouldNotHaveValidationErrorFor(x => x.Price);

        var negative = ValidUpdateDto();
        negative.Price = -1m;
        _updateValidator.TestValidate(negative).ShouldHaveValidationErrorFor(x => x.Price);

        var tooPrecise = ValidUpdateDto();
        tooPrecise.Price = 250.555m;
        _updateValidator.TestValidate(tooPrecise).ShouldHaveValidationErrorFor(x => x.Price);
    }

    [TestMethod]
    [DataRow("250")]
    [DataRow("100")]
    public void EstimatedValue_ShouldFail_WhenNotGreaterThanPrice(string estimatedValue)
    {
        var dto = ValidCreateDto();
        dto.EstimatedValue = decimal.Parse(estimatedValue);

        _createValidator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.EstimatedValue)
            .WithErrorMessage("Estimated value must be greater than the price.");
    }

    [TestMethod]
    public void EstimatedValue_ShouldFail_WithMoreThanTwoDecimals()
    {
        var dto = ValidCreateDto();
        dto.EstimatedValue = 750.123m;

        _createValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.EstimatedValue);
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(1000, true)]
    [DataRow(1001, false)]
    public void QuantityTotal_ShouldBeBetween1And1000(int quantityTotal, bool valid)
    {
        var dto = ValidCreateDto();
        dto.QuantityTotal = quantityTotal;
        dto.MaxPerCustomer = 1;

        var result = _createValidator.TestValidate(dto);

        if (valid)
            result.ShouldNotHaveValidationErrorFor(x => x.QuantityTotal);
        else
            result.ShouldHaveValidationErrorFor(x => x.QuantityTotal);
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1, true)]
    [DataRow(5, true)]
    [DataRow(6, false)]
    public void MaxPerCustomer_ShouldBeBetween1AndQuantityTotal(int maxPerCustomer, bool valid)
    {
        var dto = ValidCreateDto(); // QuantityTotal = 5
        dto.MaxPerCustomer = maxPerCustomer;

        var result = _createValidator.TestValidate(dto);

        if (valid)
            result.ShouldNotHaveValidationErrorFor(x => x.MaxPerCustomer);
        else
            result.ShouldHaveValidationErrorFor(x => x.MaxPerCustomer);
    }

    [TestMethod]
    [DataRow(18, 0)]  // equal to start
    [DataRow(17, 59)] // before start
    public void PickupEndTime_ShouldBeAfterStartTime(int endHour, int endMinute)
    {
        var dto = ValidUpdateDto(); // starts 18:00
        dto.PickupEndTime = new TimeOnly(endHour, endMinute);

        _updateValidator.TestValidate(dto)
            .ShouldHaveValidationErrorFor(x => x.PickupEndTime)
            .WithErrorMessage("Pickup end time must be later than the start time.");
    }

    [TestMethod]
    public void PickupDate_ShouldBeRequired()
    {
        var dto = ValidUpdateDto();
        dto.PickupDate = default;

        _updateValidator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.PickupDate);
    }
}
