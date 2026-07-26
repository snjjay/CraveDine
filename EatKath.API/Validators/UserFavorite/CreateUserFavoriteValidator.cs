using EatKath.API.DTOs.UserFavorite;
using FluentValidation;

namespace EatKath.API.Validators.UserFavorite
{
    public class CreateUserFavoriteValidator : AbstractValidator<CreateUserFavoriteDto>
    {
        public CreateUserFavoriteValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);
        }
    }
}