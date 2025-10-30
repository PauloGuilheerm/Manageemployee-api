using EmployeeManager.Domain.Enums;
using FluentAssertions;

namespace EmployeeManager.Tests.Domain;

public class RbacTests
{
    [Theory]
    [InlineData(Role.Director, Role.Leader, true)]
    [InlineData(Role.Leader, Role.Employee, true)]
    [InlineData(Role.Employee, Role.Director, false)]
    [InlineData(Role.Leader, Role.Director, false)]
    public void Should_validate_RBAC_permissions(Role current, Role target, bool expected)
    {
        bool result = (int)current >= (int)target;
        result.Should().Be(expected);
    }
}
