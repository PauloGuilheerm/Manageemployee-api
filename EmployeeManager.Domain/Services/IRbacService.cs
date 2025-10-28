using EmployeeManager.Domain.Enums;

namespace EmployeeManager.Domain.Services;

public interface IRbacService
{
    bool CanCreate(Role current, Role target) => (int)current >= (int)target;
    bool CanEdit(Role current, Role target) => (int)current >= (int)target;
}
