using GymSystem.BLL.ViewModels.Dashboard;

namespace GymSystem.BLL.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetAsync();
}