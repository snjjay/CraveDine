using EatKath.API.DTOs.RestaurantOpeningHour;
using FluentValidation;

namespace EatKath.API.Validators.RestaurantOpeningHour
{
    public class UpdateRestaurantOpeningHourValidator
        : AbstractValidator<UpdateRestaurantOpeningHourDto>
    {
        public UpdateRestaurantOpeningHourValidator()
        {
            RuleFor(x => x.CloseTime)
                .GreaterThan(x => x.OpenTime)
                .When(x => !x.IsClosed);
        }
    }
}