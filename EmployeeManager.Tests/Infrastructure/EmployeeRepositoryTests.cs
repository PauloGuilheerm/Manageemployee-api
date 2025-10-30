using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Infrastructure.Persistence;
using EmployeeManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Tests.Infrastructure;

public class EmployeeRepositoryTests
{
    private readonly EmployeeRepository _repo;
    private readonly ApplicationDbContext _context;

    public EmployeeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repo = new EmployeeRepository(_context);
    }

    [Fact]
    public async Task Should_add_and_get_employee_by_docnumber()
    {
        var emp = new Employee(
            "Luke", "Stone", "luke@test.com", "99999999999",
            DateTime.UtcNow.AddYears(-25), Role.Employee, "hash"
        );

        await _repo.AddAsync(emp);
        var result = await _repo.GetByDocNumberAsync("99999999999");

        result.Should().NotBeNull();
        result!.Email.Should().Be("luke@test.com");
    }

    [Fact]
    public async Task Should_check_existence_by_email()
    {
        var emp = new Employee(
            "Mary", "Olive", "mary@test.com", "11111111111",
            DateTime.UtcNow.AddYears(-28), Role.Leader, "hash"
        );

        await _repo.AddAsync(emp);
        var exists = await _repo.ExistsByEmailAsync("mary@test.com");
        exists.Should().BeTrue();
    }
}
