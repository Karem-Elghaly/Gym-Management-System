using FluentValidation;
using GymSystem.BLL.ViewModels.Members;

namespace GymSystem.BLL.Validators;

public class MemberValidator : AbstractValidator<MemberViewModel>
{
    public MemberValidator()
    {
        // الاسم: مش فاضي، من 3 لـ 100 حرف
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .Length(3, 100).WithMessage("Full name must be between 3 and 100 characters.");

        // التليفون: رقم مصري يبدأ بـ 010 أو 011 أو 012 أو 015 ويكمل 8 أرقام (إجمالي 11)
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^01[0125][0-9]{8}$").WithMessage("Phone must be a valid Egyptian number (11 digits).");

        // الإيميل: اختياري، لكن لو اتكتب لازم يكون صيغته سليمة
        // When بتشغل القاعدة بس لو الشرط اتحقق
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // تاريخ الميلاد: مطلوب ولازم يكون في الماضي والعمر مش أكتر من 100
        RuleFor(x => x.DateOfBirth)
            .NotNull().WithMessage("Date of birth is required.")
            .Must(d => d!.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth must be in the past.")
            .Must(d => d!.Value > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-100))
                .WithMessage("Date of birth is not realistic.")
            .When(x => x.DateOfBirth.HasValue);

        // النوع: لازم يتحدد وقيمته موجودة في الـ Enum
        RuleFor(x => x.Gender)
            .NotNull().WithMessage("Gender is required.")
            .IsInEnum().WithMessage("Gender is invalid.");

        RuleFor(x => x.Address).MaximumLength(250);

        // الصورة: اختيارية، لكن لو اترفعت نتأكد من الحجم والنوع
        When(x => x.ImageFile is not null, () =>
        {
            // أقصى حجم 2 ميجا
            RuleFor(x => x.ImageFile!.Length)
                .LessThanOrEqualTo(2 * 1024 * 1024).WithMessage("Image must be 2MB or less.");

            // الامتدادات المسموحة بس
            RuleFor(x => x.ImageFile!.FileName)
                .Must(name =>
                {
                    var ext = Path.GetExtension(name).ToLowerInvariant();
                    return ext is ".jpg" or ".jpeg" or ".png" or ".webp";
                }).WithMessage("Only jpg, jpeg, png or webp images are allowed.");
        });
    }
}