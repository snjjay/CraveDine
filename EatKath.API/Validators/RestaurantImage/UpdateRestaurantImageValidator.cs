using EatKath.API.DTOs.RestaurantImage;
using FluentValidation;

namespace EatKath.API.Validators.RestaurantImage
{
    public class UpdateRestaurantImageValidator : AbstractValidator<UpdateRestaurantImageDto>
    {
        public UpdateRestaurantImageValidator()
        {
            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);
        }
    }
}