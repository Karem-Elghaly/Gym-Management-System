using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(GymDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetBySubscriptionAsync(int subscriptionId)
        => await _dbSet.AsNoTracking()
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

    public async Task<IEnumerable<Payment>> GetByMemberAsync(int memberId)
        => await _dbSet.AsNoTracking()
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .Where(p => p.Subscription.MemberId == memberId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

    // SumAsync على decimal بترجع 0 لو مفيش سجلات
    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        => await _dbSet
            .Where(p => p.PaidAt >= from && p.PaidAt < to)
            .SumAsync(p => p.Amount);

    public async Task<decimal> GetTotalPaidForSubscriptionAsync(int subscriptionId)
        => await _dbSet
            .Where(p => p.SubscriptionId == subscriptionId)
            .SumAsync(p => p.Amount);

    public async Task<Dictionary<int, decimal>> GetPaidTotalsAsync(IEnumerable<int> subscriptionIds)
    {
        // نحول لـ List عشان EF يترجمها لـ IN (...) في SQL
        var ids = subscriptionIds.ToList();

        return await _dbSet
            .Where(p => ids.Contains(p.SubscriptionId))
            // نجمّع المدفوعات حسب الاشتراك
            .GroupBy(p => p.SubscriptionId)
            // لكل مجموعة: المفتاح + مجموع المبالغ
            .Select(g => new { g.Key, Total = g.Sum(p => p.Amount) })
            // نحوّلها لقاموس
            .ToDictionaryAsync(x => x.Key, x => x.Total);
    }
}