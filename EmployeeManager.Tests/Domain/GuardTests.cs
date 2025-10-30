using EmployeeManager.Domain.Validation;
using FluentAssertions;

namespace EmployeeManager.Tests.Domain;

public class GuardTests
{
    [Fact]
    public void Should_throw_for_empty_required_value()
    {
        Action act = () => Guard.Required("", "Name");
        act.Should().Throw<DomainException>()
           .WithMessage("Name is required.");
    }

    [Fact]
    public void Should_validate_valid_email()
    {
        var email = Guard.Email("test@domain.com", "Email");
        email.Should().Be("test@domain.com");
    }

    [Fact]
    public void Should_throw_for_invalid_email()
    {
        Action act = () => Guard.Email("invalid", "Email");
        act.Should().Throw<DomainException>();
    }
}
