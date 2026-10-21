using GymSystem.DAL.Repositories.Interfaces;

namespace GymSystem.DAL.UnitOfWork;

// IDisposable عشان نقدر نقفل الـ DbContext لما نخلص
public interface IUnitOfWork : IDisposable
{
    // Repository لكل جدول، الـ Service بتوصلهم كلهم من مكان واحد
    IMemberRepository Members { get; }
    IPlanRepository Plans { get; }
    ISubscriptionRepository Subscriptions { get; }
    IPaymentRepository Payments { get; }
    ITrainerRepository Trainers { get; }
    IGymClassRepository GymClasses { get; }
    IBookingRepository Bookings { get; }
    IAttendanceRepository Attendances { get; }

    // بتحفظ كل التغييرات مرة واحدة، وبترجع عدد السجلات المتأثرة
    Task<int> SaveChangesAsync();

    // Transactions: لو عملية بتعدّل أكتر من جدول وعايزين الكل ينجح أو الكل يفشل
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}