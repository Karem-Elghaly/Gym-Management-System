using FluentValidation;
using GymSystem.BLL.ViewModels.Trainers;

namespace GymSystem.BLL.Validators;

public class TrainerValidator : AbstractValidator<TrainerViewModel>
{
    public TrainerValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().Length(3, 100);
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^01[0125][0-9]{8}$").WithMessage("Phone must be a valid Egyptian number (11 digits).");

        RuleFor(x => x.Bio).MaximumLength(500);
    }
}