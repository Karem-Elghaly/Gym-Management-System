using GymSystem.BLL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Middleware;

public class GlobalExceptionMiddleware
{
    
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

   
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            
            await _next(context);
        }
        catch (Exception ex)
        {
           
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Exception after response started");
                throw;
            }

            
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
                AppValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            if (status >= 500)
               
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            else
                
                _logger.LogWarning("{Type} on {Method} {Path}: {Message}",
                    ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);

            if (status == StatusCodes.Status500InternalServerError && _env.IsDevelopment())
                throw;

            context.Response.Clear();
            context.Response.StatusCode = status;

           
            if (WantsJson(context.Request))
            {
             
                ProblemDetails problem = ex is AppValidationException validation
                    ? new ValidationProblemDetails(validation.Errors) { Status = status, Title = title }
                    : new ProblemDetails
                    {
                        Status = status,
                        Title = title,
                        Detail = status < 500 ? ex.Message : null
                    };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }


    private static bool WantsJson(HttpRequest request)
        => request.Headers.XRequestedWith == "XMLHttpRequest"
           || request.Headers.Accept.ToString().Contains("application/json");
}