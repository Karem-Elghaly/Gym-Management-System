namespace GymSystem.BLL.ViewModels.Plans;

public class PlanViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }

    // الباقة شغالة افتراضياً
    public bool IsActive { get; set; } = true;
}