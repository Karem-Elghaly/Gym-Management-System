using GymSystem.DAL.Common;

namespace GymSystem.DAL.Entities;

// الحصة (يوغا، زومبا...). سميناه GymClass مش Class لأن Class كلمة محجوزة في C#
public class GymClass : BaseEntity
{
    // اسم الحصة
    public string Name { get; set; } = string.Empty;

    // وصف الحصة، اختياري
    public string? Description { get; set; }

    // رقم المدرب
    public int TrainerId { get; set; }

    // المدرب نفسه (Navigation)
    public Trainer Trainer { get; set; } = null!;

    // موعد بداية الحصة (تاريخ + ساعة)
    public DateTime StartTime { get; set; }

    // موعد نهاية الحصة
    public DateTime EndTime { get; set; }

    // السعة القصوى: أقصى عدد أعضاء ممكن يحجزوا. هنقارنه بعدد الحجوزات قبل ما نقبل حجز جديد
    public int MaxCapacity { get; set; }

    // الحجوزات على الحصة دي
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}