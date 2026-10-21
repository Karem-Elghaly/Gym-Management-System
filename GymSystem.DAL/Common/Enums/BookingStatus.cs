namespace GymSystem.DAL.Common.Enums;

// حالات حجز العضو في حصة
public enum BookingStatus
{
    // العضو حجز مكانه ولسه الحصة ماحصلتش
    Booked = 1,

    // الحجز اتلغى (من العضو أو الإدارة)، والمكان بيرجع متاح
    Cancelled = 2,

    // العضو حضر الحصة فعلاً
    Attended = 3
}