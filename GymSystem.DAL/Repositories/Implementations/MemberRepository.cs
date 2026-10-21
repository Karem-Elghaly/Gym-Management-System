using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

// بيرث GenericRepository<Member> (ياخد كل الدوال الجاهزة) وبينفذ IMemberRepository
public class MemberRepository : GenericRepository<Member>, IMemberRepository
{
    // بنمرر الـ context للأب
    public MemberRepository(GymDbContext context) : base(context) { }

    public async Task<Member?> GetByPhoneAsync(string phone)
        => await _dbSet.FirstOrDefaultAsync(m => m.Phone == phone);

    // Include بيحمل الاشتراكات، وThenInclude بيحمل الباقة جوه كل اشتراك
    public async Task<Member?> GetWithSubscriptionsAsync(int id)
        => await _dbSet
            .Include(m => m.Subscriptions)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<(IEnumerable<Member> Items, int TotalCount)> GetPagedAsync(
        string? search, int page, int pageSize)
    {
        // نبدأ بـ Query من غير تنفيذ
        var query = _dbSet.AsNoTracking();

        // لو في كلمة بحث نفلتر بالاسم أو التليفون
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(m => m.FullName.Contains(search) || m.Phone.Contains(search));
        }

        // نعد الإجمالي قبل الـ Pagination (عشان نحسب عدد الصفحات)
        var total = await query.CountAsync();

        // الترتيب لازم قبل Skip/Take. Skip بيتخطى صفحات قبلها، Take بياخد حجم الصفحة
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}