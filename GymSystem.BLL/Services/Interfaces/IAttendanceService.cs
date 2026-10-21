using GymSystem.BLL.ViewModels.Attendance;

namespace GymSystem.BLL.Services.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceListItemViewModel>> GetByDateAsync(DateOnly date);
    Task<AttendanceListItemViewModel> CheckInAsync(CheckInViewModel model);
    Task CheckOutAsync(int attendanceId);
}