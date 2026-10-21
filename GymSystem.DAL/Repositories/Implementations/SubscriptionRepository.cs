using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(GymDbContext context) : base(context) { }

    // اشتراك Active وتاريخه لسه ما انتهاش
    public async Task<Subscription?> GetActiveByMemberAsync(int memberId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.MemberId == memberId
            && s.Status == SubscriptionStatus.Active
            && s.EndDate >= today);
    }

    public async Task<Subscription?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<Subscription>> GetByMemberAsync(int memberId)
        => await _dbSet.AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.MemberId == memberId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

    // الاشتراكات النشطة اللي نهايتها بين النهاردة وبعد withinDays يوم
    public async Task<IEnumerable<Subscription>> GetExpiringAsync(int withinDays)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var limit = today.AddDays(withinDays);
        return await _dbSet.AsNoTracking()
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active
                        && s.EndDate >= today && s.EndDate <= limit)
            .OrderBy(s => s.EndDate)
            .ToListAsync();
    }

    // نشطة بس نهايتها عدّت، من غير AsNoTracking لأننا هنعدل حالتها
    public async Task<IEnumerable<Subscription>> GetOverdueActiveAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate < today)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Subscription> Items, int TotalCount)> GetPagedAsync(
        SubscriptionStatus? status, int page, int pageSize)
    {
        var query = _dbSet.AsNoTracking()
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .AsQueryable();

        // لو اتحددت حالة نفلتر بيها
        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}