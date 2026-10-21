using FluentValidation;
using GymSystem.BLL.ViewModels.Payments;

namespace GymSystem.BLL.Validators;

public class PaymentValidator : AbstractValidator<PaymentViewModel>
{
    public PaymentValidator()
    {
        RuleFor(x => x.SubscriptionId).NotNull().WithMessage("Please select a subscription.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}