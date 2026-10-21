namespace GymSystem.BLL.ViewModels.Members;

public class MemberListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ImageName { get; set; }
    public string? CurrentPlanName { get; set; }
    public bool HasActiveSubscription { get; set; }
    public DateTime CreatedAt { get; set; }
}