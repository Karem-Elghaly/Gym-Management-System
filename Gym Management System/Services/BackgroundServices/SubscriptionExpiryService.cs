using GymSystem.BLL.Services.Interfaces;

namespace Gym_Management_System.BackgroundServices;


public class SubscriptionExpiryService : BackgroundService
{
    
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

   
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryService> _logger;

    public SubscriptionExpiryService(IServiceScopeFactory scopeFactory, ILogger<SubscriptionExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        using var timer = new PeriodicTimer(Interval);

        try
        {
            
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
          
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

            var count = await service.ExpireOverdueAsync();
            _logger.LogInformation("Expiry job finished. {Count} subscriptions expired", count);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            
            _logger.LogError(ex, "Expiry job failed");
        }
    }
}