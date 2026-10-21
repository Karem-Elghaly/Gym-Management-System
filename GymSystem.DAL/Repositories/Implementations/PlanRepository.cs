using GymSystem.DAL.Data;
using GymSystem.DAL.Entities;
using GymSystem.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Repositories.Implementations;

public class PlanRepository : GenericRepository<MembershipPlan>, IPlanRepository
{
    public PlanRepository(GymDbContext context) : base(context) { }

    public async Task<IEnumerable<MembershipPlan>> GetActivePlansAsync()
        => await _dbSet.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DurationInDays)
            .ToListAsync();

    // لو excludeId موجود نستثني الباقة نفسها (وقت التعديل)
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        => await _dbSet.AnyAsync(p => p.Name == name && (excludeId == null || p.Id != excludeId));
}