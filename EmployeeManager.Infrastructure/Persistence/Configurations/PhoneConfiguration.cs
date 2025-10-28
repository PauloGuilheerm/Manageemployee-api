using EmployeeManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManager.Infrastructure.Persistence.Configurations;

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        builder.ToTable("Phones");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Number)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Type)
            .IsRequired();
    }
}
