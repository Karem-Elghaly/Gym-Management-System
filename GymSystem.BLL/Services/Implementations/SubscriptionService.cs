using AutoMapper;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Subscriptions;
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(IUnitOfWork uow, IMapper mapper, ILogger<SubscriptionService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    // تاريخ النهاردة بتوقيت UTC (نفس التوقيت اللي بنخزن بيه)
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task<PagedResult<SubscriptionListItemViewModel>> GetPagedAsync(
        SubscriptionStatus? status, PaginationParams paging)
    {
        var (subs, total) = await _uow.Subscriptions.GetPagedAsync(status, paging.Page, paging.PageSize);

        var items = _mapper.Map<List<SubscriptionListItemViewModel>>(subs);

        // المدفوع لكل اشتراك في Query واحد
        var paid = await _uow.Payments.GetPaidTotalsAsync(items.Select(i => i.Id));
        foreach (var item in items)
            item.TotalPaid = paid.GetValueOrDefault(item.Id);

        return new PagedResult<SubscriptionListItemViewModel>(items, total, paging.Page, paging.PageSize);
    }

    public async Task<SubscriptionListItemViewModel> GetDetailsAsync(int id)
    {
        // GetWithDetailsAsync بتجيب العضو والباقة والمدفوعات مع بعض
        var sub = await _uow.Subscriptions.GetWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Subscription), id);

        var vm = _mapper.Map<SubscriptionListItemViewModel>(sub);
        vm.TotalPaid = sub.Payments.Sum(p => p.Amount);
        return vm;
    }

    public async Task<int> CreateAsync(SubscriptionCreateViewModel model)
    {
        // !.Value آمنة لأن الـ Validator اتأكد إن القيمتين مش null
        var member = await _uow.Members.GetByIdAsync(model.MemberId!.Value)
            ?? throw new NotFoundException(nameof(Member), model.MemberId);

        var plan = await _uow.Plans.GetByIdAsync(model.PlanId!.Value)
            ?? throw new NotFoundException(nameof(MembershipPlan), model.PlanId);

        if (!plan.IsActive)
            throw new BusinessRuleException("This plan is not available for new subscriptions.");

        // القاعدة الأهم: العضو مينفعش يبقى عنده اشتراكين مفتوحين
        // مفتوح = Active ولسه ماخلصش، أو Frozen
        var today = Today;
        var hasOpenSub = await _uow.Subscriptions.AnyAsync(s =>
            s.MemberId == member.Id
            && ((s.Status == SubscriptionStatus.Active && s.EndDate >= today)
                || s.Status == SubscriptionStatus.Frozen));

        if (hasOpenSub)
            throw new BusinessRuleException(
                "This member already has an active or frozen subscription. Use renew instead.");

        var subscription = BuildSubscription(member.Id, plan, model.StartDate,
            model.InitialPayment, model.PaymentMethod);

        await _uow.Subscriptions.AddAsync(subscription);

        // SaveChanges واحدة بتحفظ الاشتراك والدفعة مع بعض، وEF بيلفها في Transaction لوحده
        // يعني يا الاتنين يتحفظوا يا ولا واحد
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Subscription {SubscriptionId} created for member {MemberId} on plan {PlanId}",
            subscription.Id, member.Id, plan.Id);
        return subscription.Id;
    }

    public async Task<SubscriptionRenewViewModel> GetRenewFormAsync(int id)
    {
        var sub = await _uow.Subscriptions.GetWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Subscription), id);

        return new SubscriptionRenewViewModel
        {
            SubscriptionId = sub.Id,
            MemberName = sub.Member.FullName,
            CurrentPlanName = sub.Plan.Name,
            CurrentEndDate = sub.EndDate,
            // الباقة الحالية مختارة افتراضياً، والموظف يغيرها لو العضو عايز
            PlanId = sub.PlanId
        };
    }

    public async Task<int> RenewAsync(SubscriptionRenewViewModel model)
    {
        var current = await _uow.Subscriptions.GetWithDetailsAsync(model.SubscriptionId)
            ?? throw new NotFoundException(nameof(Subscription), model.SubscriptionId);

        // is بتفحص أكتر من قيمة مرة واحدة (C# 9 pattern)
        if (current.Status is SubscriptionStatus.Frozen or SubscriptionStatus.Cancelled)
            throw new BusinessRuleException("Frozen or cancelled subscriptions cannot be renewed.");

        // منع التجديد المكرر: لو في اشتراك نشط بيبدأ بعد نهاية ده يبقى اتجدد قبل كده
        var alreadyRenewed = await _uow.Subscriptions.AnyAsync(s =>
            s.MemberId == current.MemberId
            && s.Id != current.Id
            && s.Status == SubscriptionStatus.Active
            && s.StartDate > current.EndDate);

        if (alreadyRenewed)
            throw new BusinessRuleException("This subscription has already been renewed.");

        // لو الباقة الجديدة اتحددت نستخدمها، وإلا نفس الباقة القديمة
        var planId = model.PlanId ?? current.PlanId;
        var plan = await _uow.Plans.GetByIdAsync(planId)
            ?? throw new NotFoundException(nameof(MembershipPlan), planId);

        if (!plan.IsActive)
            throw new BusinessRuleException("This plan is not available for new subscriptions.");

        // لو الاشتراك الحالي لسه شغال، الجديد يبدأ تاني يوم من نهايته (العضو مبيخسرش أيام)
        // لو خلص خلاص، يبدأ من النهاردة
        var today = Today;
        var start = current.EndDate >= today ? current.EndDate.AddDays(1) : today;

        // بنعمل سجل اشتراك جديد بدل ما نعدل القديم، عشان تاريخ العضو والأسعار يفضلوا محفوظين
        var renewed = BuildSubscription(current.MemberId, plan, start,
            model.InitialPayment, model.PaymentMethod);

        await _uow.Subscriptions.AddAsync(renewed);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Subscription {OldId} renewed as {NewId}", current.Id, renewed.Id);
        return renewed.Id;
    }

    public async Task FreezeAsync(int id)
    {
        var sub = await GetOrThrowAsync(id);

        if (sub.Status != SubscriptionStatus.Active)
            throw new BusinessRuleException("Only active subscriptions can be frozen.");

        // بنسجل تاريخ بداية التجميد عشان نحسب الأيام وقت الفك
        sub.Status = SubscriptionStatus.Frozen;
        sub.FrozenAt = Today;
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Subscription {SubscriptionId} frozen", id);
    }

    public async Task UnfreezeAsync(int id)
    {
        var sub = await GetOrThrowAsync(id);

        if (sub.Status != SubscriptionStatus.Frozen || sub.FrozenAt is null)
            throw new BusinessRuleException("Only frozen subscriptions can be unfrozen.");

        // DayNumber رقم اليوم من أول التقويم، الفرق بينهم = عدد أيام التجميد
        var frozenDays = Today.DayNumber - sub.FrozenAt.Value.DayNumber;

        // نمدد الاشتراك بنفس عدد الأيام عشان العضو ميخسرش فلوسه
        sub.EndDate = sub.EndDate.AddDays(frozenDays);
        sub.Status = SubscriptionStatus.Active;
        sub.FrozenAt = null;
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Subscription {SubscriptionId} unfrozen, extended by {Days} days", id, frozenDays);
    }

    public async Task CancelAsync(int id)
    {
        var sub = await GetOrThrowAsync(id);

        if (sub.Status == SubscriptionStatus.Cancelled)
            throw new BusinessRuleException("Subscription is already cancelled.");

        sub.Status = SubscriptionStatus.Cancelled;
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Subscription {SubscriptionId} cancelled", id);
    }

    public async Task<int> ExpireOverdueAsync()
    {
        // الـ Repository بيجيب النشطة اللي تاريخها عدّى (Tracked عشان نعدلها)
        var overdue = (await _uow.Subscriptions.GetOverdueActiveAsync()).ToList();

        foreach (var sub in overdue)
            sub.Status = SubscriptionStatus.Expired;

        if (overdue.Count > 0)
        {
            await _uow.SaveChangesAsync();
            _logger.LogInformation("{Count} subscriptions marked as expired", overdue.Count);
        }

        return overdue.Count;
    }

    // ============ دوال مساعدة (Private) ============

    // بتجيب الاشتراك أو ترمي NotFound، عشان منكررش نفس السطرين
    private async Task<Subscription> GetOrThrowAsync(int id)
        => await _uow.Subscriptions.GetByIdAsync(id)
           ?? throw new NotFoundException(nameof(Subscription), id);

    // بتبني الاشتراك + الدفعة الأولى، ومستخدمة في الإنشاء والتجديد (DRY: ما نكررش الكود)
    private static Subscription BuildSubscription(
        int memberId, MembershipPlan plan, DateOnly start, decimal initialPayment, PaymentMethod method)
    {
        // الدفعة الأولى ماينفعش تعدي سعر الباقة
        if (initialPayment > plan.Price)
            throw new BusinessRuleException($"Initial payment cannot exceed the plan price ({plan.Price:0.##}).");

        var subscription = new Subscription
        {
            MemberId = memberId,
            PlanId = plan.Id,
            StartDate = start,
            // النهاية = البداية + مدة الباقة
            EndDate = start.AddDays(plan.DurationInDays),
            Status = SubscriptionStatus.Active,
            // نجمّد السعر وقت الشراء
            PriceAtPurchase = plan.Price
        };

        if (initialPayment > 0)
        {
            // بنضيف الدفعة على Collection الاشتراك، وEF بيربطها بالاشتراك لوحده وقت الحفظ
            // فمش محتاجين نعرف الـ Id قبل ما الاشتراك يتحفظ
            subscription.Payments.Add(new Payment
            {
                Amount = initialPayment,
                Method = method,
                Notes = "Initial payment"
            });
        }

        return subscription;
    }
}