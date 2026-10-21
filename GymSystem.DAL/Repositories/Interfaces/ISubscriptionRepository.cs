using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface ISubscriptionRepository : IGenericRepository<Subscription>
{
    // الاشتراك النشط لعضو (لمنع اشتراكين نشطين)
    Task<Subscription?> GetActiveByMemberAsync(int memberId);

    // اشتراك مع العضو والباقة والمدفوعات
    Task<Subscription?> GetWithDetailsAsync(int id);

    // كل اشتراكات عضو
    Task<IEnumerable<Subscription>> GetByMemberAsync(int memberId);

    // الاشتراكات اللي هتنتهي خلال عدد أيام (للـ Dashboard)
    Task<IEnumerable<Subscription>> GetExpiringAsync(int withinDays);

    // الاشتراكات النشطة اللي تاريخها عدّى (لتحويلها Expired)
    Task<IEnumerable<Subscription>> GetOverdueActiveAsync();

    // قائمة اشتراكات بفلتر الحالة + Pagination
    Task<(IEnumerable<Subscription> Items, int TotalCount)> GetPagedAsync(
        SubscriptionStatus? status, int page, int pageSize);
}