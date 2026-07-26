using EatKath.API.DTOs.Deal;
using FluentValidation;

namespace EatKath.API.Validators.Deal
{
    public class CreateDealValidator : AbstractValidator<CreateDealDto>
    {
        public CreateDealValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.OfferType)
                .IsInEnum();

            RuleFor(x => x.StartDate)
                .NotEmpty();

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End Date must be greater than or equal to Start Date.");

            RuleFor(x => x.StartTime)
                .NotEmpty();

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("End Time must be later than Start Time.");

            RuleFor(x => x.MaximumGuests)
                .InclusiveBetween(1, 50);

            RuleFor(x => x.AdvanceRedeemMinutes)
                .InclusiveBetween(0, 180);

            RuleFor(x => x.DailyRedemptionLimit)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.PromoImageUrl)
                .MaximumLength(500);

            RuleFor(x => x.TermsAndConditions)
                .MaximumLength(2000);
        }
    }
}