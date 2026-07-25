using EatKath.API.DTOs.MenuItem;
using FluentValidation;

namespace EatKath.API.Validators
{
    public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemDto>
    {
        public CreateMenuItemValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);

            RuleFor(x => x.MenuCategoryId)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Price)
                .GreaterThan(0);
        }
    }
}