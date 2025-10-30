using EmployeeManager.Domain.Abstractions;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Validation;

namespace EmployeeManager.Domain.Entities;

public sealed class Employee : IAggregateRoot
{
    public Guid Id { get;  set; } = Guid.NewGuid();
    public string FirstName { get;  set; }
    public string LastName { get;  set; }
    public string Email { get;  set; }
    public string DocNumber { get;  set; }
    public DateTime BirthDate { get;  set; }
    public Role Role { get;  set; }

    public Guid? ManagerId { get;  set; }
    public Employee? Manager { get;  set; }
    public List<Phone> Phones { get; set; } = new();
    public string PasswordHash { get;  set; }
    public DateTime CreatedAt { get;  set; } = DateTime.UtcNow;

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

    public void ChangeBirthDate(DateTime newBirthDate)
    {
        EnsureAdult(newBirthDate);
        BirthDate = newBirthDate;
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
        Phones.Add(phone);
    }

    public void ResetPhones()
    {
        Phones.Clear();
    }

    public void EnsureAtLeastOnePhone()
    {
        if (Phones.Count == 0)
            throw new DomainException("Employee must have at least one phone.");
    }

     static void EnsureAdult(DateTime birthDate)
    {
        var limit = DateTime.UtcNow.Date.AddYears(-18);
        if (birthDate.Date > limit)
            throw new DomainException("Employee must be at least 18 years old.");
    }

    public void ChangeNames(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
