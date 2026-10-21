using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymSystem.DAL.Data.Configurations;

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("Trainers");

        builder.Property(t => t.FullName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Specialty).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Phone).IsRequired().HasMaxLength(11);
        builder.Property(t => t.Bio).HasMaxLength(500);
        builder.Property(t => t.ImageName).HasMaxLength(200);
    }
}