using EmployeeManager.Domain.Abstractions;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Validation;

namespace EmployeeManager.Domain.Entities;

public sealed class Employee : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string DocNumber { get; private set; }
    public DateTime BirthDate { get; private set; }
    public Role Role { get; private set; }

    public Guid? ManagerId { get; private set; }
    public Employee? Manager { get; private set; }

    private readonly List<Phone> _phones = new();
    public IReadOnlyCollection<Phone> Phones => _phones.AsReadOnly();

    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    internal Employee() { }

    public Employee(
        string firstName,
        string lastName,
        string email,
        string docNumber,
        DateTime birthDate,
        Role role,
        string passwordHash,
        Guid? managerId = null)
    {
        FirstName = Guard.Required(firstName, nameof(firstName));
        LastName = Guard.Required(lastName, nameof(lastName));
        Email = Guard.Email(email, nameof(email));
        DocNumber = Guard.Required(docNumber, nameof(docNumber));
        BirthDate = birthDate;
        PasswordHash = Guard.Required(passwordHash, nameof(passwordHash));
        Role = role;

        EnsureAdult(birthDate);
        ManagerId = managerId;
    }

    public void ChangeManager(Guid? managerId)
    {
        if (managerId.HasValue && managerId.Value == Id)
            throw new DomainException("Employee cannot be their own manager.");
        ManagerId = managerId;
    }

    public void ChangeRole(Role newRole)
    {
        Role = newRole;
    }

    public void AddPhone(Phone phone)
    {
        ArgumentNullException.ThrowIfNull(phone);
        _phones.Add(phone);
    }

    public void EnsureAtLeastOnePhone()
    {
        if (_phones.Count == 0)
            throw new DomainException("Employee must have at least one phone.");
    }

    private static void EnsureAdult(DateTime birthDate)
    {
        var limit = DateTime.UtcNow.Date.AddYears(-18);
        if (birthDate.Date > limit)
            throw new DomainException("Employee must be at least 18 years old.");
    }
}
