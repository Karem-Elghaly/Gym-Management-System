using FluentValidation.Results;

namespace GymSystem.BLL.Exceptions;

public class AppValidationException : Exception
{
    // اسم الحقل ← رسائل الخطأ بتاعته
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
    }
}