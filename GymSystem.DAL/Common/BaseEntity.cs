namespace GymSystem.DAL.Common;

// abstract معناها إن الكلاس ده مينفعش تعمل منه Object مباشرة (new BaseEntity() هيدي خطأ)
// هو موجود بس عشان الكلاسات التانية ترث منه وتاخد الخصائص دي
public abstract class BaseEntity
{
    // المفتاح الأساسي (Primary Key) للجدول
    // EF Core بيفهم لوحده إن اسم Id هو الـ Primary Key، وبيخليه Identity (يزيد أوتوماتيك 1، 2، 3...)
    public int Id { get; set; }

    // ===================== Audit Fields =====================
    // دي الحقول اللي بتسجل مين عمل إيه ومتى، ومهمة جداً في أي نظام حقيقي

    // تاريخ ووقت إنشاء السجل
    // DateTime.UtcNow بياخد الوقت بتوقيت UTC العالمي، وده أفضل من الوقت المحلي
    // لأنه مش بيتأثر بتغيير الساعة الصيفي ولا بمكان السيرفر
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // تاريخ آخر تعديل، علامة ? معناها nullable يعني ممكن تبقى فاضية
    // بتفضل null لحد ما السجل يتعدل أول مرة
    public DateTime? UpdatedAt { get; set; }

    // اسم المستخدم اللي أنشأ السجل (هنملاه أوتوماتيك من الـ DbContext)
    // string? يعني نص ممكن يبقى null، مثلاً لو السجل اتعمل من الـ Seeder مش من مستخدم
    public string? CreatedBy { get; set; }

    // اسم المستخدم اللي عدّل السجل آخر مرة
    public string? UpdatedBy { get; set; }

    // ===================== Soft Delete =====================
    // الفكرة: بدل ما نمسح السجل فعلياً من الداتابيز، بنعلّم عليه إنه اتمسح
    // الفايدة: نقدر نسترجعه، والتقارير والمدفوعات القديمة متتبوظش

    // true يعني السجل اتمسح (منطقياً)، والقيمة الافتراضية false يعني سجل شغال
    public bool IsDeleted { get; set; }

    // تاريخ ووقت المسح، بيبقى null لو السجل لسه شغال
    public DateTime? DeletedAt { get; set; }

    // اسم اللي مسح السجل
    public string? DeletedBy { get; set; }
}