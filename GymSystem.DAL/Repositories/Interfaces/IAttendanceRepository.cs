using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface IAttendanceRepository : IGenericRepository<Attendance>
{
    // حضور يوم معين مع بيانات العضو
    Task<IEnumerable<Attendance>> GetByDateAsync(DateOnly date);

    // آخر دخول لعضو لسه ماخرجش (لمنع تسجيل دخول مرتين)
    Task<Attendance?> GetOpenCheckInAsync(int memberId);

    // حضور عضو
    Task<IEnumerable<Attendance>> GetByMemberAsync(int memberId);
}