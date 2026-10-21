using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    // مدفوعات اشتراك معين
    Task<IEnumerable<Payment>> GetBySubscriptionAsync(int subscriptionId);

    // مدفوعات عضو (عن طريق اشتراكاته)
    Task<IEnumerable<Payment>> GetByMemberAsync(int memberId);

    // إجمالي الإيرادات في فترة
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);

    // إجمالي المدفوع لاشتراك (لمعرفة المتبقي)
    Task<decimal> GetTotalPaidForSubscriptionAsync(int subscriptionId);

    // بترجع مجموع المدفوع لكل اشتراك في Query واحد
    // الـ Key = SubscriptionId، والـ Value = إجمالي المدفوع
    // أحسن من إننا ننادي GetTotalPaidForSubscriptionAsync جوه لوب (مشكلة N+1 Queries)
    Task<Dictionary<int, decimal>> GetPaidTotalsAsync(IEnumerable<int> subscriptionIds);
}