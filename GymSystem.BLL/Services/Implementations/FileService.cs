using GymSystem.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GymSystem.BLL.Services.Implementations;

public class FileService : IFileService
{
    // IWebHostEnvironment بيعرفنا مكان فولدر wwwroot على الجهاز
    private readonly IWebHostEnvironment _env;

    public FileService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveImageAsync(IFormFile file, string folder)
    {
        // المسار الكامل للفولدر: .../wwwroot/uploads/members
        var folderPath = Path.Combine(_env.WebRootPath, "uploads", folder);

        // نتأكد إن الفولدر موجود (لو موجود مش بيعمل حاجة)
        Directory.CreateDirectory(folderPath);

        // اسم فريد عشان صورتين بنفس الاسم ما يكتبوش فوق بعض
        // Guid رقم عشوائي عمره ما بيتكرر، وبناخد امتداد الملف الأصلي
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";

        var fullPath = Path.Combine(folderPath, fileName);

        // await using: بيقفل الملف أوتوماتيك لما نخلص
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        // بنرجع الاسم بس، وده اللي بيتخزن في الداتابيز
        return fileName;
    }

    public void Delete(string? fileName, string folder)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;

        // Path.GetFileName بتشيل أي مسارات من الاسم، حماية من هجوم Path Traversal (../../)
        var safeName = Path.GetFileName(fileName);
        var fullPath = Path.Combine(_env.WebRootPath, "uploads", folder, safeName);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}