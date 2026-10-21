using GymSystem.DAL.Common;

namespace GymSystem.DAL.Entities;

// الباقة (شهري، 3 شهور، سنوي). بترث من BaseEntity فبتاخد Id والـ Audit والـ Soft Delete
public class MembershipPlan : BaseEntity
{
    // اسم الباقة، زي "Monthly" أو "Gold"
    public string Name { get; set; } = string.Empty;

    // وصف اختياري للباقة، ? معناها ممكن يبقى null
    public string? Description { get; set; }

    // سعر الباقة. decimal هو النوع الصح للفلوس لأنه دقيق، عكس double اللي بيطلع فيه كسور غلط
    public decimal Price { get; set; }

    // مدة الباقة بالأيام (30 شهري، 90 ثلاث شهور، 365 سنوي)
    // هنستخدمها لحساب تاريخ انتهاء الاشتراك: تاريخ البداية + DurationInDays
    public int DurationInDays { get; set; }

    // هل الباقة متاحة للبيع؟ لو false منقدرش نشترك فيها جديد، لكن الاشتراكات القديمة تفضل زي ما هي
    public bool IsActive { get; set; } = true;

    // كل الاشتراكات اللي اتعملت على الباقة دي (علاقة واحد لكتير)
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}