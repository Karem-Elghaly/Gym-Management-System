using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Plans;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;

// الباقات: الأدمن وموظف الاستقبال يشوفوا ويضيفوا ويعدلوا، والحذف للأدمن بس
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class PlansController : Controller
{
    private readonly IPlanService _service;
    private readonly IValidator<PlanViewModel> _validator;

    public PlansController(IPlanService service, IValidator<PlanViewModel> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new PlanViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanViewModel model)
    {
        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        try
        {
            await _service.CreateAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            // التكرار في الاسم: بنعرض الرسالة تحت خانة الاسم
            ModelState.AddModelError(nameof(model.Name), ex.Message);
            return View(model);
        }

        TempData["Success"] = "Plan created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id) => View(await _service.GetByIdAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlanViewModel model)
    {
        if (id != model.Id) return BadRequest();

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
            ModelState.AddModelError(nameof(model.Name), ex.Message);
            return View(model);
        }

        TempData["Success"] = "Plan updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // تفعيل/إيقاف: POST لأنه بيغيّر بيانات
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        await _service.ToggleActiveAsync(id);
        TempData["Success"] = "Plan status updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) => View(await _service.GetByIdAsync(id));

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Plan deleted.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}