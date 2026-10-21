// مكتبة Identity: فيها IdentityUser وهو كلاس المستخدم الجاهز
using Microsoft.AspNetCore.Identity;

namespace GymSystem.DAL.Entities;

// بنرث من IdentityUser عشان ناخد كل حاجة جاهزة: Id (نص)، UserName، Email، PasswordHash، PhoneNumber...
// وبنزود عليه اللي إحنا محتاجينه
public class ApplicationUser : IdentityUser
{
    // الاسم الكامل للموظف أو المدرب، عشان نعرضه في الـ Navbar بدل الإيميل
    // = string.Empty معناها القيمة الافتراضية نص فاضي مش null
    public string FullName { get; set; } = string.Empty;

    // هل الحساب شغال؟ لو false مش هيقدر يعمل Login حتى لو الباسورد صح
    public bool IsActive { get; set; } = true;

    // تاريخ إنشاء الحساب
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}