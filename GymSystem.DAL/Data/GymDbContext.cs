// مكتبة Identity: فيها IdentityDbContext
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// مكتبة EF Core الأساسية
using Microsoft.EntityFrameworkCore;
using GymSystem.DAL.Common;
using GymSystem.DAL.Entities;
// عشان نقدر نستخدم Expression Trees في الفلتر العام
using System.Linq.Expressions;
using System.Reflection;

namespace GymSystem.DAL.Data;

// بنرث من IdentityDbContext<ApplicationUser> بدل DbContext العادي
// عشان نأخد جداول Identity (المستخدمين والـ Roles) جاهزة مع جداولنا في نفس الداتابيز
public class GymDbContext : IdentityDbContext<ApplicationUser>
{
    // الخدمة اللي بتقولنا مين المستخدم الحالي (لملء CreatedBy و UpdatedBy)
    private readonly ICurrentUserService? _currentUser;

    // Constructor: الـ options فيها الـ Connection String ونوع الداتابيز، بتيجي من الـ DI
    // ICurrentUserService اختياري (= null) عشان الـ Migrations تشتغل من غير HttpContext
    public GymDbContext(DbContextOptions<GymDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    // كل DbSet بيتحول لجدول في الداتابيز
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<GymClass> GymClasses => Set<GymClass>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Attendance> Attendances => Set<Attendance>();

    // الدالة دي EF Core بيناديها مرة واحدة وهو بيبني شكل الداتابيز
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // لازم ننادي الأصلية الأول عشان Identity يبني جداوله
        base.OnModelCreating(builder);

        // بتدور في المشروع على كل كلاس Configuration (IEntityTypeConfiguration) وتطبقه أوتوماتيك
        // فمش محتاجين نسجل كل Configuration بإيدينا
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // نلف على كل Entity في الموديل
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // لو الـ Entity بترث من BaseEntity (يعني عندها IsDeleted)
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // بنبني الشرط: e => e.IsDeleted == false  بشكل ديناميكي لكل Entity
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                // نركّب الفلتر: أي Query على الـ Entity دي هتستبعد المحذوفين أوتوماتيك
                entityType.SetQueryFilter(lambda);
            }
        }
    }

    // SaveChangesAsync بنعدّلها عشان نملا الـ Audit قبل الحفظ
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    // نسخة الـ Sync كمان، عشان لو حد ناداها بالغلط ماتتخطاش الـ Audit
    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    // الدالة اللي بتملا الحقول أوتوماتيك
    private void ApplyAuditAndSoftDelete()
    {
        // الوقت والمستخدم الحاليين
        var now = DateTime.UtcNow;
        var user = _currentUser?.UserName;

        // ChangeTracker بيتابع كل الـ Entities اللي اتغيرت من آخر حفظ
        // بنفلتر اللي بترث من BaseEntity بس
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            // نشوف حالة الـ Entity: Added / Modified / Deleted
            switch (entry.State)
            {
                // سجل جديد: نملا بيانات الإنشاء
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    break;

                // سجل اتعدل: نملا بيانات التعديل
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    // نمنع تعديل تاريخ الإنشاء بالغلط
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;

                // حد ناده Remove: بدل المسح الفعلي بنحوله Soft Delete
                case EntityState.Deleted:
                    // نلغي المسح الفعلي ونرجعه Modified
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = user;
                    break;
            }
        }
    }
}