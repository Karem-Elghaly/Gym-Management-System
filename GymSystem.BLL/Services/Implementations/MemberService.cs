using AutoMapper;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Members;
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using GymSystem.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class MemberService : IMemberService
{
    // اسم فولدر صور الأعضاء، ثابت في مكان واحد
    private const string ImagesFolder = "members";

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileService _files;
    // ILogger<MemberService>: الـ <MemberService> بيظهر في اللوج عشان نعرف الرسالة جاية منين
    private readonly ILogger<MemberService> _logger;

    // كل الاعتماديات بتيجي من الـ DI (Constructor Injection)
    public MemberService(IUnitOfWork uow, IMapper mapper, IFileService files, ILogger<MemberService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _files = files;
        _logger = logger;
    }

    public async Task<PagedResult<MemberListItemViewModel>> GetPagedAsync(MemberFilterViewModel filter)
    {
        // الـ Repository بيرجع صفحة الأعضاء + الإجمالي
        var (members, total) = await _uow.Members.GetPagedAsync(filter.Search, filter.Page, filter.PageSize);

        var items = _mapper.Map<List<MemberListItemViewModel>>(members);

        // نجيب الاشتراكات النشطة لأعضاء الصفحة دي بس في Query واحد (مش Query لكل عضو)
        var ids = items.Select(i => i.Id).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var activeSubs = await _uow.Subscriptions.Query()
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => ids.Contains(s.MemberId)
                        && s.Status == SubscriptionStatus.Active
                        && s.EndDate >= today)
            .ToListAsync();

        // لكل عضو ناخد أحدث اشتراك نشط (احتياطاً لو عنده اتنين بسبب التجديد المبكر)
        var activeByMember = activeSubs
            .GroupBy(s => s.MemberId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.EndDate).First());

        foreach (var item in items)
        {
            // TryGetValue بتدور في القاموس من غير ما ترمي خطأ لو المفتاح مش موجود
            if (activeByMember.TryGetValue(item.Id, out var sub))
            {
                item.HasActiveSubscription = true;
                item.CurrentPlanName = sub.Plan.Name;
            }
        }

        return new PagedResult<MemberListItemViewModel>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<MemberViewModel> GetForEditAsync(int id)
    {
        // ?? throw: لو الـ Repository رجّع null نرمي NotFoundException
        var member = await _uow.Members.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Member), id);

        return _mapper.Map<MemberViewModel>(member);
    }

    public async Task<MemberDetailsViewModel> GetDetailsAsync(int id)
    {
        var member = await _uow.Members.GetWithSubscriptionsAsync(id)
            ?? throw new NotFoundException(nameof(Member), id);

        var vm = _mapper.Map<MemberDetailsViewModel>(member);
        vm.Age = CalculateAge(member.DateOfBirth);

        // نجيب المدفوع لكل اشتراكات العضو في Query واحد
        var paid = await _uow.Payments.GetPaidTotalsAsync(vm.Subscriptions.Select(s => s.Id));
        foreach (var s in vm.Subscriptions)
            s.TotalPaid = paid.GetValueOrDefault(s.Id); // لو مفيش دفعات ترجع 0

        // الأحدث أولاً
        vm.Subscriptions = vm.Subscriptions.OrderByDescending(s => s.StartDate).ToList();
        return vm;
    }

    public async Task<int> CreateAsync(MemberViewModel model)
    {
        // قاعدة شغل: التليفون ما يتكررش (الـ Query Filter بيستبعد المحذوفين فبيطابق الـ Unique Index)
        if (await _uow.Members.GetByPhoneAsync(model.Phone) is not null)
            throw new BusinessRuleException("A member with this phone number already exists.");

        var member = _mapper.Map<Member>(model);

        // لو اترفعت صورة نحفظها ونخزن اسمها
        if (model.ImageFile is not null)
            member.ImageName = await _files.SaveImageAsync(model.ImageFile, ImagesFolder);

        try
        {
            await _uow.Members.AddAsync(member);
            await _uow.SaveChangesAsync();
        }
        catch
        {
            // لو الحفظ فشل نمسح الصورة اللي اترفعت عشان ما نسيبش ملفات يتيمة
            _files.Delete(member.ImageName, ImagesFolder);
            throw; // نعيد رمي نفس الخطأ للـ Middleware
        }

        // Structured Logging: {MemberId} متغير بيتسجل كـ Property تقدر تدور بيه في Serilog
        _logger.LogInformation("Member {MemberId} created", member.Id);
        return member.Id;
    }

    public async Task UpdateAsync(MemberViewModel model)
    {
        var member = await _uow.Members.GetByIdAsync(model.Id)
            ?? throw new NotFoundException(nameof(Member), model.Id);

        // التليفون مكرر عند عضو تاني؟ (نستثني العضو نفسه)
        var samePhone = await _uow.Members.GetByPhoneAsync(model.Phone);
        if (samePhone is not null && samePhone.Id != model.Id)
            throw new BusinessRuleException("Another member already uses this phone number.");

        // Map(source, destination): بينقل القيم على الكائن الموجود بدل ما يعمل واحد جديد
        // الـ member متتبع (Tracked) فـ EF هيلاحظ التغييرات لوحده ومحتاجين نناديش Update
        _mapper.Map(model, member);

        if (model.ImageFile is not null)
        {
            var oldImage = member.ImageName;
            member.ImageName = await _files.SaveImageAsync(model.ImageFile, ImagesFolder);
            await _uow.SaveChangesAsync();
            // نمسح الصورة القديمة بعد نجاح الحفظ بس
            _files.Delete(oldImage, ImagesFolder);
        }
        else
        {
            await _uow.SaveChangesAsync();
        }

        _logger.LogInformation("Member {MemberId} updated", member.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _uow.Members.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Member), id);

        // قاعدة شغل: ممنوع نمسح عضو اشتراكه شغال أو متجمد
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasOpenSub = await _uow.Subscriptions.AnyAsync(s =>
            s.MemberId == id
            && ((s.Status == SubscriptionStatus.Active && s.EndDate >= today)
                || s.Status == SubscriptionStatus.Frozen));

        if (hasOpenSub)
            throw new BusinessRuleException("Cannot delete a member with an active or frozen subscription.");

        // Delete هنا Soft Delete (الـ DbContext بيحوله). الصورة بنسيبها عشان لو اتسترجع
        _uow.Members.Delete(member);
        await _uow.SaveChangesAsync();

        _logger.LogWarning("Member {MemberId} deleted (soft)", id);
    }

    public async Task<IReadOnlyList<LookupItem>> GetLookupAsync()
        => await _uow.Members.Query()
            .AsNoTracking()
            .OrderBy(m => m.FullName)
            // الاسم + التليفون عشان الموظف يفرّق بين أعضاء بنفس الاسم
            .Select(m => new LookupItem(m.Id, m.FullName + " - " + m.Phone))
            .ToListAsync();

    // حساب العمر: الفرق بين السنين، ولو عيد ميلاده السنة دي لسه ماجاش ننقص سنة
    private static int CalculateAge(DateOnly dob)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }
}