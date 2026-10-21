using AutoMapper;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Payments;
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IUnitOfWork uow, IMapper mapper, ILogger<PaymentService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<PaymentListItemViewModel>> GetPagedAsync(PaginationParams paging)
    {
        // بنبني الـ Query الأول من غير تنفيذ، وبنحمل العضو والباقة عن طريق الاشتراك
        var query = _uow.Payments.Query()
            .AsNoTracking()
            .Include(p => p.Subscription).ThenInclude(s => s.Member)
            .Include(p => p.Subscription).ThenInclude(s => s.Plan);

        var total = await query.CountAsync();

        var payments = await query
            .OrderByDescending(p => p.PaidAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync();

        var items = _mapper.Map<List<PaymentListItemViewModel>>(payments);
        return new PagedResult<PaymentListItemViewModel>(items, total, paging.Page, paging.PageSize);
    }

    public async Task<int> CreateAsync(PaymentViewModel model)
    {
        var sub = await _uow.Subscriptions.GetByIdAsync(model.SubscriptionId!.Value)
            ?? throw new NotFoundException(nameof(Subscription), model.SubscriptionId);

        if (sub.Status == SubscriptionStatus.Cancelled)
            throw new BusinessRuleException("Cannot add a payment to a cancelled subscription.");

        // المتبقي = سعر الاشتراك - اللي اتدفع قبل كده
        var paid = await _uow.Payments.GetTotalPaidForSubscriptionAsync(sub.Id);
        var remaining = sub.PriceAtPurchase - paid;

        if (remaining <= 0)
            throw new BusinessRuleException("This subscription is already fully paid.");

        if (model.Amount > remaining)
            throw new BusinessRuleException($"Amount exceeds the remaining balance ({remaining:0.##}).");

        // بنبني الدفعة بإيدينا بدل AutoMapper عشان نتحكم في كل حقل
        var payment = new Payment
        {
            SubscriptionId = sub.Id,
            Amount = model.Amount,
            Method = model.Method,
            Notes = model.Notes
        };

        await _uow.Payments.AddAsync(payment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Payment {PaymentId} of {Amount} recorded for subscription {SubscriptionId}",
            payment.Id, payment.Amount, sub.Id);
        return payment.Id;
    }

    public async Task<IReadOnlyList<LookupItem>> GetPayableSubscriptionsAsync()
    {
        // كل الاشتراكات غير الملغية مع العضو والباقة
        var subs = await _uow.Subscriptions.Query()
            .AsNoTracking()
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .Where(s => s.Status != SubscriptionStatus.Cancelled)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var paid = await _uow.Payments.GetPaidTotalsAsync(subs.Select(s => s.Id));

        // نفلتر في الذاكرة على اللي لسه عليه متبقي، ونعرض المتبقي في النص
        return subs
            .Select(s => new { Sub = s, Remaining = s.PriceAtPurchase - paid.GetValueOrDefault(s.Id) })
            .Where(x => x.Remaining > 0)
            .Select(x => new LookupItem(x.Sub.Id,
                $"{x.Sub.Member.FullName} - {x.Sub.Plan.Name} (Remaining {x.Remaining:0.##} EGP)"))
            .ToList();
    }
}