using EatKath.API.DTOs.SurpriseBag;
using FluentValidation;

namespace EatKath.API.Validators.SurpriseBag
{
    public class CreateSurpriseBagValidator : SurpriseBagInputValidator<CreateSurpriseBagDto>
    {
        public CreateSurpriseBagValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);
        }
    }
}
