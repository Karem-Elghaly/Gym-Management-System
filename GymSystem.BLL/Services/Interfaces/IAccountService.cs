using GymSystem.BLL.ViewModels.Account;

namespace GymSystem.BLL.Services.Interfaces;

public interface IAccountService
{
    Task LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task<string> CreateUserAsync(RegisterUserViewModel model);
}