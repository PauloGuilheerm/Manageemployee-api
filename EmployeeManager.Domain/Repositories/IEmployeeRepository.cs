using EmployeeManager.Domain.Entities;

namespace EmployeeManager.Domain.Repositories;

public interface IEmployeeRepository
{
    void DetachAll();
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetByDocNumberAsync(string docNumber, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByDocNumberAsync(string docNumber, CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task DeleteAsync(Employee employee, CancellationToken ct = default);

}
