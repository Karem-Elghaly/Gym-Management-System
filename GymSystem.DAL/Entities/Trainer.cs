using GymSystem.DAL.Common;

namespace GymSystem.DAL.Entities;

// المدرب
public class Trainer : BaseEntity
{
    // الاسم الكامل
    public string FullName { get; set; } = string.Empty;

    // التخصص (كمال أجسام، يوغا، كارديو...)
    public string Specialty { get; set; } = string.Empty;

    // رقم التليفون
    public string Phone { get; set; } = string.Empty;

    // نبذة عن المدرب، اختياري
    public string? Bio { get; set; }

    // اسم ملف صورة المدرب، اختياري
    public string? ImageName { get; set; }

    // الحصص اللي المدرب بيدرّسها
    public ICollection<GymClass> Classes { get; set; } = new List<GymClass>();
}