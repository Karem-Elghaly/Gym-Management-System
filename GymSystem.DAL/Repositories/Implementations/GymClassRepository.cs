using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class GymClassRepository : GenericRepository<GymClass>, IGymClassRepository
{
    public GymClassRepository(GymDbContext context) : base(context) { }

    public async Task<GymClass?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Member)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<GymClass>> GetAllWithTrainerAsync()
        => await _dbSet.AsNoTracking()
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .OrderBy(c => c.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<GymClass>> GetUpcomingAsync()
        => await _dbSet.AsNoTracking()
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .Where(c => c.StartTime > DateTime.Now)
            .OrderBy(c => c.StartTime)
            .ToListAsync();
}