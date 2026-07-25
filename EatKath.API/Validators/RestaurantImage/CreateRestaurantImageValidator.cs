using EatKath.API.DTOs.RestaurantImage;
using FluentValidation;

namespace EatKath.API.Validators.RestaurantImage
{
    public class CreateRestaurantImageValidator : AbstractValidator<CreateRestaurantImageDto>
    {
        public CreateRestaurantImageValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);

            RuleFor(x => x.ImageUrl)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);
        }
    }
}