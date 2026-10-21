using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Configurations;

public class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        builder.ToTable("MembershipPlans");

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);

        // decimal(18,2): 18 رقم إجمالي منهم 2 بعد الفاصلة. لو منحددش EF هيدي تحذير
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");

        // اسم الباقة فريد بين الباقات غير المحذوفة
        builder.HasIndex(p => p.Name).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}