using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(GymDbContext context) : base(context) { }

    // الحجز المحسوب على السعة هو اللي مش ملغي
    public async Task<int> CountActiveByClassAsync(int gymClassId)
        => await _dbSet.CountAsync(b =>
            b.GymClassId == gymClassId && b.Status != BookingStatus.Cancelled);

    public async Task<Booking?> GetByMemberAndClassAsync(int memberId, int gymClassId)
        => await _dbSet.FirstOrDefaultAsync(b =>
            b.MemberId == memberId && b.GymClassId == gymClassId);

    public async Task<IEnumerable<Booking>> GetByMemberAsync(int memberId)
        => await _dbSet.AsNoTracking()
            .Include(b => b.GymClass)
            .Where(b => b.MemberId == memberId)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();
}