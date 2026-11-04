using GymSystem.DAL.Common;

namespace Gym_Management_System.Services;


public class CurrentUserService : ICurrentUserService
{
   
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

   
    public string? UserName => _accessor.HttpContext?.User?.Identity?.Name;
}