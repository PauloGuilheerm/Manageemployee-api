using System.Text.Json.Serialization;
using EmployeeManager.Domain.Abstractions;
using EmployeeManager.Domain.Enums;

namespace EmployeeManager.Domain.Entities;

public sealed class Phone : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; }
    [JsonIgnore]
    public Employee Employee { get; set; }
    public PhoneType Type { get; set; }
    public Guid EmployeeId { get; set; }
    public Phone(string number, PhoneType type)
    {
        Number = string.IsNullOrWhiteSpace(number)
            ? throw new ArgumentException("Phone number is required.", nameof(number))
            : number.Trim();

        Type = type;
    }
}
