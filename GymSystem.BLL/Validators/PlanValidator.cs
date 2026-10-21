using FluentValidation;
using GymSystem.BLL.ViewModels.Plans;

namespace GymSystem.BLL.Validators;

public class PlanValidator : AbstractValidator<PlanViewModel>
{
    public PlanValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);

        // السعر لازم أكبر من صفر
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        // المدة بالأيام: من يوم واحد لحد 3 سنين
        RuleFor(x => x.DurationInDays)
            .InclusiveBetween(1, 1095).WithMessage("Duration must be between 1 and 1095 days.");
    }
}