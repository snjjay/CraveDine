using EatKath.API.DTOs.SurpriseBagReservation;
using EatKath.API.Validators.SurpriseBagReservation;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EatKath.API.Tests.Validators;

[TestClass]
public class SurpriseBagReservationValidatorTests
{
    private readonly CreateSurpriseBagReservationValidator _createValidator = new();
    private readonly CollectSurpriseBagReservationValidator _collectValidator = new();

    [TestMethod]
    public void Create_ShouldPass_WithBagAndQuantity()
    {
        _createValidator.TestValidate(new CreateSurpriseBagReservationDto { SurpriseBagId = 1, Quantity = 2 })
            .ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Create_ShouldFail_WhenBagIsMissing()
    {
        _createValidator.TestValidate(new CreateSurpriseBagReservationDto { SurpriseBagId = 0, Quantity = 1 })
            .ShouldHaveValidationErrorFor(x => x.SurpriseBagId);
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(-1, false)]
    [DataRow(1, true)]
    [DataRow(1000, true)]
    [DataRow(1001, false)]
    public void Create_QuantityShouldBeBetween1And1000(int quantity, bool valid)
    {
        var result = _createValidator.TestValidate(new CreateSurpriseBagReservationDto { SurpriseBagId = 1, Quantity = quantity });

        if (valid)
            result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
        else
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [TestMethod]
    [DataRow("K7M2Q9XA", true)]
    [DataRow(" k7m2q9xa ", true)]
    [DataRow("ABCDEFGHJKMN", true)]   // 12 = the column limit
    [DataRow("ABCDEFGHJKMNP", false)] // 13
    [DataRow("", false)]
    [DataRow("   ", false)]
    [DataRow(null, false)]
    public void Collect_CodeShouldBePresent_AndFitTheColumn(string? code, bool valid)
    {
        var result = _collectValidator.TestValidate(new CollectSurpriseBagReservationDto { ConfirmationCode = code! });

        if (valid)
            result.ShouldNotHaveValidationErrorFor(x => x.ConfirmationCode);
        else
            result.ShouldHaveValidationErrorFor(x => x.ConfirmationCode);
    }
}
