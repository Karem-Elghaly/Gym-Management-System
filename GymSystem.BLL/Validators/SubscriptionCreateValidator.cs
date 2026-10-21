using FluentValidation;
using GymSystem.BLL.ViewModels.Subscriptions;

namespace GymSystem.BLL.Validators;

public class SubscriptionCreateValidator : AbstractValidator<SubscriptionCreateViewModel>
{
    public SubscriptionCreateValidator()
    {
        RuleFor(x => x.MemberId)
            .NotNull().WithMessage("Please select a member.");

        RuleFor(x => x.PlanId)
            .NotNull().WithMessage("Please select a plan.");

        // نسمح ببداية اشتراك من أسبوع فات (تسجيل متأخر) لحد سنة قدام
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7))
                .WithMessage("Start date cannot be older than 7 days.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1))
                .WithMessage("Start date is too far in the future.");

        // الدفعة الأولى مش بالسالب (صفر مسموح)
        RuleFor(x => x.InitialPayment)
            .GreaterThanOrEqualTo(0).WithMessage("Payment cannot be negative.");

        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}