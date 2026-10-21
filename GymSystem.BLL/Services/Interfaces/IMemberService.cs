using GymSystem.BLL.Common;
using GymSystem.BLL.ViewModels.Members;

namespace GymSystem.BLL.Services.Interfaces;

public interface IMemberService
{
    Task<PagedResult<MemberListItemViewModel>> GetPagedAsync(MemberFilterViewModel filter);
    Task<MemberViewModel> GetForEditAsync(int id);
    Task<MemberDetailsViewModel> GetDetailsAsync(int id);
    Task<int> CreateAsync(MemberViewModel model);
    Task UpdateAsync(MemberViewModel model);
    Task DeleteAsync(int id);

    // قائمة الأعضاء للـ Dropdown
    Task<IReadOnlyList<LookupItem>> GetLookupAsync();
}