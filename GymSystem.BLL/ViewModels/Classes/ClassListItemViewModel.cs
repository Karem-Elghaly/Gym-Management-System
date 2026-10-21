namespace GymSystem.BLL.ViewModels.Classes;

public class ClassListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxCapacity { get; set; }
    public int BookedCount { get; set; }

    // الأماكن المتبقية، بتتحسب أوتوماتيك من الخاصيتين اللي فوق
    public int AvailableSeats => MaxCapacity - BookedCount;
    public bool IsFull => AvailableSeats <= 0;
}