using EmployeeManager.Domain.Entities;
using EmployeeManager.Domain.Repositories;
using EmployeeManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void DetachAll()
    {
        foreach (var entry in _context.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }
    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        => await _context.Employees.Include(e => e.Phones).ToListAsync(ct);
    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _context.Employees
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Email == email, ct);
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context
        .Employees
        .Include(e => e.Phones)
        .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee?> GetByDocNumberAsync(string docNumber, CancellationToken ct = default)
        => await _context.Employees.FirstOrDefaultAsync(e => e.DocNumber == docNumber, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Employees.AnyAsync(e => e.Email == email, ct);

    public async Task<bool> ExistsByDocNumberAsync(string docNumber, CancellationToken ct = default)
        => await _context.Employees.AnyAsync(e => e.DocNumber == docNumber, ct);

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
    {
        await _context.Employees.AddAsync(employee, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        DetachAll();
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken ct = default)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(ct);
    }
}
