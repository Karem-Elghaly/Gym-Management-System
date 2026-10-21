using Gym_Management_System.Extensions;
using Gym_Management_System.Middleware;
using GymSystem.BLL.Extensions;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;

// Logger مؤقت بيشتغل قبل ما الإعدادات تتحمل، عشان لو التطبيق وقع وهو بيبدأ نشوف السبب
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // بنستبدل نظام اللوج الافتراضي بـ Serilog، وبيقرا إعداداته من قسم "Serilog" في appsettings
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        // Enrich: بيضيف سياق إضافي على كل رسالة لوج
        .Enrich.FromLogContext());

    // AuthorizeFilter عام: كل Action محتاجة تسجيل دخول تلقائياً
    // ولو عايزين صفحة مفتوحة (زي Login) بنحط عليها [AllowAnonymous]
    // ده أأمن من إننا نفتكر نحط [Authorize] على كل Controller
    builder.Services.AddControllersWithViews(options =>
        options.Filters.Add(new AuthorizeFilter()));

    // تسجيل الداتابيز والـ Identity (من الـ PL) والـ Services والـ Validators (من الـ BLL)
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddBllServices();

    var app = builder.Build();

    // أول Migration وبيانات أولية (لازم قبل ما التطبيق يبدأ يستقبل طلبات)
    await app.MigrateAndSeedAsync();

    // ===== الـ Pipeline: الترتيب هنا مهم جداً =====

    // بيسجل سطر لوج واحد لكل طلب (Method, Path, Status, الوقت)
    app.UseSerilogRequestLogging();

    // لو الرد رجع بـ Status Code خطأ وفاضي، بيعيد التنفيذ على /Home/Error
    // لازم يكون قبل الـ Exception Middleware عشان يشوف الـ Status اللي بيحطه
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (!app.Environment.IsDevelopment())
        // بيقول للمتصفح يستخدم HTTPS دايماً (في الإنتاج بس)
        app.UseHsts();

    app.UseHttpsRedirection();

    // UseStaticFiles بتخدم أي ملف في wwwroot وقت التشغيل، ومنها صور الأعضاء المرفوعة
    // (MapStaticAssets بتاعة القالب بتخدم ملفات الـ Build بس، فصور الأعضاء مش هتظهر بيها)
    app.UseStaticFiles();

    app.UseRouting();

    // الترتيب إجباري: الأول نعرف المستخدم (Authentication) وبعدين نشوف صلاحياته (Authorization)
    app.UseAuthentication();
    app.UseAuthorization();

    // الراوت الافتراضي: /Controller/Action/Id
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    await app.RunAsync();
}
catch (Exception ex)
{
    // أي خطأ وقت البداية (زي الداتابيز مش موصلة) يتسجل قبل ما التطبيق يقفل
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    // بيضمن إن آخر رسائل اللوج اتكتبت في الملف قبل القفل
    await Log.CloseAndFlushAsync();
}