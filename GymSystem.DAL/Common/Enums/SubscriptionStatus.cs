namespace GymSystem.DAL.Common.Enums;

// الحالات اللي ممكن الاشتراك يكون فيها
public enum SubscriptionStatus
{
    // اشتراك شغال وتاريخ انتهاءه لسه ماجاش
    Active = 1,

    // تاريخ الانتهاء عدّى
    Expired = 2,

    // الاشتراك متجمد مؤقتاً (العضو مسافر مثلاً)، وبيرجع يشتغل بعدين
    Frozen = 3,

    // الاشتراك اتلغى بقرار من الإدارة أو العضو
    Cancelled = 4
}