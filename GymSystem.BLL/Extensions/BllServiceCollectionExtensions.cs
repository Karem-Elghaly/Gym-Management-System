using FluentValidation;
using GymSystem.BLL.Mapping;
using GymSystem.BLL.Services.Implementations;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace GymSystem.BLL.Extensions;

public static class BllServiceCollectionExtensions
{
    // this IServiceCollection: Extension Method، فبننادي builder.Services.AddBllServices()
    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {
        // AutoMapper: بيدور على كل Profile في نفس Assembly بتاع MappingProfile
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation: بيسجل كل الـ Validators أوتوماتيك
        services.AddValidatorsFromAssemblyContaining<MemberValidator>();

        // Scoped: نسخة واحدة لكل طلب HTTP، وده المناسب لأي حاجة بتستخدم DbContext
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}