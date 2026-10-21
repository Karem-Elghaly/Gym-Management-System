using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Attendance;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;

// تسجيل الحضور شغل الاستقبال والأدمن
[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _service;

    public AttendanceController(IAttendanceService service) => _service = service;

    // date اختياري، الافتراضي النهاردة
    public async Task<IActionResult> Index(DateOnly? date = null)
    {
        var day = date ?? DateOnly.FromDateTime(DateTime.Now);
        ViewData["Date"] = day;
        return View(await _service.GetByDateAsync(day));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(CheckInViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Phone))
        {
            TempData["Error"] = "Please enter the member's phone number.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var result = await _service.CheckInAsync(model);
            TempData["Success"] = $"{result.MemberName} checked in.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id)
    {
        try
        {
            await _service.CheckOutAsync(id);
            TempData["Success"] = "Checked out.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}