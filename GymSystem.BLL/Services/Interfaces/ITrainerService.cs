using GymSystem.BLL.Common;
using GymSystem.BLL.ViewModels.Trainers;

namespace GymSystem.BLL.Services.Interfaces;

public interface ITrainerService
{
    Task<IReadOnlyList<TrainerListItemViewModel>> GetAllAsync();
    Task<TrainerViewModel> GetByIdAsync(int id);
    Task<int> CreateAsync(TrainerViewModel model);
    Task UpdateAsync(TrainerViewModel model);
    Task DeleteAsync(int id);
    Task<IReadOnlyList<LookupItem>> GetLookupAsync();
}