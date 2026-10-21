using FluentValidation;
using Gym_Management_System.Extensions;
using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Account;
using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_Management_System.Controllers;

// [Authorize] على مستوى الكنترولر مش لازم، لأن عندنا AuthorizeFilter عام في Program.cs
// وبنستثني Login بـ [AllowAnonymous]
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

    // ===================== Login =====================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // لو مسجل دخول بالفعل مالوش لازمة يشوف الصفحة
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        // returnUrl: الصفحة اللي كان رايحها قبل ما يتحول للدخول
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
            // خطأ عام على الفورم كله (مش على حقل معين)
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Password = string.Empty; // منرجعش الباسورد للمتصفح
            return View(model);
        }

        // Url.IsLocalUrl: حماية من Open Redirect (محدش يبعت رابط لموقع تاني)
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ===================== Logout =====================

    // POST عشان حد ميعملش لك Logout برابط في صفحة تانية (CSRF)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ===================== إنشاء مستخدم (Admin فقط) =====================

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