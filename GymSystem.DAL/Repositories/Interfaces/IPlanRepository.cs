using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface IPlanRepository : IGenericRepository<MembershipPlan>
{
    // الباقات المتاحة للبيع بس (IsActive = true)
    Task<IEnumerable<MembershipPlan>> GetActivePlansAsync();

    // هل في اسم باقة مكرر؟ excludeId لاستثناء الباقة نفسها وقت التعديل
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
}