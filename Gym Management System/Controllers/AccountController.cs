using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Account;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly IValidator<LoginViewModel> _loginValidator;
    private readonly IValidator<RegisterUserViewModel> _registerValidator;

    public AccountController(
        IAccountService accountService,
        IValidator<LoginViewModel> loginValidator,
        IValidator<RegisterUserViewModel> registerValidator)
    {
        _accountService = accountService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }


    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

       
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var validation = await _loginValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        try
        {
            await _accountService.LoginAsync(model);
        }
        catch (BusinessRuleException ex)
        {
  
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Password = string.Empty; 
            return View(model);
        }

      
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

 
   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

   

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet]
    public IActionResult CreateUser() => View(new RegisterUserViewModel());

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(RegisterUserViewModel model)
    {
        var validation = await _registerValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            validation.AddToModelState(ModelState);
            return View(model);
        }

        try
        {
            await _accountService.CreateUserAsync(model);
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["Success"] = "User created successfully.";
        return RedirectToAction("Index", "Home");
    }
}