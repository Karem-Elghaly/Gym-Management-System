namespace GymSystem.BLL.ViewModels.Account;

public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // تفضل مسجل دخول (Persistent Cookie)
    public bool RememberMe { get; set; }
}