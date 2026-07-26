using EatKath.API.DTOs.UserFavorite;
using FluentValidation;

namespace EatKath.API.Validators.UserFavorite
{
    public class RemoveUserFavoriteValidator : AbstractValidator<RemoveUserFavoriteDto>
    {
        public RemoveUserFavoriteValidator()
        {
            RuleFor(x => x.RestaurantId)
                .GreaterThan(0);
        }
    }
}