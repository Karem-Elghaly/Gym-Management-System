using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Gym_Management_System.Extensions;

public static class ValidationExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
           
            var key = error.PropertyName.Split('.')[0];
            modelState.AddModelError(key, error.ErrorMessage);
        }
    }
}