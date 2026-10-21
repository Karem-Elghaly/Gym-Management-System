using GymSystem.DAL.Entities;

namespace GymSystem.DAL.Repositories.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    // عدد الحجوزات الفعالة في حصة (للتحقق من السعة)
    Task<int> CountActiveByClassAsync(int gymClassId);

    // حجز العضو في الحصة لو موجود
    Task<Booking?> GetByMemberAndClassAsync(int memberId, int gymClassId);

    // حجوزات عضو مع بيانات الحصة
    Task<IEnumerable<Booking>> GetByMemberAsync(int memberId);
}