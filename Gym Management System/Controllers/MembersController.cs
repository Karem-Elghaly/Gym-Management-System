using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Members;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;

// الأعضاء متاحين للأدمن وموظف الاستقبال بس. المدرب مالوش دعوة بإدارة الأعضاء
// الـ Roles بتتفصل بفاصلة = "أي واحد منهم" (OR)
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class MembersController : Controller
{
    private readonly IMemberService _service;
    // الـ Validator بييجي من الـ DI (اتسجل أوتوماتيك في AddBllServices)
    private readonly IValidator<MemberViewModel> _validator;

    public MembersController(IMemberService service, IValidator<MemberViewModel> validator)
    {
        _service = service;
        _validator = validator;
    }

    // ===================== القائمة =====================

    // [FromQuery]: بيملا الفلتر من الـ URL (?search=ahmed&page=2)
    public async Task<IActionResult> Index([FromQuery] MemberFilterViewModel filter)
    {
        var result = await _service.GetPagedAsync(filter);

        // بنبعت كلمة البحث للـ View عشان تفضل في الخانة وفي روابط الصفحات
        ViewData["Search"] = filter.Search;
        return View(result);
    }

    // ===================== التفاصيل =====================

    public async Task<IActionResult> Details(int id)
    {
        // لو العضو مش موجود الـ Service بترمي NotFoundException والـ Middleware بيحولها 404
        var model = await _service.GetDetailsAsync(id);
        return View(model);
    }

    // ===================== الإضافة =====================

    [HttpGet]
    public IActionResult Create() => View(new MemberViewModel());

    [HttpPost]
    // بيتأكد إن الفورم جاي من موقعنا (حماية من CSRF)
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberViewModel model)
    {
        // 1) التحقق من شكل المدخلات (FluentValidation)
        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        // 2) قواعد الشغل (زي تكرار التليفون) بتترمي كـ BusinessRuleException من الـ Service
        try
        {
            await _service.CreateAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(nameof(model.Phone), ex.Message);
            return View(model);
        }

        // TempData بتعيش لحد الطلب الجاي بس، والـ _Alerts بتعرضها
        TempData["Success"] = "Member created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ===================== التعديل =====================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
        => View(await _service.GetForEditAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MemberViewModel model)
    {
        // حماية: لو الـ Id في الرابط مختلف عن الـ Id في الفورم (حد عدّل الفورم بإيده)
        if (id != model.Id)
            return BadRequest();

        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        try
        {
            await _service.UpdateAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(nameof(model.Phone), ex.Message);
            return View(model);
        }

        TempData["Success"] = "Member updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ===================== الحذف (Admin بس) =====================

    // صفحة تأكيد قبل الحذف، عشان الحذف ما يحصلش بضغطة غلط
    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
        => View(await _service.GetForEditAsync(id));

    // ActionName("Delete"): الفورم بيبعت لـ /Members/Delete، لكن الدالة اسمها DeleteConfirmed
    // لأن C# مبيسمحش بدالتين بنفس الاسم والـ Parameters
    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Member deleted.";
        }
        catch (BusinessRuleException ex)
        {
            // زي: العضو عنده اشتراك شغال. بنعرض الرسالة ونرجع للقائمة
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}