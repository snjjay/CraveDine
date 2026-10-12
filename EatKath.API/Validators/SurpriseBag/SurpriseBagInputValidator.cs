using EatKath.API.DTOs.SurpriseBag;
using FluentValidation;

namespace EatKath.API.Validators.SurpriseBag
{
    // Rules shared by the create and update validators. Rules that need the
    // current time or existing reservations are enforced by SurpriseBagService.
    public abstract class SurpriseBagInputValidator<T> : AbstractValidator<T>
        where T : SurpriseBagInputDto
    {
        public const int MaxQuantityTotal = 1000;

        protected SurpriseBagInputValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Description)
                .NotNull()
                .MaximumLength(1000);

            RuleFor(x => x.Conditions)
                .NotNull()
                .MaximumLength(1000);

            RuleFor(x => x.Category)
                .IsInEnum();

            // decimal(18,2) in the database: reject extra decimals rather
            // than letting SQL Server round them silently.
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .PrecisionScale(18, 2, true);

            RuleFor(x => x.EstimatedValue)
                .GreaterThan(x => x.Price)
                .WithMessage("Estimated value must be greater than the price.")
                .PrecisionScale(18, 2, true)
                .When(x => x.EstimatedValue.HasValue);

            RuleFor(x => x.QuantityTotal)
                .InclusiveBetween(1, MaxQuantityTotal);

            RuleFor(x => x.MaxPerCustomer)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(x => x.QuantityTotal)
                .WithMessage("Max per customer cannot be more than the total quantity.");

            RuleFor(x => x.PickupDate)
                .NotEmpty();

            // Same-day window: cannot cross midnight.
            RuleFor(x => x.PickupEndTime)
                .GreaterThan(x => x.PickupStartTime)
                .WithMessage("Pickup end time must be later than the start time.");
        }
    }
}
