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
        }
    }
}