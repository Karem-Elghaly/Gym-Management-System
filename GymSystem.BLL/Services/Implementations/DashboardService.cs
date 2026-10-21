using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Dashboard;
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.UnitOfWork;

namespace GymSystem.BLL.Services.Implementations;

public class DashboardService : IDashboardService
{
    // الاشتراك اللي هيخلص خلال الأيام دي بيعتبر "قرب ينتهي"
    private const int ExpiringWithinDays = 7;

    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardViewModel> GetAsync()
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // حدود النهاردة والشهر الحالي (UTC) لحساب الحضور والإيراد
        var dayStart = today.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        // الاستعلامات بتتنفذ ورا بعض (مش Task.WhenAll) لأن الـ DbContext
        // مش بيدعم أكتر من عملية في نفس الوقت، وده بيرمي InvalidOperationException
        var vm = new DashboardViewModel
        {
            TotalMembers = await _uow.Members.CountAsync(),

            ActiveSubscriptions = await _uow.Subscriptions.CountAsync(s =>
                s.Status == SubscriptionStatus.Active && s.EndDate >= today),

            TodayAttendance = await _uow.Attendances.CountAsync(a =>
                a.CheckInTime >= dayStart && a.CheckInTime < dayEnd),

            // مواعيد الحصص بتوقيت الجيم المحلي (زي ما صلحنا في الخطوة 0)
            UpcomingClassesCount = await _uow.GymClasses.CountAsync(c => c.StartTime > DateTime.Now),

            MonthRevenue = await _uow.Payments.GetTotalRevenueAsync(monthStart, nextMonthStart)
        };

        // الاشتراكات اللي قربت تنتهي
        var expiring = await _uow.Subscriptions.GetExpiringAsync(ExpiringWithinDays);

        vm.ExpiringSubscriptions = expiring
            // لو العضو اتمسح Soft بيرجع Member = null، فنتخطاه
            .Where(s => s.Member is not null)
            .Select(s => new ExpiringSubscriptionViewModel
            {
                SubscriptionId = s.Id,
                MemberId = s.MemberId,
                MemberName = s.Member.FullName,
                Phone = s.Member.Phone,
                PlanName = s.Plan.Name,
                EndDate = s.EndDate,
                // DayNumber: رقم اليوم من أول التقويم، فالفرق = الأيام المتبقية
                DaysLeft = s.EndDate.DayNumber - today.DayNumber
            })
            .ToList();

        vm.ExpiringSoonCount = vm.ExpiringSubscriptions.Count;
        return vm;
    }
}