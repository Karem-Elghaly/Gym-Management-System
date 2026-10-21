using FluentValidation;
using GymSystem.BLL.ViewModels.Account;

namespace GymSystem.BLL.Validators;

public class LoginValidator : AbstractValidator<LoginViewModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}