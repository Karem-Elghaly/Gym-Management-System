using Gym_Management_System.Extensions;
using Gym_Management_System.Middleware;
using GymSystem.BLL.Extensions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;


Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

   
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
       
        .Enrich.FromLogContext());

   
    builder.Services.AddControllersWithViews(options =>
        options.Filters.Add(new AuthorizeFilter()));

    
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddBllServices();

    var app = builder.Build();

  
    await app.MigrateAndSeedAsync();

   
    app.UseSerilogRequestLogging();

    
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (!app.Environment.IsDevelopment())
        
        app.UseHsts();

    app.UseHttpsRedirection();

    
    app.UseStaticFiles();

    app.UseRouting();

    
    app.UseAuthentication();
    app.UseAuthorization();

   
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    await app.RunAsync();
}
catch (Exception ex)
{
  
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{

    await Log.CloseAndFlushAsync();
}