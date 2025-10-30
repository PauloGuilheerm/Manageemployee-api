using EmployeeManager.Api.Services;
using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly JwtService _jwt;

    public AuthController(IEmployeeRepository repo, JwtService jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await _repo.ExistsByEmailAsync(request.Email))
            return BadRequest("Email já cadastrado.");

        var hash = HashPassword(request.Password);

        var employee = new Employee(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            docNumber: request.DocNumber,
            birthDate: request.BirthDate,
            role: request.Role,
            passwordHash: hash
        );

        employee.AddPhone(new Phone(request.PhoneNumber, request.PhoneType));
        employee.EnsureAtLeastOnePhone();

        await _repo.AddAsync(employee);

        var token = _jwt.GenerateToken(employee);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _repo.GetByDocNumberAsync(request.DocNumber);
        if (user is null)
            return Unauthorized("Usuário não encontrado.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized("Senha inválida.");

        var token = _jwt.GenerateToken(user);
        return Ok(new { token });
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var hash = HashPassword(password);
        return hash == storedHash;
    }
}

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateTime BirthDate,
    Role Role,
    string Password,
    string PhoneNumber,
    PhoneType PhoneType
);

public record LoginRequest(string DocNumber, string Password);
