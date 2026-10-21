using GymSystem.BLL.Common;
using GymSystem.BLL.ViewModels.Payments;

namespace GymSystem.BLL.Services.Interfaces;

public interface IPaymentService
{
    Task<PagedResult<PaymentListItemViewModel>> GetPagedAsync(PaginationParams paging);
    Task<int> CreateAsync(PaymentViewModel model);

    // الاشتراكات اللي لسه عليها مبلغ متبقي (للـ Dropdown في فورم الدفع)
    Task<IReadOnlyList<LookupItem>> GetPayableSubscriptionsAsync();
}