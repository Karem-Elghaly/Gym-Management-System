using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Classes;

public class ClassDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxCapacity { get; set; }

    // الحاجزين
    public List<BookedMemberViewModel> Bookings { get; set; } = new();

    public int AvailableSeats => MaxCapacity - Bookings.Count(b => b.Status != BookingStatus.Cancelled);
}

// كلاس صغير بيمثل عضو حاجز في الحصة
public class BookedMemberViewModel
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public DateTime BookedAt { get; set; }
}