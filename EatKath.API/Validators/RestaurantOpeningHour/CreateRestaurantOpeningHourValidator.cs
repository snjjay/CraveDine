using EatKath.API.DTOs.RestaurantOpeningHour;
using FluentValidation;

namespace EatKath.API.Validators.RestaurantOpeningHour
{
    public class CreateRestaurantOpeningHourValidator
        : AbstractValidator<CreateRestaurantOpeningHourDto>
    {
        public CreateRestaurantOpeningHourValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);

            RuleFor(x => x.CloseTime)
                .GreaterThan(x => x.OpenTime)
                .When(x => !x.IsClosed);
        }
    }
}