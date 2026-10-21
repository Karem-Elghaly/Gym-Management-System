using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Subscriptions;

public class SubscriptionListItemViewModel
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal PriceAtPurchase { get; set; }

    // المدفوع فعلاً والمتبقي (بيتحسبوا في الـ Service)
    public decimal TotalPaid { get; set; }
    public decimal Remaining => PriceAtPurchase - TotalPaid;
}