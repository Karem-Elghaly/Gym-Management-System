using AutoMapper;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Attendance;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class AttendanceService : IAttendanceService
{
    // لو العضو نسي يسجل خروج وعدّى أكتر من الساعات دي، نعتبر الدخول القديم اتقفل
    private static readonly TimeSpan StaleCheckInAfter = TimeSpan.FromHours(12);

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(IUnitOfWork uow, IMapper mapper, ILogger<AttendanceService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AttendanceListItemViewModel>> GetByDateAsync(DateOnly date)
    {
        var list = await _uow.Attendances.GetByDateAsync(date);
        return _mapper.Map<List<AttendanceListItemViewModel>>(list);
    }

    public async Task<AttendanceListItemViewModel> CheckInAsync(CheckInViewModel model)
    {
        // بنطبع التليفون من المسافات، عشان الموظف ممكن يلزقه بمسافات
        var phone = model.Phone.Trim();

        var member = await _uow.Members.GetByPhoneAsync(phone)
            ?? throw new BusinessRuleException("No member found with this phone number.");

        // لازم اشتراك شغال
        var sub = await _uow.Subscriptions.GetActiveByMemberAsync(member.Id);
        if (sub is null)
            throw new BusinessRuleException($"{member.FullName} has no active subscription.");

        // لو عنده دخول مفتوح (لسه جوه)
        var open = await _uow.Attendances.GetOpenCheckInAsync(member.Id);
        if (open is not null)
        {
            if (DateTime.UtcNow - open.CheckInTime < StaleCheckInAfter)
                throw new BusinessRuleException($"{member.FullName} is already checked in.");

            // دخول قديم نسي يقفله: بنقفله بنفس وقت الدخول (مدة صفر) ونسجل تحذير
            // الأفضل ما نخترعش مدة، وفي نفس الوقت منسيبش العضو محبوس برا
            open.CheckOutTime = open.CheckInTime;
            _logger.LogWarning("Stale check-in {AttendanceId} auto-closed for member {MemberId}", open.Id, member.Id);
        }

        var attendance = new Attendance { MemberId = member.Id };
        await _uow.Attendances.AddAsync(attendance);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Member {MemberId} checked in", member.Id);

        // بنبني الرد بإيدينا لأننا عارفين الاسم من غير Query جديد
        return new AttendanceListItemViewModel
        {
            Id = attendance.Id,
            MemberId = member.Id,
            MemberName = member.FullName,
            CheckInTime = attendance.CheckInTime
        };
    }

    public async Task CheckOutAsync(int attendanceId)
    {
        var attendance = await _uow.Attendances.GetByIdAsync(attendanceId)
            ?? throw new NotFoundException(nameof(Attendance), attendanceId);

        if (attendance.CheckOutTime is not null)
            throw new BusinessRuleException("This member is already checked out.");

        attendance.CheckOutTime = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Attendance {AttendanceId} checked out", attendanceId);
    }
}