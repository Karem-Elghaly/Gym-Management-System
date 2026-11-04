using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Trainers;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;

[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class TrainersController : Controller
{
    private readonly ITrainerService _service;
    private readonly IValidator<TrainerViewModel> _validator;

    public TrainersController(ITrainerService service, IValidator<TrainerViewModel> validator)
    {
        _service = service;
        _validator = validator;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new TrainerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainerViewModel model)
    {
        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        await _service.CreateAsync(model);
        TempData["Success"] = "Trainer created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id) => View(await _service.GetByIdAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TrainerViewModel model)
    {
        if (id != model.Id) return BadRequest();

        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        await _service.UpdateAsync(model);
        TempData["Success"] = "Trainer updated successfully.";
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
            TempData["Success"] = "Trainer deleted.";
        }
        catch (BusinessRuleException ex)
        {
        
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}