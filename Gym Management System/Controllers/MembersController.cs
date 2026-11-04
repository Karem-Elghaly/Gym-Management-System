using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Members;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;


[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Receptionist}")]
public class MembersController : Controller
{
    private readonly IMemberService _service;
   
    private readonly IValidator<MemberViewModel> _validator;

    public MembersController(IMemberService service, IValidator<MemberViewModel> validator)
    {
        _service = service;
        _validator = validator;
    }


    public async Task<IActionResult> Index([FromQuery] MemberFilterViewModel filter)
    {
        var result = await _service.GetPagedAsync(filter);

     
        ViewData["Search"] = filter.Search;
        return View(result);
    }

   

    public async Task<IActionResult> Details(int id)
    {
 
        var model = await _service.GetDetailsAsync(id);
        return View(model);
    }


    [HttpGet]
    public IActionResult Create() => View(new MemberViewModel());

    [HttpPost]

    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberViewModel model)
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
            ModelState.AddModelError(nameof(model.Phone), ex.Message);
            return View(model);
        }

      
        TempData["Success"] = "Member created successfully.";
        return RedirectToAction(nameof(Index));
    }

    

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
        => View(await _service.GetForEditAsync(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MemberViewModel model)
    {
        
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

    
    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
        => View(await _service.GetForEditAsync(id));

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
          
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}