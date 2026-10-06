using EatKath.API.DTOs.Restaurant;
using FluentValidation;

namespace EatKath.API.Validators.Restaurant
{
    public class CreateRestaurantValidator : AbstractValidator<CreateRestaurantDto>
    {
        public CreateRestaurantValidator()
        {
            RuleFor(x => x.OwnerId)
                .GreaterThan(0);

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

            // Same rules RestaurantService enforces (it also checks the
            // ids exist).
            RuleFor(x => x.CuisineIds)
                .NotEmpty()
                .WithMessage("Select at least one cuisine.")
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Each cuisine can only be selected once.");

            RuleForEach(x => x.CuisineIds)
                .GreaterThan(0);
        }
    }
}