using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GymSystem.DAL.Data;

// static class: مفيش حاجة هنعمل منها Object، هي دالة واحدة بنناديها مرة وقت التشغيل
public static class DbSeeder
{
    // adminEmail و adminPassword nullable: لو مش موجودين بنتخطى إنشاء الأدمن بس
    // الباسورد جاي من الـ Configuration مش مكتوب في الكود، عشان ما يترفعش على GitHub
    public static async Task SeedAsync(IServiceProvider services, string? adminEmail, string? adminPassword)
    {
        // بنجيب الخدمات من الـ DI Container
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<GymDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // ===== 1) الـ Roles =====
        // نلف على الـ 3 Roles، ولو واحد مش موجود ننشئه (فالتشغيل المتكرر مش بيعمل تكرار)
        foreach (var role in new[] { UserRoles.Admin, UserRoles.Receptionist, UserRoles.Trainer })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ===== 2) حساب الأدمن =====
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword)
            && await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                FullName = "System Admin",
                Email = adminEmail,
                UserName = adminEmail,      // Identity محتاج UserName وإحنا بنستخدم الإيميل
                EmailConfirmed = true
            };

            // CreateAsync بتعمل Hash للباسورد، مش بتخزنه نص عادي
            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);
            else
                // لو الباسورد ضعيف مثلاً: نسجل السبب بدل ما نكسر التشغيل
                logger.LogError("Failed to create admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // ===== 3) باقات افتراضية =====
        // IgnoreQueryFilters: عشان لو الباقات اتمسحت Soft ما نرجعش نزرعها تاني
        if (!await context.MembershipPlans.IgnoreQueryFilters().AnyAsync())
        {
            context.MembershipPlans.AddRange(
                new MembershipPlan { Name = "Monthly", Description = "1 month access", Price = 500, DurationInDays = 30 },
                new MembershipPlan { Name = "Quarterly", Description = "3 months access", Price = 1350, DurationInDays = 90 },
                new MembershipPlan { Name = "Yearly", Description = "12 months access", Price = 4800, DurationInDays = 365 }
            );
            await context.SaveChangesAsync();
        }
    }
}