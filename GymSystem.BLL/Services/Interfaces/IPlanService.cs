using GymSystem.BLL.Common;
using GymSystem.BLL.ViewModels.Plans;

namespace GymSystem.BLL.Services.Interfaces;

public interface IPlanService
{
    Task<IReadOnlyList<PlanListItemViewModel>> GetAllAsync();
    Task<PlanViewModel> GetByIdAsync(int id);
    Task<int> CreateAsync(PlanViewModel model);
    Task UpdateAsync(PlanViewModel model);
    Task DeleteAsync(int id);

    // تفعيل/إيقاف الباقة
    Task ToggleActiveAsync(int id);

    // الباقات المتاحة للبيع للـ Dropdown
    Task<IReadOnlyList<LookupItem>> GetActiveLookupAsync();
}