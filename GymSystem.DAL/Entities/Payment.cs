using GymSystem.DAL.Common;
using GymSystem.DAL.Common.Enums;

namespace GymSystem.DAL.Entities;

// دفعة مالية مرتبطة باشتراك
public class Payment : BaseEntity
{
    // رقم الاشتراك اللي الدفعة دي تخصه
    public int SubscriptionId { get; set; }

    // الاشتراك نفسه (Navigation)
    public Subscription Subscription { get; set; } = null!;

    // المبلغ المدفوع
    public decimal Amount { get; set; }

    // طريقة الدفع (Cash / Card / Transfer)
    public PaymentMethod Method { get; set; }

    // تاريخ ووقت الدفع الفعلي
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    // ملاحظات اختيارية (مثلاً رقم العملية لو تحويل)
    public string? Notes { get; set; }
}