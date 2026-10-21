using FluentValidation;
using GymSystem.BLL.ViewModels.Subscriptions;

namespace GymSystem.BLL.Validators;

public class SubscriptionRenewValidator : AbstractValidator<SubscriptionRenewViewModel>
{
    public SubscriptionRenewValidator()
    {
        RuleFor(x => x.PlanId).NotNull().WithMessage("Please select a plan.");

        RuleFor(x => x.InitialPayment)
            .GreaterThanOrEqualTo(0).WithMessage("Payment cannot be negative.");

        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}