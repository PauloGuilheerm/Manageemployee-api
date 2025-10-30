using System.Security.Cryptography;
using System.Text;
using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Enums;
using EmployeeManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace EmployeeManager.Infrastructure.Extensions;

public static class MigrationExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var hasDirector = context.Employees.Any(e => e.Role == Role.Director);
            if (!hasDirector)
            {
                var password = "Admin@123";
                var hash = Convert.ToBase64String(
                    SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password))
                );

                var director = new Employee(
                    firstName: "System",
                    lastName: "Director",
                    email: "director@company.com",
                    docNumber: "00000000000",
                    birthDate: DateTime.UtcNow.AddYears(-30),
                    role: Role.Director,
                    passwordHash: hash
                );

                director.AddPhone(new Phone("999999999", PhoneType.Mobile));
                context.Employees.Add(director);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao aplicar migrations: {ex.Message}");
        }

        return host;
    }
}
