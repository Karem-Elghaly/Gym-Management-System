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

        RuleFor(x => x.ClassDate).NotNull().WithMessage("Please choose the class date.");
        RuleFor(x => x.StartTimeOfDay).NotNull().WithMessage("Please choose the start time.");
        RuleFor(x => x.EndTimeOfDay).NotNull().WithMessage("Please choose the end time.");

        // وقت النهاية لازم يكون بعد البداية (بنفس اليوم)
        RuleFor(x => x.EndTimeOfDay)
            .Must((m, end) => end > m.StartTimeOfDay)
            .WithMessage("End time must be after the start time.")
            .When(x => x.StartTimeOfDay.HasValue && x.EndTimeOfDay.HasValue);

        // الحصة الجديدة لازم تبدأ في المستقبل (التعديل مستثنى عشان حصة قديمة ما تترفضش)
        RuleFor(x => x.StartTimeOfDay)
            .Must((m, start) => m.ClassDate!.Value.ToDateTime(start!.Value) > DateTime.Now)
            .WithMessage("The class must start in the future.")
            .When(x => x.Id == 0 && x.ClassDate.HasValue && x.StartTimeOfDay.HasValue);

        RuleFor(x => x.MaxCapacity)
            .InclusiveBetween(1, 200).WithMessage("Capacity must be between 1 and 200.");
    }
}