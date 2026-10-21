namespace GymSystem.BLL.ViewModels.Attendance;

public class AttendanceListItemViewModel
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }

    // لو لسه ما خرجش يبقى لسه جوه
    public bool IsInside => CheckOutTime is null;
}