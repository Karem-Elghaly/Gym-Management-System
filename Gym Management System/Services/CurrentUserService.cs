using GymSystem.DAL.Common;

namespace Gym_Management_System.Services;

// هنا بس بنعرف الـ HttpContext، والـ DAL فاضلة مستقلة عن الويب
public class CurrentUserService : ICurrentUserService
{
    // IHttpContextAccessor بيدينا الطلب الحالي من أي مكان في الكود
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    // ?. بتمنع الخطأ لو مفيش HttpContext (زي وقت الـ Seeder أو الـ Background Service)
    // Identity.Name هو الـ UserName، وإحنا خليناه الإيميل
    public string? UserName => _accessor.HttpContext?.User?.Identity?.Name;
}