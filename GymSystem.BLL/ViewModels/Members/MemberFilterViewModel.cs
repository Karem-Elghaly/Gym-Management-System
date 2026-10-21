using GymSystem.BLL.Common;

namespace GymSystem.BLL.ViewModels.Members;

public class MemberFilterViewModel : PaginationParams
{
    // كلمة البحث (اسم أو تليفون)
    public string? Search { get; set; }
}