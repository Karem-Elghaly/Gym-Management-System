using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface ITrainerRepository : IGenericRepository<Trainer>
{
    // هل المدرب عنده حصص قادمة؟ (لمنع حذفه)
    Task<bool> HasUpcomingClassesAsync(int trainerId);
}