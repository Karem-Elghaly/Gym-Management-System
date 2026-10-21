using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Configurations;

// IEntityTypeConfiguration<Member> يعني الكلاس ده مسؤول عن إعدادات جدول Member بس
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        // اسم الجدول في الداتابيز
        builder.ToTable("Members");

        // الاسم: مطلوب وأقصى طول 100 حرف
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(100);

        // التليفون: مطلوب، 11 رقم
        builder.Property(m => m.Phone).IsRequired().HasMaxLength(11);

        // الإيميل: اختياري وأقصى طول 150
        builder.Property(m => m.Email).HasMaxLength(150);

        builder.Property(m => m.ImageName).HasMaxLength(200);
        builder.Property(m => m.Address).HasMaxLength(250);

        // Index فريد على التليفون: يمنع تكرار نفس الرقم
        // الشرط بيخلي الـ Unique يطبق على السجلات غير المحذوفة بس،
        // عشان لو عضو اتمسح (Soft Delete) نقدر نسجل نفس الرقم تاني
        builder.HasIndex(m => m.Phone)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}