using GymSystem.DAL.Data;
using GymSystem.DAL.Repositories.Implementations;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymSystem.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly GymDbContext _context;

    // الـ Transaction الحالية لو فيه واحدة شغالة
    private IDbContextTransaction? _transaction;

    // بنعمل الـ Repositories بشكل Lazy: ما بتتعملش غير لما حد يطلبها
    // وكلهم بيشاركوا نفس الـ DbContext، فـ Save واحد بيحفظ تغييراتهم كلها
    private IMemberRepository? _members;
    private IPlanRepository? _plans;
    private ISubscriptionRepository? _subscriptions;
    private IPaymentRepository? _payments;
    private ITrainerRepository? _trainers;
    private IGymClassRepository? _gymClasses;
    private IBookingRepository? _bookings;
    private IAttendanceRepository? _attendances;

    public UnitOfWork(GymDbContext context) => _context = context;

    // ??= معناها: لو المتغير null اعمله new وخزنه، ولو موجود رجّعه زي ما هو
    public IMemberRepository Members => _members ??= new MemberRepository(_context);
    public IPlanRepository Plans => _plans ??= new PlanRepository(_context);
    public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public ITrainerRepository Trainers => _trainers ??= new TrainerRepository(_context);
    public IGymClassRepository GymClasses => _gymClasses ??= new GymClassRepository(_context);
    public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
    public IAttendanceRepository Attendances => _attendances ??= new AttendanceRepository(_context);

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    // نحفظ التغييرات ونثبّت الـ Transaction
    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // نرجع في كل التغييرات
    public async Task RollbackAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}