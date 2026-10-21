namespace GymSystem.BLL.ViewModels.Dashboard;

public class ExpiringSubscriptionViewModel
{
    public int SubscriptionId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateOnly EndDate { get; set; }

    // الأيام المتبقية على الانتهاء
    public int DaysLeft { get; set; }
}