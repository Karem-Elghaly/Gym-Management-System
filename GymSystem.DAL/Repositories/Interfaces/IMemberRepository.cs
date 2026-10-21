using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

// بيرث كل دوال الـ Generic ويزود عليها دوال خاصة بالأعضاء
public interface IMemberRepository : IGenericRepository<Member>
{
    // بحث بالتليفون (بنستخدمه لمنع التكرار)
    Task<Member?> GetByPhoneAsync(string phone);

    // عضو مع اشتراكاته (Include) لصفحة التفاصيل
    Task<Member?> GetWithSubscriptionsAsync(int id);

    // قائمة أعضاء بـ Search + Pagination. بترجع الأعضاء + إجمالي العدد (Tuple)
    Task<(IEnumerable<Member> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize);
}