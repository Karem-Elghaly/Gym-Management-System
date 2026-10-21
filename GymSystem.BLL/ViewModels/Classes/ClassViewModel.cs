namespace GymSystem.BLL.ViewModels.Classes;

public class ClassViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TrainerId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int MaxCapacity { get; set; }
}