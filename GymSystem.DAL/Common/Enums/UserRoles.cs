namespace GymSystem.DAL.Common.Enums;

// static class معناه إن مفيش حد هيعمل منه Object، هو مجرد حاوية للثوابت
// استخدمناه بدل enum لأن ASP.NET Identity بيتعامل مع الـ Roles كـ نصوص (string) مش أرقام
public static class UserRoles
{
    // const معناها قيمة ثابتة وقت الكتابة ومستحيل تتغير وقت التشغيل
    // ده اسم الـ Role زي ما هيتخزن في جدول AspNetRoles
    public const string Admin = "Admin";

    // موظف الاستقبال: بيسجل الأعضاء والاشتراكات والمدفوعات
    public const string Receptionist = "Receptionist";

    // المدرب: بيشوف حصصه وحاجزينها
    public const string Trainer = "Trainer";
}