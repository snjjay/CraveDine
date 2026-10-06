using EatKath.API.DTOs.Restaurant;
using FluentValidation;

namespace EatKath.API.Validators.Restaurant
{
    public class UpdateRestaurantValidator : AbstractValidator<UpdateRestaurantDto>
    {
        public UpdateRestaurantValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.AreaId)
                .GreaterThan(0);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(20);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Website)
                .MaximumLength(200);

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500);

            // Cuisines are optional on update (null = unchanged), but a
            // supplied list replaces the set and must not be empty.
            When(x => x.CuisineIds != null, () =>
            {
                RuleFor(x => x.CuisineIds!)
                    .NotEmpty()
                    .WithMessage("Select at least one cuisine.")
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("Each cuisine can only be selected once.");

                RuleForEach(x => x.CuisineIds)
                    .GreaterThan(0);
            });
        }
    }
}