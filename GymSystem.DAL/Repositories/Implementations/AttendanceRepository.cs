using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(GymDbContext context) : base(context) { }

    public async Task<IEnumerable<Attendance>> GetByDateAsync(DateOnly date)
    {
        // نحول التاريخ لبداية ونهاية اليوم
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        return await _dbSet.AsNoTracking()
            .Include(a => a.Member)
            .Where(a => a.CheckInTime >= start && a.CheckInTime < end)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();
    }

    // دخول من غير وقت خروج = العضو لسه جوه
    public async Task<Attendance?> GetOpenCheckInAsync(int memberId)
        => await _dbSet
            .Where(a => a.MemberId == memberId && a.CheckOutTime == null)
            .OrderByDescending(a => a.CheckInTime)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Attendance>> GetByMemberAsync(int memberId)
        => await _dbSet.AsNoTracking()
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.CheckInTime)
            .ToListAsync();
}