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
        try
        {
            if (await _repo.ExistsByEmailAsync(request.Email))
                return StatusCode(StatusCodes.Status400BadRequest, "This email address is already in use.");

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
            return StatusCode(StatusCodes.Status200OK, new { token });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var user = await _repo.GetByDocNumberAsync(request.DocNumber);
            if (user is null)
                return StatusCode(StatusCodes.Status400BadRequest, "Usuário não encontrado.");

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return StatusCode(StatusCodes.Status400BadRequest, "Senha inválida.");

            var token = _jwt.GenerateToken(user);
            return StatusCode(StatusCodes.Status200OK, new { token });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "An unexpected error occurred. Please try again later or contact support if the problem persists."
            });
        }
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
