namespace GymSystem.BLL.ViewModels.Dashboard;

public class DashboardViewModel
{
    public int TotalMembers { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int TodayAttendance { get; set; }
    public int UpcomingClassesCount { get; set; }
    public decimal MonthRevenue { get; set; }

    // قائمة الاشتراكات اللي هتنتهي قريب
    public List<ExpiringSubscriptionViewModel> ExpiringSubscriptions { get; set; } = new();
}