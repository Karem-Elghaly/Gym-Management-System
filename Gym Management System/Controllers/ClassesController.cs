using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Classes;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gym_Management_System.Controllers;


[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist},{UserRoles.Trainer}")]
public class ClassesController : Controller
{
    private const string StaffRoles = $"{UserRoles.Admin},{UserRoles.Receptionist}";

    private readonly IClassService _service;
    private readonly ITrainerService _trainers;
    private readonly IMemberService _members;
    private readonly IValidator<ClassViewModel> _validator;

    public ClassesController(
        IClassService service, ITrainerService trainers,
        IMemberService members, IValidator<ClassViewModel> validator)
    {
        _service = service;
        _trainers = trainers;
        _members = members;
        _validator = validator;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    public async Task<IActionResult> Details(int id)
    {
        var model = await _service.GetDetailsAsync(id);


        if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Receptionist))
            ViewBag.Members = new SelectList(await _members.GetLookupAsync(), "Id", "Text");

        return View(model);
    }


    [Authorize(Roles = StaffRoles)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
       
        var model = new ClassViewModel
        {
            MaxCapacity = 20,
            ClassDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            StartTimeOfDay = new TimeOnly(18, 0),
            EndTimeOfDay = new TimeOnly(19, 0)
        };
        await LoadTrainersAsync(model.TrainerId);
        return View(model);
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassViewModel model)
    {
        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            await LoadTrainersAsync(model.TrainerId);
            return View(model);
        }

       
        model.ComposeTimes();

        try
        {
            await _service.CreateAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadTrainersAsync(model.TrainerId);
            return View(model);
        }

        TempData["Success"] = "Class created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = StaffRoles)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _service.GetForEditAsync(id);
  
        model.SplitTimes();
        await LoadTrainersAsync(model.TrainerId);
        return View(model);
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClassViewModel model)
    {
        if (id != model.Id) return BadRequest();

        var validation = await _validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            await LoadTrainersAsync(model.TrainerId);
            return View(model);
        }

        model.ComposeTimes();

        try
        {
            await _service.UpdateAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadTrainersAsync(model.TrainerId);
            return View(model);
        }

        TempData["Success"] = "Class updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Class deleted.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Index));
    }



    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(BookingViewModel model)
    {
        if (model.MemberId is null)
        {
            TempData["Error"] = "Please select a member.";
            return RedirectToAction(nameof(Details), new { id = model.GymClassId });
        }

        try
        {
            await _service.BookAsync(model);
            TempData["Success"] = "Member booked successfully.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.GymClassId });
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int bookingId, int classId)
    {
        try
        {
            await _service.CancelBookingAsync(bookingId);
            TempData["Success"] = "Booking cancelled.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = classId });
    }

    private async Task LoadTrainersAsync(int? trainerId)
        => ViewBag.Trainers = new SelectList(await _trainers.GetLookupAsync(), "Id", "Text", trainerId);
}