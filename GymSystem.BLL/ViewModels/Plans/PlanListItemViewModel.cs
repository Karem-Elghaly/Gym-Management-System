namespace GymSystem.BLL.ViewModels.Plans;

public class PlanListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public bool IsActive { get; set; }
}