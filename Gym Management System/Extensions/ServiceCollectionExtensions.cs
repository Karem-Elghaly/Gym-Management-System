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
    
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
       
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        
        services.AddDbContext<GymDbContext>(options =>
         options.UseSqlServer(connectionString,
       
        sql => sql.MigrationsAssembly("GymSystem.DAL")));
       
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

       
        services.AddScoped<IUnitOfWork, GymSystem.DAL.UnitOfWork.UnitOfWork>();

       
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;

           
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.AllowedForNewUsers = true;

           
            options.User.RequireUniqueEmail = true;
        })
        
        .AddEntityFrameworkStores<GymDbContext>()
        
        .AddDefaultTokenProviders();

        
        services.ConfigureApplicationCookie(options =>
        {
           
            options.LoginPath = "/Account/Login";
           
            options.AccessDeniedPath = "/Account/AccessDenied";

           
            options.Cookie.HttpOnly = true;

          
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

     
        services.AddHostedService<SubscriptionExpiryService>();

        return services;
    }
}