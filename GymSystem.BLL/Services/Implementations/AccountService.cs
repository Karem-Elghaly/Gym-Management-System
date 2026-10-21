using GymSystem.BLL.Exceptions;
using GymSystem.BLL.Services.Interfaces;
using GymSystem.BLL.ViewModels.Account;
using GymSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GymSystem.BLL.Services.Implementations;

public class AccountService : IAccountService
{
    // UserManager: إدارة المستخدمين (إنشاء، بحث، Roles)
    private readonly UserManager<ApplicationUser> _userManager;
    // SignInManager: تسجيل الدخول والخروج وكوكي الجلسة
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task LoginAsync(LoginViewModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        // نفس الرسالة للحالتين (مش موجود / متوقف) عشان محدش يعرف إيميلات الموظفين بالتجربة
        if (user is null || !user.IsActive)
            throw new BusinessRuleException("Invalid email or password.");

        // lockoutOnFailure: بعد كذا محاولة غلط الحساب بيتقفل مؤقتاً (حماية من تخمين الباسورد)
        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} is locked out", model.Email);
            throw new BusinessRuleException("Account is temporarily locked. Try again later.");
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for {Email}", model.Email);
            throw new BusinessRuleException("Invalid email or password.");
        }

        _logger.LogInformation("User {Email} logged in", model.Email);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
    }

    public async Task<string> CreateUserAsync(RegisterUserViewModel model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) is not null)
            throw new BusinessRuleException("A user with this email already exists.");

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
            // Identity بيحتاج UserName، فبنستخدم الإيميل نفسه
            UserName = model.Email,
            // الأدمن هو اللي بينشئ الحساب فمش محتاجين تأكيد إيميل
            EmailConfirmed = true
        };

        var created = await _userManager.CreateAsync(user, model.Password);
        if (!created.Succeeded)
            throw new BusinessRuleException(string.Join(" ", created.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            // لو إضافة الـ Role فشلت نمسح المستخدم، عشان منسيبش حساب من غير صلاحية
            await _userManager.DeleteAsync(user);
            throw new BusinessRuleException(string.Join(" ", roleResult.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {Email} created with role {Role}", model.Email, model.Role);
        return user.Id;
    }
}