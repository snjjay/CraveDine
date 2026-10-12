using EatKath.API.DTOs.SurpriseBag;
using EatKath.API.DTOs.SurpriseBagReservation;
using EatKath.API.Validators.SurpriseBag;
using FluentValidation;

namespace EatKath.API.Validators.SurpriseBagReservation
{
    // Field rules only; stock, MaxPerCustomer and availability are checked
    // by SurpriseBagReservationService against the bag.
    public class CreateSurpriseBagReservationValidator : AbstractValidator<CreateSurpriseBagReservationDto>
    {
        public CreateSurpriseBagReservationValidator()
        {
            RuleFor(x => x.SurpriseBagId)
                .GreaterThan(0);

            // No bag holds more than MaxQuantityTotal.
            RuleFor(x => x.Quantity)
                .InclusiveBetween(1, SurpriseBagInputValidator<CreateSurpriseBagDto>.MaxQuantityTotal);
        }
    }
}
