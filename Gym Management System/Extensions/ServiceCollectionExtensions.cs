using Gym_Management_System.BackgroundServices;
using Gym_Management_System.Services;
using GymSystem.DAL.Common;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gym_Management_System.Extensions;

public static class ServiceCollectionExtensions
{
    // بتسجل كل اللي يخص الداتابيز والـ Identity والـ Cookies
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // الـ Connection String من appsettings.json، ولو مش موجود نوقف التشغيل برسالة واضحة
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // تسجيل الـ DbContext بـ SQL Server (Scoped تلقائياً: نسخة لكل طلب)
        services.AddDbContext<GymDbContext>(options =>
         options.UseSqlServer(connectionString,
        // بنقول لـ EF إن ملفات الـ Migrations بتتحط وتتقرا من مشروع الـ DAL
        // لأن الـ DbContext هناك، ومشروع الـ MVC مجرد Startup
        sql => sql.MigrationsAssembly("GymSystem.DAL")));
        // مطلوب عشان CurrentUserService يوصل للـ HttpContext
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // الاسم الكامل لأن عندنا Namespace وكلاس بنفس الاسم UnitOfWork
        services.AddScoped<IUnitOfWork, GymSystem.DAL.UnitOfWork.UnitOfWork>();

        // ===== ASP.NET Identity =====
        // IdentityRole: كلاس الـ Role الجاهز. ApplicationUser: المستخدم بتاعنا
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // سياسة الباسورد (متطابقة مع RegisterUserValidator)
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;

            // القفل: بعد 5 محاولات غلط الحساب بيتقفل 5 دقايق (حماية من تخمين الباسورد)
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.AllowedForNewUsers = true;

            // ممنوع إيميلين متشابهين
            options.User.RequireUniqueEmail = true;
        })
        // بنقول لـ Identity يخزن المستخدمين والـ Roles في الـ DbContext بتاعنا
        .AddEntityFrameworkStores<GymDbContext>()
        // Token Providers: لازمين لو استخدمنا Reset Password بعدين
        .AddDefaultTokenProviders();

        // ===== إعدادات الـ Cookie =====
        services.ConfigureApplicationCookie(options =>
        {
            // لو حد مش مسجل دخول وفتح صفحة محمية يتحول هنا
            options.LoginPath = "/Account/Login";
            // لو مسجل دخول لكن مالوش صلاحية
            options.AccessDeniedPath = "/Account/AccessDenied";

            // الكوكي مش بيتقرا من JavaScript (حماية من XSS)
            options.Cookie.HttpOnly = true;

            // الجلسة 8 ساعات (يوم شغل)، وبتتجدد طول ما الموظف شغال
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        // الـ Background Service اللي بيقفل الاشتراكات المنتهية
        services.AddHostedService<SubscriptionExpiryService>();

        return services;
    }
}