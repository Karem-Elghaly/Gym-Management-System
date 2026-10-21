using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class TrainerRepository : GenericRepository<Trainer>, ITrainerRepository
{
    public TrainerRepository(GymDbContext context) : base(context) { }

    // هل في حصة للمدرب ميعادها لسه جاي؟
    public async Task<bool> HasUpcomingClassesAsync(int trainerId)
        => await _context.GymClasses
           .AnyAsync(c => c.TrainerId == trainerId && c.StartTime > DateTime.Now);
}