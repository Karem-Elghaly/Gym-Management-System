using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Subscriptions;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gym_Management_System.Controllers;

[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class SubscriptionsController : Controller
{
    private readonly ISubscriptionService _service;

    private readonly IMemberService _members;
    private readonly IPlanService _plans;
    private readonly IValidator<SubscriptionCreateViewModel> _createValidator;
    private readonly IValidator<SubscriptionRenewViewModel> _renewValidator;

    public SubscriptionsController(
        ISubscriptionService service,
        IMemberService members,
        IPlanService plans,
        IValidator<SubscriptionCreateViewModel> createValidator,
        IValidator<SubscriptionRenewViewModel> renewValidator)
    {
        _service = service;
        _members = members;
        _plans = plans;
        _createValidator = createValidator;
        _renewValidator = renewValidator;
    }

   
    public async Task<IActionResult> Index(
        [FromQuery] SubscriptionStatus? status, [FromQuery] PaginationParams paging)
    {
        var result = await _service.GetPagedAsync(status, paging);
        ViewData["Status"] = status;
        return View(result);
    }

 

    public async Task<IActionResult> Details(int id)
        => View(await _service.GetDetailsAsync(id));

   
    [HttpGet]
    public async Task<IActionResult> Create(int? memberId = null)
    {
        var model = new SubscriptionCreateViewModel { MemberId = memberId };
        await LoadLookupsAsync(model.MemberId, model.PlanId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubscriptionCreateViewModel model)
    {
        var validation = await _createValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            await LoadLookupsAsync(model.MemberId, model.PlanId);
            return View(model);
        }

        try
        {
            var id = await _service.CreateAsync(model);
            TempData["Success"] = "Subscription created successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessRuleException ex)
        {
            
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadLookupsAsync(model.MemberId, model.PlanId);
            return View(model);
        }
    }

   

    [HttpGet]
    public async Task<IActionResult> Renew(int id)
    {
        var model = await _service.GetRenewFormAsync(id);
        await LoadPlansAsync(model.PlanId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renew(int id, SubscriptionRenewViewModel model)
    {
        if (id != model.SubscriptionId) return BadRequest();

        var validation = await _renewValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            await LoadPlansAsync(model.PlanId);
            return View(model);
        }

        try
        {
            var newId = await _service.RenewAsync(model);
            TempData["Success"] = "Subscription renewed successfully.";
            return RedirectToAction(nameof(Details), new { id = newId });
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadPlansAsync(model.PlanId);
            return View(model);
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Freeze(int id)
        => RunAsync(id, () => _service.FreezeAsync(id), "Subscription frozen.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Unfreeze(int id)
        => RunAsync(id, () => _service.UnfreezeAsync(id), "Subscription unfrozen and extended.");

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Cancel(int id)
        => RunAsync(id, () => _service.CancelAsync(id), "Subscription cancelled.");

    
    private async Task<IActionResult> RunAsync(int id, Func<Task> action, string successMessage)
    {
        try
        {
            await action();
            TempData["Success"] = successMessage;
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

   
    private async Task LoadLookupsAsync(int? memberId, int? planId)
    {
        ViewBag.Members = new SelectList(await _members.GetLookupAsync(), "Id", "Text", memberId);
        await LoadPlansAsync(planId);
    }

    private async Task LoadPlansAsync(int? planId)
        => ViewBag.Plans = new SelectList(await _plans.GetActiveLookupAsync(), "Id", "Text", planId);
}