using GymSystem.DAL.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace GymSystem.BLL.ViewModels.Members;

public class MemberViewModel
{
    // 0 = إضافة جديدة، أكبر من 0 = تعديل
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }

    // الملف المرفوع من الفورم
    public IFormFile? ImageFile { get; set; }

    // اسم الصورة الحالية وقت التعديل
    public string? ExistingImageName { get; set; }
}