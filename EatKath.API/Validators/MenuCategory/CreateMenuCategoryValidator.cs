using EatKath.API.DTOs.MenuCategory;
using FluentValidation;

namespace EatKath.API.Validators
{
    public class CreateMenuCategoryValidator : AbstractValidator<CreateMenuCategoryDto>
    {
        public CreateMenuCategoryValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);
        }
    }
}