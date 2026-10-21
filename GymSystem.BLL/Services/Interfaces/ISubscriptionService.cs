using GymSystem.BLL.Common;
using GymSystem.BLL.ViewModels.Subscriptions;
using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.Services.Interfaces;

public interface ISubscriptionService
{
    Task<PagedResult<SubscriptionListItemViewModel>> GetPagedAsync(SubscriptionStatus? status, PaginationParams paging);
    Task<SubscriptionListItemViewModel> GetDetailsAsync(int id);
    Task<int> CreateAsync(SubscriptionCreateViewModel model);

    // بيجهز بيانات فورم التجديد
    Task<SubscriptionRenewViewModel> GetRenewFormAsync(int id);
    Task<int> RenewAsync(SubscriptionRenewViewModel model);

    Task FreezeAsync(int id);
    Task UnfreezeAsync(int id);
    Task CancelAsync(int id);

    // بتحول الاشتراكات النشطة اللي تاريخها عدّى لـ Expired، وبترجع عددها
    Task<int> ExpireOverdueAsync();
}