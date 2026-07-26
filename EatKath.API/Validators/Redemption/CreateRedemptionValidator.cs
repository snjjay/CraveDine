using EatKath.API.DTOs.Redemption;
using FluentValidation;

namespace EatKath.API.Validators.Redemption
{
    public class CreateRedemptionValidator : AbstractValidator<CreateRedemptionDto>
    {
        public CreateRedemptionValidator()
        {
            RuleFor(x => x.DealId)
                .GreaterThan(0);

            RuleFor(x => x.ArrivalDate)
                .NotEmpty();

            RuleFor(x => x.ArrivalTime)
                .NotEmpty();

            RuleFor(x => x.GuestCount)
                .InclusiveBetween(1, 20)
                .WithMessage("Guest count must be between 1 and 20.");
        }
    }
}