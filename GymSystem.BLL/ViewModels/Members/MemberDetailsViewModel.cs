using GymSystem.BLL.ViewModels.Subscriptions;
using GymSystem.DAL.Common.Enums;

namespace GymSystem.BLL.ViewModels.Members;

public class MemberDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? ImageName { get; set; }
    public int Age { get; set; }
    public List<SubscriptionListItemViewModel> Subscriptions { get; set; } = new();
}