using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Subscriptions;

public class SubscriptionRenewViewModel
{
    // الاشتراك اللي بنجدده
    public int SubscriptionId { get; set; }

    // للعرض فقط في الفورم
    public string MemberName { get; set; } = string.Empty;
    public string CurrentPlanName { get; set; } = string.Empty;
    public DateOnly CurrentEndDate { get; set; }

    // الباقة الجديدة (ممكن العضو يغير باقته وقت التجديد)
    public int? PlanId { get; set; }

    public decimal InitialPayment { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}