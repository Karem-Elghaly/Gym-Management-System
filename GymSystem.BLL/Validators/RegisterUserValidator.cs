using FluentValidation;
using GymSystem.BLL.ViewModels.Account;
using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUserViewModel>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().Length(3, 100);

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        // الباسورد: 8 أحرف على الأقل، حرف كبير، حرف صغير، رقم
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a number.");

        // التأكيد لازم يطابق الباسورد
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        // الـ Role لازم واحد من الثلاثة المعروفين
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => r is UserRoles.Admin or UserRoles.Receptionist or UserRoles.Trainer)
            .WithMessage("Role is invalid.");
    }
}