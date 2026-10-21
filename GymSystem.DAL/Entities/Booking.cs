using GymSystem.DAL.Common;
using GymSystem.DAL.Common.Enums;

namespace GymSystem.DAL.Entities;

// حجز عضو في حصة. جدول وسيط بين Member و GymClass
public class Booking : BaseEntity
{
    // رقم العضو الحاجز
    public int MemberId { get; set; }

    // العضو نفسه
    public Member Member { get; set; } = null!;

    // رقم الحصة المحجوزة
    public int GymClassId { get; set; }

    // الحصة نفسها
    public GymClass GymClass { get; set; } = null!;

    // حالة الحجز (Booked / Cancelled / Attended)
    public BookingStatus Status { get; set; } = BookingStatus.Booked;

    // وقت ما العضو عمل الحجز
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}