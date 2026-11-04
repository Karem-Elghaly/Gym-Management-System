using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Common;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Payments;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gym_Management_System.Controllers;

[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class PaymentsController : Controller
{
    private readonly IPaymentService _service;
    private readonly IValidator<PaymentViewModel> _validator;

    public PaymentsController(IPaymentService service, IValidator<PaymentViewModel> validator)
    {
        _service = service;
        _validator = validator;
    }

    
    public async Task<IActionResult> Index([FromQuery] PaginationParams paging)
        => View(await _service.GetPagedAsync(paging));


    [HttpGet]
    public async Task<IActionResult> Create(int? subscriptionId = null)
    {
        var model = new PaymentViewModel { SubscriptionId = subscriptionId };
        await LoadLookupsAsync(model.SubscriptionId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentViewModel model)
    {
        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            await LoadLookupsAsync(model.SubscriptionId);
            return View(model);
        }

        try
        {
            await _service.CreateAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            
            ModelState.AddModelError(nameof(model.Amount), ex.Message);
            await LoadLookupsAsync(model.SubscriptionId);
            return View(model);
        }

        TempData["Success"] = "Payment recorded successfully.";
        return RedirectToAction(nameof(Index));
    }


    private async Task LoadLookupsAsync(int? subscriptionId)
        => ViewBag.Subscriptions = new SelectList(
            await _service.GetPayableSubscriptionsAsync(), "Id", "Text", subscriptionId);
}