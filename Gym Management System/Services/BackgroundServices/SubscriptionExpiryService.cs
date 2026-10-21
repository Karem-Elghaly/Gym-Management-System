using GymSystem.BLL.Services.Interfaces;

namespace Gym_Management_System.BackgroundServices;

// BackgroundService: كلاس بيشتغل في الخلفية طول عمر التطبيق، من غير ما حد يطلبه
public class SubscriptionExpiryService : BackgroundService
{
    // بنشتغل كل 24 ساعة
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    // الـ BackgroundService بيتسجل Singleton، لكن ISubscriptionService Scoped
    // فمينفعش نحقنها مباشرة، بنحقن المصنع ونعمل Scope بإيدينا كل مرة
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryService> _logger;

    public SubscriptionExpiryService(IServiceScopeFactory scopeFactory, ILogger<SubscriptionExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // PeriodicTimer: مؤقت بيصحى كل Interval من غير ما نستخدم Task.Delay في لوب
        using var timer = new PeriodicTimer(Interval);

        try
        {
            // do-while: نشتغل أول مرة فوراً عند بداية التطبيق، وبعدين كل 24 ساعة
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // التطبيق بيقفل: ده طبيعي، منعملش حاجة
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Scope جديد = DbContext جديد لكل دورة
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

            var count = await service.ExpireOverdueAsync();
            _logger.LogInformation("Expiry job finished. {Count} subscriptions expired", count);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            // أي خطأ نسجله بس، عشان الـ Job ما يوقعش التطبيق كله
            _logger.LogError(ex, "Expiry job failed");
        }
    }
}