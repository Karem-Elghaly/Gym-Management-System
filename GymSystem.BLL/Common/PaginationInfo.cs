namespace GymSystem.BLL.Common;

// بيانات الصفحة بس، من غير نوع العناصر، عشان الـ Partial تفضل واحدة لكل الجداول
public record PaginationInfo(int Page, int TotalPages, bool HasPrevious, bool HasNext);