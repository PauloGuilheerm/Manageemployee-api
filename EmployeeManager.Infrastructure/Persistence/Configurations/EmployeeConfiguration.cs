using EmployeeManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManager.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.DocNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.DocNumber)
            .IsUnique();

        builder.Property(e => e.PasswordHash)
            .IsRequired();

        builder.HasMany(e => e.Phones)
            .WithOne(p => p.Employee)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
