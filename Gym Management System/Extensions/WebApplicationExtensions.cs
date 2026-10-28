using GymSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Gym_Management_System.Extensions;

public static class WebApplicationExtensions
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<GymDbContext>();
        await db.Database.MigrateAsync();

        var email = app.Configuration["SeedAdmin:Email"];
        var password = app.Configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            app.Logger.LogWarning("SeedAdmin settings not found. Admin account was not created.");

        await DbSeeder.SeedAsync(services, email, password);

        // البيانات التجريبية: في بيئة التطوير فقط، وبعد ما تفعّلها من الإعدادات
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("SeedDevData"))
            await DevDataSeeder.SeedAsync(db);
    }
}