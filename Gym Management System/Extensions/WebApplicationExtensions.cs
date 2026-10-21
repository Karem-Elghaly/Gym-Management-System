using GymSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Gym_Management_System.Extensions;

public static class WebApplicationExtensions
{
    // بتطبق الـ Migrations وتزرع البيانات الأولية وقت بداية التشغيل
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        // الـ DbContext Scoped، ومفيش طلب HTTP دلوقتي، فبنعمل Scope يدوي
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<GymDbContext>();

        // MigrateAsync بتنشئ الداتابيز لو مش موجودة وتطبق أي Migration ناقصة
        await db.Database.MigrateAsync();

        // بيانات الأدمن من الـ Configuration (appsettings.Development.json أو User Secrets)
        var email = app.Configuration["SeedAdmin:Email"];
        var password = app.Configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            app.Logger.LogWarning("SeedAdmin settings not found. Admin account was not created.");

        await DbSeeder.SeedAsync(services, email, password);
    }
}