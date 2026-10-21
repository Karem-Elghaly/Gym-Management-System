using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface IGymClassRepository : IGenericRepository<GymClass>
{
    // الحصة مع المدرب والحجوزات (والأعضاء الحاجزين)
    Task<GymClass?> GetWithDetailsAsync(int id);

    // كل الحصص مع المدرب، مرتبة بالميعاد
    Task<IEnumerable<GymClass>> GetAllWithTrainerAsync();

    // الحصص القادمة فقط
    Task<IEnumerable<GymClass>> GetUpcomingAsync();
}