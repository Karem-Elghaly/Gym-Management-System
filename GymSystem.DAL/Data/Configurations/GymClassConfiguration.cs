using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Configurations;

public class GymClassConfiguration : IEntityTypeConfiguration<GymClass>
{
    public void Configure(EntityTypeBuilder<GymClass> builder)
    {
        builder.ToTable("GymClasses");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);

        // مدرب واحد له حصص كتير، وممنوع مسح مدرب عنده حصص
        builder.HasOne(c => c.Trainer)
            .WithMany(t => t.Classes)
            .HasForeignKey(c => c.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}