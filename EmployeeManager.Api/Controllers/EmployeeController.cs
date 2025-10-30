using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _repo;

    public EmployeeController(IEmployeeRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = (await _repo.GetAllAsync())
            .Select(e => new
            {
                e.Id,
                FullName = $"{e.FirstName} {e.LastName}",
                e.Email,
                e.DocNumber,
                e.Role,
                Manager = e.ManagerId,
                Phones = e.Phones.Select(p => new { p.Number, p.Type })
            });

        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var employee = await _repo.GetByIdAsync(id);
        if (employee is null)
            return NotFound();

        return Ok(new
        {
            employee.Id,
            FullName = $"{employee.FirstName} {employee.LastName}",
            employee.Email,
            employee.DocNumber,
            employee.Role,
            Manager = employee.ManagerId,
            Phones = employee.Phones.Select(p => new { p.Number, p.Type })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateRequest request)
    {
        var currentUserRole = GetCurrentUserRole();
        if (!CanCreate(currentUserRole, request.Role))
            return Forbid("Você não tem permissão para criar um funcionário com esse papel.");

        if (await _repo.ExistsByDocNumberAsync(request.DocNumber))
            return BadRequest("Já existe um funcionário com esse documento.");

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
            return BadRequest("Pelo menos um telefone é obrigatório.");

        foreach (var p in request.Phones)
            employee.AddPhone(new Phone(p.Number, p.Type));

        employee.EnsureAtLeastOnePhone();

        await _repo.AddAsync(employee);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new { employee.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmployeeUpdateRequest request)
    {
        var employee = await _repo.GetByIdAsync(id);
        if (employee is null)
            return NotFound();

        var currentUserRole = GetCurrentUserRole();
        if (!CanEdit(currentUserRole, employee.Role))
            return Forbid("Você não tem permissão para editar esse funcionário.");

        employee.ChangeManager(request.ManagerId);
        if (request.NewRole.HasValue && CanCreate(currentUserRole, request.NewRole.Value))
            employee.ChangeRole(request.NewRole.Value);

        await _repo.UpdateAsync(employee);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _repo.GetByIdAsync(id);
        if (employee is null)
            return NotFound();

        var currentUserRole = GetCurrentUserRole();
        if (!CanEdit(currentUserRole, employee.Role))
            return Forbid("Você não tem permissão para excluir esse funcionário.");

        await _repo.DeleteAsync(employee);
        return NoContent();
    }

    private Role GetCurrentUserRole()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
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

public record EmployeeUpdateRequest(Guid? ManagerId, Role? NewRole);
