using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Validation;
using FluentAssertions;

namespace EmployeeManager.Tests.Domain;

public class EmployeeTests
{
    [Fact]
    public void Should_create_valid_employee()
    {
        var emp = new Employee(
            "John", "Doe", "john@test.com", "12345678900",
            DateTime.UtcNow.AddYears(-25),
            Role.Employee, "hash_password"
        );

        emp.FirstName.Should().Be("John");
        emp.Role.Should().Be(Role.Employee);
    }

    [Fact]
    public void Should_not_allow_under_18_years_old()
    {
        Action act = () => new Employee(
            "Anna", "Young", "anna@test.com", "12345678900",
            DateTime.UtcNow.AddYears(-17),
            Role.Employee, "hash"
        );

        act.Should().Throw<DomainException>()
           .WithMessage("Employee must be at least 18 years old.");
    }

    [Fact]
    public void Should_not_allow_self_manager()
    {
        var emp = new Employee(
            "Carl", "Pereira", "carl@test.com", "22233344455",
            DateTime.UtcNow.AddYears(-30), Role.Employee, "hash"
        );

        emp.Invoking(e => e.ChangeManager(emp.Id))
           .Should().Throw<DomainException>()
           .WithMessage("Employee cannot be their own manager.");
    }

    [Fact]
    public void Should_require_at_least_one_phone()
    {
        var emp = new Employee(
            "Mark", "Lima", "mark@test.com", "123",
            DateTime.UtcNow.AddYears(-20),
            Role.Employee, "hash"
        );

        emp.Invoking(e => e.EnsureAtLeastOnePhone())
           .Should().Throw<DomainException>()
           .WithMessage("Employee must have at least one phone.");
    }
}
