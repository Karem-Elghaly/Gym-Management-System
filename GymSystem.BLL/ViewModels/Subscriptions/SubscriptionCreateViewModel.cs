using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Subscriptions;

public class SubscriptionCreateViewModel
{
    // int? عشان الـ dropdown يبدأ فاضي والـ Validator يتأكد إنه اتختار
    public int? MemberId { get; set; }
    public int? PlanId { get; set; }

    // تاريخ بداية الاشتراك، افتراضياً النهاردة
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    // الدفعة الأولى اختيارية: ممكن العضو يدفع جزء أو الكل وقت الاشتراك
    public decimal InitialPayment { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}