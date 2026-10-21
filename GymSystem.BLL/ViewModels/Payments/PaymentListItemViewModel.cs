using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Payments;

public class PaymentListItemViewModel
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; }
    public string? Notes { get; set; }
}