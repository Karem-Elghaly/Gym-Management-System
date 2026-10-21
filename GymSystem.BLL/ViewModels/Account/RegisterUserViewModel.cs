namespace GymSystem.BLL.ViewModels.Account;

// الأدمن بس هو اللي بينشئ مستخدمين (مفيش تسجيل عام)
public class RegisterUserViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    // Admin / Receptionist / Trainer
    public string Role { get; set; } = string.Empty;
}