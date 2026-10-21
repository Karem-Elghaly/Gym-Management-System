using Microsoft.AspNetCore.Http;

namespace GymSystem.BLL.Services.Interfaces;

public interface IFileService
{
    // بتحفظ الصورة في wwwroot/uploads/{folder} وبترجع اسم الملف الجديد
    Task<string> SaveImageAsync(IFormFile file, string folder);

    // بتمسح الصورة لو موجودة (لو الاسم null أو الملف مش موجود مبتعملش حاجة)
    void Delete(string? fileName, string folder);
}