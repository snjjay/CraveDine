using EatKath.API.DTOs.MenuCategory;
using FluentValidation;

namespace EatKath.API.Validators
{
    public class UpdateMenuCategoryValidator : AbstractValidator<UpdateMenuCategoryDto>
    {
        public UpdateMenuCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0);
        }
    }
}