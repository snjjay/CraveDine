using EatKath.API.DTOs.SurpriseBagReservation;
using FluentValidation;

namespace EatKath.API.Validators.SurpriseBagReservation
{
    public class CollectSurpriseBagReservationValidator : AbstractValidator<CollectSurpriseBagReservationDto>
    {
        public CollectSurpriseBagReservationValidator()
        {
            // Same limit as the database column (nvarchar(12)).
            RuleFor(x => x.ConfirmationCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(code => code.Trim().Length <= 12)
                .WithMessage("Confirmation code must be 12 characters or fewer.");
        }
    }
}
