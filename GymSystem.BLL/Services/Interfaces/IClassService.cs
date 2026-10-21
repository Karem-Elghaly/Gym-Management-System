using GymSystem.BLL.ViewModels.Classes;

namespace GymSystem.BLL.Services.Interfaces;

public interface IClassService
{
    Task<IReadOnlyList<ClassListItemViewModel>> GetAllAsync();
    Task<IReadOnlyList<ClassListItemViewModel>> GetUpcomingAsync();
    Task<ClassViewModel> GetForEditAsync(int id);
    Task<ClassDetailsViewModel> GetDetailsAsync(int id);
    Task<int> CreateAsync(ClassViewModel model);
    Task UpdateAsync(ClassViewModel model);
    Task DeleteAsync(int id);
    Task BookAsync(BookingViewModel model);
    Task CancelBookingAsync(int bookingId);
}