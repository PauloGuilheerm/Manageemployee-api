using System;
using System.Security.Cryptography;
using System.Text;
using EmployeeManager.Api.Services;
using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Repositories;
using EmployeeManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly ApplicationDbContext _db;
    private readonly JwtService _jwt;
    public AuthController(IEmployeeRepository repo, ApplicationDbContext db, JwtService jwt)
    {
        _repo = repo;
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var employee = await _repo.GetByDocNumberAsync(request.DocNumber, ct);

            if (employee is null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new
                {
                    message = "Invalid credentials."
                });
            }

            var incomingHash = HashPassword(request.Password);

            if (!string.Equals(incomingHash, employee.PasswordHash, StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new
                {
                    message = "Invalid credentials."
                });
            }

            var token = _jwt.GenerateToken(employee);

            return StatusCode(StatusCodes.Status200OK, new
            {
                token,
                employee = new
                {
                    id = employee.Id,
                    fullName = $"{employee.FirstName} {employee.LastName}",
                    role = employee.Role.ToString(),
                    email = employee.Email
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
    }

    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var currentUserRole = GetCurrentUserRole();
            if (!CanCreate(currentUserRole, request.Role))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You don't have permission to create an employee with this role."
                });
            }

            var emailExists = await _db.Employees.AnyAsync(e => e.Email == request.Email, ct);
            if (emailExists)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    message = "This email address is already in use."
                });
            }

            var docExists = await _db.Employees.AnyAsync(e => e.DocNumber == request.DocNumber, ct);
            if (docExists)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    message = "An employee with this document number already exists."
                });
            }

            if (request.Phones is null || request.Phones.Count == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    message = "At least one phone number is required."
                });
            }

            var passwordHash = HashPassword(request.Password);

            var newEmployee = new Employee(
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                docNumber: request.DocNumber,
                birthDate: request.BirthDate,
                role: request.Role,
                passwordHash: passwordHash,
                managerId: request.ManagerId
            );

            newEmployee.Phones ??= new List<Phone>();

            foreach (var p in request.Phones)
            {
                newEmployee.Phones.Add(new Phone(p.Number, p.Type)
                {
                    EmployeeId = newEmployee.Id
                });
            }

            _db.Employees.Add(newEmployee);
            await _db.SaveChangesAsync(ct);

            var token = _jwt.GenerateToken(newEmployee);

            return StatusCode(StatusCodes.Status201Created, new
            {
                token,
                employee = new
                {
                    id = newEmployee.Id,
                    fullName = $"{newEmployee.FirstName} {newEmployee.LastName}",
                    role = newEmployee.Role.ToString(),
                    email = newEmployee.Email
                }
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
    }

    // ========= helpers internos =========

    // mesma lógica que você já usa no EmployeeController
    private Role GetCurrentUserRole()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        return Enum.TryParse<Role>(claim, out var role) ? role : Role.Employee;
    }

    // mesma lógica que você já usa no EmployeeController
    private static bool CanCreate(Role current, Role target)
        => (int)current >= (int)target;

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(password))
        );
    }
}

public record LoginRequest(
    string DocNumber,
    string Password
);
public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateTime BirthDate,
    Role Role,
    string Password,
    Guid? ManagerId,
    List<AuthPhoneRequest> Phones
);

public record AuthPhoneRequest(
    string Number,
    PhoneType Type
);
