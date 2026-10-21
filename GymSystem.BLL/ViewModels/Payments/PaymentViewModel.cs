using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Payments;

public class PaymentViewModel
{
    public int? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string? Notes { get; set; }
}