namespace GymSystem.BLL.ViewModels.Trainers;

public class TrainerListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // عدد حصصه القادمة، للعرض في الجدول
    public int UpcomingClassesCount { get; set; }
}