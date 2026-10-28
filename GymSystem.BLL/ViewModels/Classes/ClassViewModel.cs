namespace GymSystem.BLL.ViewModels.Classes;

public class ClassViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TrainerId { get; set; }
    public int MaxCapacity { get; set; }

    // ===== اللي الموظف بيدخله في الفورم: 3 خانات بسيطة =====
    // DateOnly? و TimeOnly? بيتحولوا في الـ View لـ input من نوع date و time
    public DateOnly? ClassDate { get; set; }
    public TimeOnly? StartTimeOfDay { get; set; }
    public TimeOnly? EndTimeOfDay { get; set; }

    // ===== اللي الـ Service والـ Mapping بيستخدموه (مبقاش في الفورم) =====
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    // بتدمج التاريخ مع كل وقت لتكوين التاريخ والوقت الكاملين، بنناديها بعد ما الفاليديشن ينجح
    public void ComposeTimes()
    {
        if (ClassDate.HasValue && StartTimeOfDay.HasValue && EndTimeOfDay.HasValue)
        {
            StartTime = ClassDate.Value.ToDateTime(StartTimeOfDay.Value);
            EndTime = ClassDate.Value.ToDateTime(EndTimeOfDay.Value);
        }
    }

    // العكس: بتفصل التاريخ والوقت الكاملين لـ 3 خانات، بنناديها وقت فتح فورم التعديل
    public void SplitTimes()
    {
        if (StartTime.HasValue)
        {
            ClassDate = DateOnly.FromDateTime(StartTime.Value);
            StartTimeOfDay = TimeOnly.FromDateTime(StartTime.Value);
        }

        if (EndTime.HasValue)
            EndTimeOfDay = TimeOnly.FromDateTime(EndTime.Value);
    }
}