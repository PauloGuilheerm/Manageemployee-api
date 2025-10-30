using System;
using System.Security.Claims;
using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Repositories;
using EmployeeManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly ApplicationDbContext _db;

    public EmployeeController(IEmployeeRepository repo, ApplicationDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var employees = (await _repo.GetAllAsync())
            .Select(e => new
            {
                e.Id,
                e.FirstName,
                e.LastName,
                FullName = $"{e.FirstName} {e.LastName}",
                e.Email,
                e.DocNumber,
                e.Role,
                e.BirthDate,
                Manager = e.ManagerId,
                Phones = e.Phones.Select(p => new { p.Number, p.Type })
            });

            return StatusCode(StatusCodes.Status200OK, employees);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
    }

    [HttpGet("byEmail/{email}")]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return StatusCode(StatusCodes.Status404NotFound, new { message = "User email not found in token." });

            var employee = await _repo.GetByEmailAsync(email, ct);
            if (employee is null)
                return StatusCode(StatusCodes.Status404NotFound, new { message = "Employee not found." });

            return StatusCode(StatusCodes.Status200OK, new
            {
                employee.Id,
                fullName = $"{employee.FirstName} {employee.LastName}",
                employee.Email,
                employee.DocNumber,
                employee.Role,
                employee.ManagerId,
                phones = employee.Phones.Select(p => new { p.Number, p.Type })
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var employee = await _repo.GetByIdAsync(id);
            if (employee is null)
                return NotFound();

            return StatusCode(StatusCodes.Status200OK, new
            {
                employee.Id,
                employee.FirstName,
                employee.LastName,
                employee.BirthDate,
                FullName = $"{employee.FirstName} {employee.LastName}",
                employee.Email,
                employee.DocNumber,
                employee.Role,
                Manager = employee.ManagerId,
                Phones = employee.Phones.Select(p => new { p.Number, p.Type })
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateRequest request)
    {
        try
        {
            var currentUserRole = GetCurrentUserRole();
            if (!CanCreate(currentUserRole, request.Role))
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You don't have permission to create an employee with this role."
                });

            if (await _repo.ExistsByDocNumberAsync(request.DocNumber))
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    message = "An employee with this document already exists."
                });

            if (await _repo.ExistsByEmailAsync(request.Email))
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    message = "An employee with this email already exists."
                });

            var employee = new Employee(
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                docNumber: request.DocNumber,
                birthDate: request.BirthDate,
                role: request.Role,
                passwordHash: HashPassword(request.Password),
                managerId: request.ManagerId
            );

            if (request.Phones == null || request.Phones.Count == 0)
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    message = "At least one phone number is required."
                });

            foreach (var p in request.Phones)
                employee.AddPhone(new Phone(p.Number, p.Type));

            employee.EnsureAtLeastOnePhone();

            await _repo.AddAsync(employee);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new { employee.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
    }

    [HttpPut("{employeeId:guid}")]
    public async Task<IActionResult> Update(Guid employeeId, [FromBody] EmployeeUpdateRequest request)
    {
        try
        {
            var employee = await _repo.GetByIdAsync(employeeId);
            if (employee is null)
                return NotFound();

            var currentUserRole = GetCurrentUserRole();
            if (!CanEdit(currentUserRole, employee.Role) && !request.IsOwner)
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You don't have permission to edit this employee."
                });

            employee.ChangeManager(request.ManagerId);
            if (request.NewRole.HasValue && CanCreate(currentUserRole, request.NewRole.Value))
                employee.ChangeRole(request.NewRole.Value);

            employee.ChangeBirthDate(request.BirthDate);
            employee.ChangeNames(request.FirstName, request.LastName);

            var existingPhones = _db.Phones.Where(p => p.EmployeeId == employee.Id);
            _db.Phones.RemoveRange(existingPhones);

            foreach (var phone in request.Phones)
            {
                _db.Phones.Add(new Phone(phone.Number, phone.Type)
                {
                    EmployeeId = employee.Id
                });
            }

            await _db.SaveChangesAsync();


            return StatusCode(StatusCodes.Status200OK, new
            {
                employee.Id,
                fullName = $"{employee.FirstName} {employee.LastName}",
                employee.Email,
                employee.DocNumber,
                employee.Role,
                employee.ManagerId,
                phones = employee.Phones
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var employee = await _repo.GetByIdAsync(id);
            if (employee is null)
                return NotFound();

            var currentUserRole = GetCurrentUserRole();
            if (!CanEdit(currentUserRole, employee.Role))
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You don't have permission to delete this employee."
                });

            await _repo.DeleteAsync(employee);
            return StatusCode(StatusCodes.Status204NoContent);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
    }

    private Role GetCurrentUserRole()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        return Enum.TryParse<Role>(claim, out var role) ? role : Role.Employee;
    }

    private static bool CanCreate(Role current, Role target)
        => (int)current >= (int)target;

    private static bool CanEdit(Role current, Role target)
        => (int)current >= (int)target;

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }
}

public record PhoneRequest(string Number, PhoneType Type);

public record EmployeeCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateTime BirthDate,
    Role Role,
    string Password,
    Guid? ManagerId,
    List<PhoneRequest> Phones
);

public record EmployeeUpdateRequest(
    Guid? ManagerId, 
    Role? NewRole,
    DateTime BirthDate,
    bool IsOwner,
    string FirstName,
    string LastName,
    List<PhoneRequest> Phones
    );
