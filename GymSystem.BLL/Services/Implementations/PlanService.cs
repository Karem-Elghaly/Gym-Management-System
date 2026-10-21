using AutoMapper;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Plans;
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class PlanService : IPlanService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PlanService> _logger;

    public PlanService(IUnitOfWork uow, IMapper mapper, ILogger<PlanService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlanListItemViewModel>> GetAllAsync()
    {
        var plans = await _uow.Plans.GetAllAsync();
        // الأرخص مدة الأول
        return _mapper.Map<List<PlanListItemViewModel>>(plans.OrderBy(p => p.DurationInDays));
    }

    public async Task<PlanViewModel> GetByIdAsync(int id)
    {
        var plan = await _uow.Plans.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(MembershipPlan), id);
        return _mapper.Map<PlanViewModel>(plan);
    }

    public async Task<int> CreateAsync(PlanViewModel model)
    {
        // اسم الباقة ما يتكررش
        if (await _uow.Plans.NameExistsAsync(model.Name))
            throw new BusinessRuleException("A plan with this name already exists.");

        var plan = _mapper.Map<MembershipPlan>(model);
        await _uow.Plans.AddAsync(plan);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Plan {PlanId} created", plan.Id);
        return plan.Id;
    }

    public async Task UpdateAsync(PlanViewModel model)
    {
        var plan = await _uow.Plans.GetByIdAsync(model.Id)
            ?? throw new NotFoundException(nameof(MembershipPlan), model.Id);

        // نستثني الباقة نفسها من فحص التكرار
        if (await _uow.Plans.NameExistsAsync(model.Name, model.Id))
            throw new BusinessRuleException("A plan with this name already exists.");

        // تعديل السعر هنا مش بيأثر على الاشتراكات القديمة لأنها محتفظة بـ PriceAtPurchase
        _mapper.Map(model, plan);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Plan {PlanId} updated", plan.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var plan = await _uow.Plans.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(MembershipPlan), id);

        // ممنوع نمسح باقة عليها اشتراكات شغالة أو متجمدة
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var inUse = await _uow.Subscriptions.AnyAsync(s =>
            s.PlanId == id
            && ((s.Status == SubscriptionStatus.Active && s.EndDate >= today)
                || s.Status == SubscriptionStatus.Frozen));

        if (inUse)
            throw new BusinessRuleException("Cannot delete a plan that has active subscriptions. Deactivate it instead.");

        _uow.Plans.Delete(plan);
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Plan {PlanId} deleted (soft)", id);
    }

    public async Task ToggleActiveAsync(int id)
    {
        var plan = await _uow.Plans.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(MembershipPlan), id);

        // عكس القيمة: true تبقى false والعكس
        plan.IsActive = !plan.IsActive;
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Plan {PlanId} active state is now {IsActive}", id, plan.IsActive);
    }

    public async Task<IReadOnlyList<LookupItem>> GetActiveLookupAsync()
    {
        var plans = await _uow.Plans.GetActivePlansAsync();
        // السعر والمدة في النص عشان الموظف يشوفهم وهو بيختار
        return plans.Select(p => new LookupItem(p.Id, $"{p.Name} - {p.Price:0.##} EGP ({p.DurationInDays} days)")).ToList();
    }
}