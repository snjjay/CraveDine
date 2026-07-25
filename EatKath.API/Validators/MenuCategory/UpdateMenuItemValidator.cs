using EatKath.API.DTOs.MenuItem;
using FluentValidation;

namespace EatKath.API.Validators
{
    public class UpdateMenuItemValidator : AbstractValidator<UpdateMenuItemDto>
    {
        public UpdateMenuItemValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Price)
                .GreaterThan(0);
        }
    }
}