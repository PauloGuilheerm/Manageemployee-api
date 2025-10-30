using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using FluentAssertions;

namespace EmployeeManager.Tests.Domain;

public class PhoneTests
{
    [Fact]
    public void Should_create_valid_phone()
    {
        var phone = new Phone("99999999", PhoneType.Mobile);
        phone.Number.Should().Be("99999999");
        phone.Type.Should().Be(PhoneType.Mobile);
    }

    [Fact]
    public void Should_not_accept_empty_number()
    {
        Action act = () => new Phone("", PhoneType.Home);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Phone number is required.*");
    }
}
