using GymSystem.BLL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Middleware;

public class GlobalExceptionMiddleware
{
    // الـ Middleware اللي بعدنا في الـ Pipeline
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

    // الدالة اللي ASP.NET بيناديها مع كل طلب
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // نسيب الطلب يكمل لباقي الـ Pipeline (Controllers وغيرها)
            await _next(context);
        }
        catch (Exception ex)
        {
            // لو الرد بدأ يتبعت للمتصفح مينفعش نغير الـ Status، فنسجل ونعيد الرمي
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Exception after response started");
                throw;
            }

            // نحول نوع الخطأ لـ Status Code وعنوان
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
                AppValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            if (status >= 500)
                // الأخطاء الحقيقية: بنسجل الـ Stack Trace كامل
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            else
                // الأخطاء المتوقعة: Warning من غير Stack Trace عشان اللوج ما يتملاش
                _logger.LogWarning("{Type} on {Method} {Path}: {Message}",
                    ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);

            // في التطوير نعيد رمي الخطأ عشان صفحة المطور تعرض التفاصيل
            if (status == StatusCodes.Status500InternalServerError && _env.IsDevelopment())
                throw;

            context.Response.Clear();
            context.Response.StatusCode = status;

            // طلبات AJAX/API بنرد عليها JSON، وطلبات الصفحات بنسيبها فاضية
            // عشان StatusCodePages يعرض صفحة الخطأ
            if (WantsJson(context.Request))
            {
                // لا نعرض تفاصيل الخطأ الداخلي في الـ 500 أبداً
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

    // هل الطلب جاي من AJAX أو بيطلب JSON؟
    private static bool WantsJson(HttpRequest request)
        => request.Headers.XRequestedWith == "XMLHttpRequest"
           || request.Headers.Accept.ToString().Contains("application/json");
}