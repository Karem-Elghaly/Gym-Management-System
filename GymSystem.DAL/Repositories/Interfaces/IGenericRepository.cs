using System.Linq.Expressions;
using GymSystem.DAL.Common;

namespace GymSystem.DAL.Repositories.Interfaces;

// where T : BaseEntity معناها: T لازم يكون كلاس بيرث من BaseEntity
// عشان نضمن إن أي Entity هنتعامل معاها فيها Id والـ Soft Delete
public interface IGenericRepository<T> where T : BaseEntity
{
    // بترجع كل السجلات (غير المحذوفة). AsNoTracking جوه التنفيذ للقراءة بس
    Task<IEnumerable<T>> GetAllAsync();

    // بترجع سجل واحد بالـ Id، أو null لو مش موجود
    Task<T?> GetByIdAsync(int id);

    // بترجع سجلات بشرط. Expression<Func<T,bool>> هو شرط EF بيحوله لـ SQL (Where)
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    // بترجع أول سجل بيحقق الشرط، أو null
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    // بترجع true لو في سجل واحد على الأقل بيحقق الشرط
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    // بترجع عدد السجلات (بشرط اختياري)
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // بترجع IQueryable عشان الـ Repositories الخاصة والـ Services تبني Query (Pagination مثلاً)
    // قبل ما يتنفذ على الداتابيز
    IQueryable<T> Query();

    // إضافة سجل جديد (مش بيحفظ، بيحفظ UnitOfWork)
    Task AddAsync(T entity);

    // تعديل سجل
    void Update(T entity);

    // مسح (Soft Delete بيتم في DbContext تلقائي)
    void Delete(T entity);
}