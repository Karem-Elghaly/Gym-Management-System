using AutoMapper;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Classes;
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class ClassService : IClassService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ClassService> _logger;

    public ClassService(IUnitOfWork uow, IMapper mapper, ILogger<ClassService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClassListItemViewModel>> GetAllAsync()
    {
        var classes = await _uow.GymClasses.GetAllWithTrainerAsync();
        return _mapper.Map<List<ClassListItemViewModel>>(classes);
    }

    public async Task<IReadOnlyList<ClassListItemViewModel>> GetUpcomingAsync()
    {
        var classes = await _uow.GymClasses.GetUpcomingAsync();
        return _mapper.Map<List<ClassListItemViewModel>>(classes);
    }

    public async Task<ClassViewModel> GetForEditAsync(int id)
    {
        var cls = await _uow.GymClasses.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(GymClass), id);
        return _mapper.Map<ClassViewModel>(cls);
    }

    public async Task<ClassDetailsViewModel> GetDetailsAsync(int id)
    {
        var cls = await _uow.GymClasses.GetWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(GymClass), id);

        var vm = _mapper.Map<ClassDetailsViewModel>(cls);

        // الحجوزات الملغية بتظهر في الآخر، والباقي بترتيب وقت الحجز
        vm.Bookings = vm.Bookings
            .OrderBy(b => b.Status == BookingStatus.Cancelled)
            .ThenBy(b => b.BookedAt)
            .ToList();
        return vm;
    }

    public async Task<int> CreateAsync(ClassViewModel model)
    {
        await EnsureTrainerAvailableAsync(model);

        var cls = _mapper.Map<GymClass>(model);
        await _uow.GymClasses.AddAsync(cls);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Class {ClassId} created", cls.Id);
        return cls.Id;
    }

    public async Task UpdateAsync(ClassViewModel model)
    {
        var cls = await _uow.GymClasses.GetByIdAsync(model.Id)
            ?? throw new NotFoundException(nameof(GymClass), model.Id);

        // السعة الجديدة ماينفعش تبقى أقل من اللي حاجزين فعلاً
        var booked = await _uow.Bookings.CountActiveByClassAsync(model.Id);
        if (model.MaxCapacity < booked)
            throw new BusinessRuleException($"Capacity cannot be less than the current bookings ({booked}).");

        await EnsureTrainerAvailableAsync(model);

        _mapper.Map(model, cls);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Class {ClassId} updated", cls.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var cls = await _uow.GymClasses.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(GymClass), id);

        // حصة لسه جاية وفيها حجوزات: ممنوع نمسحها من تحت الأعضاء
        if (cls.StartTime > DateTime.Now && await _uow.Bookings.CountActiveByClassAsync(id) > 0)
            throw new BusinessRuleException("Cannot delete an upcoming class that has bookings.");

        _uow.GymClasses.Delete(cls);
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Class {ClassId} deleted (soft)", id);
    }

    public async Task BookAsync(BookingViewModel model)
    {
        var cls = await _uow.GymClasses.GetByIdAsync(model.GymClassId)
            ?? throw new NotFoundException(nameof(GymClass), model.GymClassId);

        // !.Value آمنة لأن الـ Validator/الـ Controller اتأكد إن العضو اتحدد
        var member = await _uow.Members.GetByIdAsync(model.MemberId!.Value)
            ?? throw new NotFoundException(nameof(Member), model.MemberId);

        // 1) الحصة لازم لسه ماخدتش بدايتها
        if (cls.StartTime <= DateTime.Now)
            throw new BusinessRuleException("This class has already started.");

        // 2) اشتراك شغال، وبيغطي يوم الحصة (مش بس النهاردة)
        var sub = await _uow.Subscriptions.GetActiveByMemberAsync(member.Id);
        if (sub is null)
            throw new BusinessRuleException("This member has no active subscription.");

        if (sub.EndDate < DateOnly.FromDateTime(cls.StartTime))
            throw new BusinessRuleException("The member's subscription ends before this class.");

        // 3) الحجز المكرر
        var existing = await _uow.Bookings.GetByMemberAndClassAsync(member.Id, cls.Id);
        if (existing is not null && existing.Status != BookingStatus.Cancelled)
            throw new BusinessRuleException("This member is already booked in this class.");

        // 4) السعة
        var booked = await _uow.Bookings.CountActiveByClassAsync(cls.Id);
        if (booked >= cls.MaxCapacity)
            throw new BusinessRuleException("This class is full.");

        if (existing is not null)
        {
            // كان لاغي وبيحجز تاني: بنرجّع نفس السجل (الـ Unique Index مش هيسمح بسجل تاني)
            existing.Status = BookingStatus.Booked;
            existing.BookedAt = DateTime.UtcNow;
        }
        else
        {
            await _uow.Bookings.AddAsync(new Booking { MemberId = member.Id, GymClassId = cls.Id });
        }

        await _uow.SaveChangesAsync();
        _logger.LogInformation("Member {MemberId} booked class {ClassId}", member.Id, cls.Id);
    }

    public async Task CancelBookingAsync(int bookingId)
    {
        // محتاجين الحصة عشان نعرف ميعادها، فبنحملها مع الحجز
        var booking = await _uow.Bookings.Query()
            .Include(b => b.GymClass)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);

        if (booking.Status != BookingStatus.Booked)
            throw new BusinessRuleException("Only active bookings can be cancelled.");

        if (booking.GymClass.StartTime <= DateTime.Now)
            throw new BusinessRuleException("Cannot cancel a booking after the class has started.");

        // Soft: بنغير الحالة بس، والمكان بيرجع متاح لأن العد بيستبعد الملغي
        booking.Status = BookingStatus.Cancelled;
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} cancelled", bookingId);
    }

    // ============ دوال مساعدة ============

    // بتتأكد إن المدرب موجود ومش عنده حصة متداخلة مع الميعاد ده
    private async Task EnsureTrainerAvailableAsync(ClassViewModel model)
    {
        var trainerId = model.TrainerId!.Value;
        var start = model.StartTime!.Value;
        var end = model.EndTime!.Value;

        if (!await _uow.Trainers.AnyAsync(t => t.Id == trainerId))
            throw new NotFoundException(nameof(Trainer), trainerId);

        // تداخل فترتين: بداية الأولى قبل نهاية التانية، ونهاية الأولى بعد بداية التانية
        // model.Id != c.Id عشان وقت التعديل ما نقارنش الحصة بنفسها
        var overlaps = await _uow.GymClasses.AnyAsync(c =>
            c.TrainerId == trainerId
            && c.Id != model.Id
            && c.StartTime < end
            && c.EndTime > start);

        if (overlaps)
            throw new BusinessRuleException("The trainer already has a class during this time.");
    }
}