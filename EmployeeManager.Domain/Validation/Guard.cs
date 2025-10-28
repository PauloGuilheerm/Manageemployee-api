using System.Text.RegularExpressions;

namespace EmployeeManager.Domain.Validation;

public static class Guard
{
    public static string Required(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field} is required.");
        return value.Trim();
    }

    public static string Email(string? value, string field)
    {
        var validatedValue = Required(value, field);
        if (!Regex.IsMatch(validatedValue, @"^\S+@\S+\.\S+$"))
            throw new DomainException($"{field} is not a valid email.");
        return validatedValue;
    }
}
