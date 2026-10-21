using FluentValidation;
using GymSystem.BLL.ViewModels.Classes;

namespace GymSystem.BLL.Validators;

public class ClassValidator : AbstractValidator<ClassViewModel>
{
    public ClassValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);

        RuleFor(x => x.TrainerId).NotNull().WithMessage("Please select a trainer.");

        RuleFor(x => x.StartTime).NotNull().WithMessage("Start time is required.");

        // وقت النهاية لازم يكون بعد البداية
        RuleFor(x => x.EndTime)
            .NotNull().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.")
            .When(x => x.StartTime.HasValue);

        // الميعاد في المستقبل: بنطبقه عند الإنشاء بس (Id == 0)، عشان تعديل حصة قديمة ما يترفضش
        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.Now).WithMessage("Class must start in the future.")
            .When(x => x.Id == 0 && x.StartTime.HasValue);

        // السعة من 1 لـ 200
        RuleFor(x => x.MaxCapacity)
            .InclusiveBetween(1, 200).WithMessage("Capacity must be between 1 and 200.");
    }
}