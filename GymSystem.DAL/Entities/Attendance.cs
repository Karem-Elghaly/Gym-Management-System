using GymSystem.DAL.Common;

namespace GymSystem.DAL.Entities;

// تسجيل دخول عضو للجيم
public class Attendance : BaseEntity
{
    // رقم العضو
    public int MemberId { get; set; }

    // العضو نفسه
    public Member Member { get; set; } = null!;

    // وقت الدخول
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    // وقت الخروج. nullable لأن العضو ممكن يكون لسه جوه الجيم ومخرجش
    public DateTime? CheckOutTime { get; set; }
}