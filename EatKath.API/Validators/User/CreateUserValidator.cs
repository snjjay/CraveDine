using EatKath.API.DTOs.User;
using FluentValidation;

namespace EatKath.API.Validators.User
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(255);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20);

            RuleFor(x => x.RoleId)
                .GreaterThan(0);
        }
    }
}