using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasOne(b => b.Member)
            .WithMany(m => m.Bookings)
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.GymClass)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.GymClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // نفس العضو ميحجزش نفس الحصة مرتين (للسجلات غير المحذوفة)
        builder.HasIndex(b => new { b.MemberId, b.GymClassId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}