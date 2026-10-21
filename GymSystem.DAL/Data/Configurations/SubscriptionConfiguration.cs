using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.Property(s => s.PriceAtPurchase).HasColumnType("decimal(18,2)");

        // علاقة: عضو واحد له اشتراكات كتير
        builder.HasOne(s => s.Member)
            .WithMany(m => m.Subscriptions)
            .HasForeignKey(s => s.MemberId)
            // Restrict: ممنوع مسح عضو عنده اشتراكات (حماية إضافية)
            .OnDelete(DeleteBehavior.Restrict);

        // علاقة: باقة واحدة لها اشتراكات كتير
        builder.HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index لتسريع البحث عن اشتراكات عضو بحالة معينة
        builder.HasIndex(s => new { s.MemberId, s.Status });
    }
}