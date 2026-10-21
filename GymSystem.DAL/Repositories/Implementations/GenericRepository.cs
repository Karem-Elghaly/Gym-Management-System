using System.Linq.Expressions;
using GymSystem.DAL.Common;
using GymSystem.DAL.Data;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

// بينفذ العقد لأي Entity. protected عشان الـ Repositories الخاصة ترث وتستخدمهم
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    // الـ DbContext اللي بنتعامل بيه مع الداتابيز
    protected readonly GymDbContext _context;

    // DbSet<T> هو "الجدول" الخاص بالـ Entity دي
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(GymDbContext context)
    {
        _context = context;
        // Set<T>() بتجيب الجدول المناسب لـ T أوتوماتيك
        _dbSet = context.Set<T>();
    }

    // AsNoTracking: بنقول لـ EF "ماتتابعش التغييرات"، أسرع في القراءة فقط
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();

    // FindAsync بيدور بالـ Primary Key، والـ Query Filter بيطبق (المحذوفين مش بيرجعوا)
    // بنستخدم FirstOrDefaultAsync بدل FindAsync لأن FindAsync بتتجاهل الـ Query Filter لو السجل في الـ Cache
    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);

    // لو predicate فاضي نعد الكل، لو موجود نعد اللي بيحقق الشرط
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate is null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);

    // بنرجع IQueryable من غير تنفيذ، المستدعي يكمل عليه Where/Include/Skip/Take
    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    // Remove بيعلّم السجل Deleted، والـ DbContext بيحوله Soft Delete وقت الحفظ
    public void Delete(T entity) => _dbSet.Remove(entity);
}