using System.Security.Cryptography;
using System.Text;
using FluentAssertions;

namespace EmployeeManager.Tests.Utils;

public class PasswordHashTests
{
    private static string Hash(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    [Fact]
    public void Should_generate_same_hash_for_same_password()
    {
        var h1 = Hash("Admin@123");
        var h2 = Hash("Admin@123");

        h1.Should().Be(h2);
    }

    [Fact]
    public void Should_generate_different_hashes_for_different_passwords()
    {
        var h1 = Hash("password1");
        var h2 = Hash("password2");

        h1.Should().NotBe(h2);
    }
}
