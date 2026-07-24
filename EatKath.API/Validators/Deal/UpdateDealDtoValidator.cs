using EatKath.API.DTOs.Deal;
using FluentValidation;

namespace EatKath.API.Validators.Deal
{
    public class UpdateDealDtoValidator : AbstractValidator<UpdateDealDto>
    {
        public UpdateDealDtoValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.OriginalPrice)
                .GreaterThan(0);

            RuleFor(x => x.DiscountedPrice)
                .GreaterThan(0);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End Date must be after Start Date.");
        }
    }
}